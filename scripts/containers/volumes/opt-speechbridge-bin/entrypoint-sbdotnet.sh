#!/bin/bash

set -e

LOCALETC=/usr/local/etc
MONOBIN=/usr/bin/mono
SBHOME=/opt/speechbridge
SBBIN="${SBHOME}/bin"
SBCFG="${SBHOME}/config"
SBTEMPL="${SBHOME}/templates"

LVCONF=/etc/lumenvox

# If LV services are DNS-resolvable, use those over the passed-in addresses
set +e
ping -c 1 lvsre > /dev/null
if [ "$?" -eq "0" ] ; then
  export LVSRESRV=`host -t A lvsre | awk '{print $NF}' | tr -d [:space:]`
fi
ping -c 1 lvlicense > /dev/null
if [ $? -eq "0" ] ; then
  export LVLICSRV=`host -t A lvlicense | awk '{print $NF}' | tr -d [:space:]`
fi
set -e

# Generate config files with environment variable replacement
${SBBIN}/envsub.sh < ${SBTEMPL}/SBLocalRM.exe.config.TEMPLATE > ${SBCFG}/SBLocalRM.exe.config
${SBBIN}/envsub.sh < ${SBTEMPL}/AudioMgr.exe.config.TEMPLATE > ${SBCFG}/AudioMgr.exe.config
${SBBIN}/envsub.sh < ${SBTEMPL}/DialogMgr.exe.config.TEMPLATE > ${SBCFG}/DialogMgr.exe.config
${SBBIN}/envsub.sh < ${SBTEMPL}/sbdbutils.exe.config.TEMPLATE > ${SBCFG}/sbdbutils.exe.config
${SBBIN}/envsub.sh < ${SBTEMPL}/SBSched.exe.config.TEMPLATE > ${SBCFG}/SBSched.exe.config
${SBBIN}/envsub.sh < ${LOCALETC}/client_property.conf.TEMPLATE > ${LVCONF}/client_property.conf

# Wait for Postgres to be ready
until PGPASSWORD=${POSTGRES_SBPASSWORD} psql -h "postgres" -d "dspeechbridge" -U "${POSTGRES_SBUSER}" -c '\l' > /dev/null; do
  >&2 echo "Postgres is unavailable - sleeping..."
  sleep 1
done
echo "Postgres is up."

# Save postgres IP address in file (so SIP components on host network can find postgres on the bridged network)
PGIP=`nslookup postgres | awk '/^Address: / { print $2 }'`
echo '#!/bin/bash' > ${SBCFG}/postgres_ip.sh
echo "POSTGRES_IP=${PGIP}" >> ${SBCFG}/postgres_ip.sh

# Set IP addresses in DB
#   Host
PGPASSWORD=${POSTGRES_SBPASSWORD} psql -h postgres -d ${POSTGRES_SBDB} -U ${POSTGRES_SBUSER} -c "UPDATE tblConfigParams SET sValue='${SIPPROXYADDR}' WHERE sName='SipProxy'" -t
#   Self
IPADDR=`hostname -I`
PGPASSWORD=${POSTGRES_SBPASSWORD} psql -h postgres -d ${POSTGRES_SBDB} -U ${POSTGRES_SBUSER} -c "UPDATE tblConfigParams SET sValue='${IPADDR}' WHERE sName='AudioMgrIp'" -t

# Save SIP config files
${MONOBIN} --config ${SBCFG}/sbdbutils.exe.config ${SBBIN}/sbdbutils.exe --write-configs-sip

# Generate VoiceXML
${MONOBIN} --config ${SBCFG}/sbdbutils.exe.config ${SBBIN}/sbdbutils.exe --generate-vxml

# Start SB
${MONOBIN} --config ${SBCFG}/SBLocalRM.exe.config ${SBBIN}/SBLocalRM.exe --console

# Will we get here when container is stopped?
# Clear IP address field
PGPASSWORD=${POSTGRES_SBPASSWORD} psql -h postgres -d ${POSTGRES_SBDB} -U ${POSTGRES_SBUSER} -c "UPDATE tblConfigParams SET sValue='' WHERE sName='AudioMgrIp'" -t
