#####################################################################################
# sbpostinstall_POSTFIRSTBOOT.sh
#
# Revision info:
# - 9/10/2008 BDA - Moved all posgtresql operations from sbinstall.sh to here.
# - 20100304  BDA - Added installing MR RPMs
# - 20101027  BDA - Removed unnecessary steps for "reconfigSys" after 3.0 update.
# - 20140806  BDA - Updated for 6.0.1
# - 20150917  BDA - Added regeneration of LumenVox's Info.bts
#####################################################################################
# This script will be run at the tail end of the first-boot wizard during a system	#
# reinitialization that was run when the system boots up following a				#
# `touch /etc/reconfigSys` command.  This file should be renamed to					#
# `/home/speechbridge/software/sbpostinstall.sh` so that it is run by the FBW.		#
#####################################################################################
#!/bin/sh

SB=/opt/speechbridge
SBHOME=/home/speechbridge
SW=$SBHOME/software
BIN=$SB/bin
CONFIG=$SB/config
LOGDIR=$SB/logs
LOG=$LOGDIR/sbinstall.log
TMPLANGS=/tmp/sblangs.txt

echo "-------------------------------------------------------------------------------"
echo "- SpeechBridge post-install                                                   -"
echo "-------------------------------------------------------------------------------"
echo "-------------------------------------------------------------------------------"				>> $LOG
echo "- SpeechBridge post-install                                                   -"				>> $LOG
echo "- `date`                                                -"									>> $LOG
echo "-------------------------------------------------------------------------------"				>> $LOG

echo "Generating Info.bts..."																		>> $LOG
LVSRE_DIR=/opt/lumenvox/engine
LVLSBIN_DIR=/opt/lumenvox/licenseserver/bin
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$LVSRE_DIR/lib
$LVLSBIN_DIR/getlvsystem_info																			>> $LOG 2>> $LOG
mv -f $LVLSBIN_DIR/Info.bts /home/speechbridge														>> $LOG 2>> $LOG
cp -f /home/speechbridge/Info.bts $SB/license															>> $LOG 2>> $LOG

echo ""
echo "Resetting SpeechBridge database..."
echo ""
echo ""																								>> $LOG
echo "Resetting SpeechBridge database..."															>> $LOG
echo ""																								>> $LOG

# Save list of currently installed languages so they can be reinstalled if this is run from `touch /etc/reconfigSys`
/opt/novell/mono/bin/mono $BIN/sbdbutils.exe --get-langs		> $TMPLANGS							2>> $LOG

# Initialize the DB
/opt/novell/mono/bin/mono $BIN/sbdbutils.exe --run-sqlscript-commands $CONFIG/SBCreate_pgsql.sql				>> $LOG 2>> $LOG
/opt/novell/mono/bin/mono $BIN/sbdbutils.exe --reset-admin											>> $LOG 2>> $LOG

# Install the appropriate languages
if grep "en-US" $TMPLANGS > /dev/null; then
	echo "Found en-US language pack, reinitializing..."												>> $LOG
	/opt/novell/mono/bin/mono $BIN/sbdbutils.exe --run-sqlscript-commands $CONFIG/sblp_en-US_pgsql.sql			>> $LOG 2>> $LOG
fi
if grep "en-AU" $TMPLANGS > /dev/null; then
	echo "Found en-AU language pack, reinitializing..."												>> $LOG
	/opt/novell/mono/bin/mono $BIN/sbdbutils.exe --run-sqlscript-commands $CONFIG/sblp_en-AU_pgsql.sql			>> $LOG 2>> $LOG
fi
if grep "es-MX" $TMPLANGS > /dev/null; then
	echo "Found es-MX language pack, reinitializing..."												>> $LOG
	/opt/novell/mono/bin/mono $BIN/sbdbutils.exe --run-sqlscript-commands $CONFIG/sblp_es-MX_pgsql.sql			>> $LOG 2>> $LOG
fi

rm -f $TMPLANGS																						>> $LOG 2>> $LOG

# Restart services that may be sensitive to the IP address changing, since we've been called the first-boot wizard
service lumenvoxlm restart
sleep 10s
service lvdaemon restart
sleep 25s
service httpd restart			# Restarting Apache doesn't seem to be necessary, but it can't hurt
service speechbridged restart

echo "-------------------------------------------------------------------------------"
echo "- SpeechBridge post-install now complete.                                     -"
echo "-------------------------------------------------------------------------------"
echo ""
echo "-------------------------------------------------------------------------------"				>> $LOG
echo "- SpeechBridge post-install now complete.                                     -"				>> $LOG
echo "-------------------------------------------------------------------------------"				>> $LOG
echo ""																								>> $LOG
