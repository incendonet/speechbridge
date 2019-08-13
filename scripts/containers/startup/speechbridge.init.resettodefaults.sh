#!/bin/bash

###############################################################################
# speechbridge.init.resettodefaults.sh
#
# Resets the settings to defaults.  Assumes system-wide environment vars have
# been set, and DB has already been created.
###############################################################################

CONTAINERSBHOME=/opt/speechbridge
SBBIN=$CONTAINERSBHOME/bin
SERVICEDIR=/usr/local/opt/incendonet/services/speechbridge

CONFIG_FILE="$SERVICEDIR/speechbridge.init.resettodefaults.conf"
SQL_FILE="${SERVICEDIR}/resettodefaults.sql"

RETRYSECONDS=1

# Read in config settings
[ -r "$CONFIG_FILE" ] && . "$CONFIG_FILE"

# Wait for postgres to be ready
until docker-compose run --rm sbdotnet bash -c "PGPASSWORD=$POSTGRES_SBPASSWORD psql -h \"postgres\" -d \"dspeechbridge\" -U \"$POSTGRES_SBUSER\" -c '\l' > /dev/null"; do
    >&2 echo "Postgres is unavailable - retry in ${RETRYSECONDS} sec"
    sleep "${RETRYSECONDS}"
done

# Set values
echo "UPDATE tblConfigParams SET sValue='$SIP_PROXY_ADDR' WHERE sName='SipProxy'" > "${SQL_FILE}"
echo "UPDATE tblConfigParams SET sValue='$SIP_FIRST_SB_EXT' WHERE sName='SipFirstExt'" >> "${SQL_FILE}"
echo "UPDATE tblConfigParams SET sValue='$SIP_SB_PASS' WHERE sName='SipPassword'" >> "${SQL_FILE}"
echo "UPDATE tblConfigParams SET sValue='$SIP_NUM_PORTS' WHERE sName='SipNumExt'" >> "${SQL_FILE}"
echo "UPDATE tblConfigParams SET sValue='$SIP_FIRST_SB_SIP_PORT' WHERE sName='FirstLocalSipPort'" >> "${SQL_FILE}"
echo "UPDATE tblConfigParams SET sValue='$RTP_FIRST_PORT' WHERE sName='RtpPortMin'" >> "${SQL_FILE}"
echo "UPDATE tblConfigParams SET sValue='$RTP_LAST_PORT' WHERE sName='RtpPortMax'" >> "${SQL_FILE}"
echo "UPDATE tblConfigParams SET sValue='$AUDIOMGR_ADDR' WHERE sName='AudioMgrIp'" >> "${SQL_FILE}"
echo "UPDATE tblConfigParams SET sValue='$PBX_TYPE' WHERE sName='IppbxType'" >> "${SQL_FILE}"
docker-compose run --rm sbdotnet bash -c "mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-sqlscript-commands \"{SQL_FILE}\""

# Generate config files from DB
docker-compose run --rm sbdotnet bash -c "mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --write-configs-sip"
