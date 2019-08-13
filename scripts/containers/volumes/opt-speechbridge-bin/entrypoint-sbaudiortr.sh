#!/bin/bash
# Expected args:
#   $1 - Most likely "AudioRtr" or "AudioRtr ProxySrv" 

set -e

LIB=/usr/lib
SBHOME=/opt/speechbridge
SBBIN=${SBHOME}/bin
SBCFG=${SBHOME}/config
SBLOG=${SBHOME}/logs

# Wait for Postgres to be ready
until [ -e ${SBCFG}/postgres_ip.sh ]; do
  >&2 echo "Waiting for postgres_ip.sh to be written - sleeping..."
  sleep 1
done

source ${SBCFG}/postgres_ip.sh
until PGPASSWORD=${POSTGRES_SBPASSWORD} psql -h "${POSTGRES_IP}" -d "dspeechbridge" -U "${POSTGRES_SBUSER}" -c '\l' > /dev/null; do
  >&2 echo "Postgres is unavailable - sleeping..."
  sleep 1
  source ${SBCFG}/postgres_ip.sh
done
echo "Postgres is up."

# Wait for config file
until [ -e ${SBCFG}/0.cfg ]; do
  >&2 echo "Waiting for 0.cfg to be written - sleeping..."
  sleep 1
done

# Get the number of ports from the DB  (The iconv step )
TEMPSTR=$(PGPASSWORD=${POSTGRES_SBPASSWORD} psql -h "${POSTGRES_IP}" -d ${POSTGRES_SBDB} -U ${POSTGRES_SBUSER} -c "select sValue from tblConfigParams where sName='SipNumExt'" -t | iconv -f utf-8 -t us-ascii//TRANSLIT)
NUM_SIP_PORTS=$(echo ${TEMPSTR} | iconv -f UTF8 -t ASCII//TRANSLIT)

# Start SB SIP components.
# 'num_sip_port' (all upper case) below should be replaced by entrypoint.sbdontnet.sh
# Args:  $1 is the apps to start (only "AudioRtr" at this time)
${SBBIN}/SBLauncher ${NUM_SIP_PORTS} ${SBBIN} ${SBCFG} ${SBLOG} "$1"
