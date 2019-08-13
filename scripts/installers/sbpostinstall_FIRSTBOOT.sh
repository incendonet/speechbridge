#####################################################################################
# sbpostinstall_FIRSTBOOT.sh
#
# Revision info:
# - 9/10/2008 BDA - Moved all posgtresql operations from sbinstall.sh to here.
# - 20100304  BDA - Added installing MR RPMs
# - 20101027  BDA - Removed unnecessary steps for "reconfigSys" after 3.0 update.
# - 20141003  BDA - Updated for 6.0.1
#####################################################################################
# This script will be run at the tail end of the installation process by the first-	#
# boot wizard, and later be overwritten by a more minimalist copy to be run when	#
# the system is re-initialized while booting up after a `touch /etc/reconfigSys`	#
# command has been issued to set the system (primarily the DB) back to a fresh		#
# state, but software doesn't need to be installed again (in fact, that could cause	#
# problems.)  This is because the FBW doesn't know the difference between the first	#
#installation and subsequent reinitializations.  In both cases, the FBW just runs	#
# `/home/speechbridge/software/sbpostinstall.sh`, so we have to make sure that the	#
# correct one is in place.															#
#####################################################################################
#!/bin/sh

SB=/opt/speechbridge
VDS=$SB/VoiceDocStore
PROMPTS=$VDS/Prompts
SBHOME=/home/speechbridge
SW=$SBHOME/software
BIN=$SB/bin
CONFIG=$SB/config
LOGDIR=$SB/logs
LOG=$LOGDIR/sbinstall.log

LVSRE_DIR=/opt/lumenvox/engine
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$LVSRE_DIR/lib


mkdir -p $LOGDIR

echo "-------------------------------------------------------------------------------"
echo "- SpeechBridge post-install                                                   -"
echo "-------------------------------------------------------------------------------"
echo "-------------------------------------------------------------------------------" >> $LOG
echo "- SpeechBridge post-install                                                   -" >> $LOG
echo "- `date`                                                -" >> $LOG
echo "-------------------------------------------------------------------------------" >> $LOG

#echo ""
echo "Generating LumenVox Info.bts..."
/opt/lumenvox/licenseserver/bin/getlvsystem_info >> $LOG 2>> $LOG
mv -f /opt/lumenvox/licenseserver/bin/Info.bts /home/speechbridge >> $LOG 2>> $LOG

#echo ""
echo "Configuring PostgreSql..."
chkconfig postgresql on >> $LOG 2>> $LOG
service postgresql start >> $LOG 2>> $LOG

usermod -G speechbridge postgres >> $LOG 2>> $LOG

#####################################################################################
# The commands in this section is done in sbinstall_4-0-1-0.rpm, left here for		#
# documentation (i.e., for the benefit of future installers)						#
#####################################################################################
#rm -f /var/lib/pgsql/data/pg_hba.conf >> $LOG 2>> $LOG
#cp -f $CONFIG/pg_hba.conf /var/lib/pgsql/data >> $LOG 2>> $LOG
#chown postgres:postgres /var/lib/pgsql/data/pg_hba.conf >> $LOG 2>> $LOG
#chmod u+rw /var/lib/pgsql/data/pg_hba.conf >> $LOG 2>> $LOG
#chown postgres:postgres $CONFIG/SBPreCreate_pgsql.sql >> $LOG 2>> $LOG
#chown postgres:postgres $CONFIG/SBCreate_pgsql.sql >> $LOG 2>> $LOG
#service postgresql stop >> $LOG 2>> $LOG
#service postgresql start >> $LOG 2>> $LOG
#
#echo ""
#echo "Configuring PostgreSql database..."
#echo ""
#echo "" >> $LOG
#echo "Configuring PostgreSql database..." >> $LOG
#echo "" >> $LOG
#
#cp -f /etc/sudoers /etc/sudoers.1 2>> $LOG
#sed 's/Defaults    requiretty/#Defaults    requiretty/g' /etc/sudoers.1 > /etc/sudoers 2>> $LOG
#sudo -u postgres psql -f $CONFIG/SB_pgsql.sql >> $LOG 2>> $LOG
#cp -f /etc/sudoers.1 /etc/sudoers 2>> $LOG
#rm -f /etc/sudoers.1 2>> $LOG
#################################################################################

echo ""
echo "Install SpeechBridge RPM..."
echo ""
echo "" >> $LOG
echo "Install SpeechBridge RPM..." >> $LOG
echo "" >> $LOG
rpm -ivh $SW/sbinstall-4.0.1-0.i686.rpm >> $LOG 2>> $LOG

echo "4.0.1" > $SB/build.txt

echo ""
echo "Resetting SpeechBridge database..."
echo ""
echo "" >> $LOG
echo "Resetting SpeechBridge database..." >> $LOG

#/opt/novell/mono/bin/mono $BIN/sbdbutils.exe --run-script $CONFIG/SBCreate_pgsql.sql >> $LOG 2>> $LOG		# This is done in sbinstall-4.0.1-0.i686.rpm
/opt/novell/mono/bin/mono $BIN/sbdbutils.exe --reset-admin >> $LOG 2>> $LOG

echo ""                                                 >> $LOG
echo "Updating prompts..."                              >> $LOG
tar -xjf $SW/misc/NewPrompts.tar.bz2 -C $PROMPTS      >> $LOG 2>> $LOG
chown speechbridge:speechbridge $PROMPTS/*.wav        >> $LOG 2>> $LOG
chmod ugo+r $PROMPTS/*.wav                            >> $LOG 2>> $LOG

echo ""                                                 >> $LOG
echo "Installing latest Maintenance Releases..."        >> $LOG
#echo ""
#echo "Installing latest Maintenance Releases..."
#echo ""

tar -xjf $SW/speechbridge_4-1-1_bin.tar.bz2 -C $SW     >> $LOG 2>> $LOG
tar -xjf $SW/speechbridge_4-2-1_bin.tar.bz2 -C $SW     >> $LOG 2>> $LOG
tar -xjf $SW/speechbridge_5-0-1_bin.tar.bz2 -C $SW     >> $LOG 2>> $LOG
tar -xjf $SW/speechbridge_6-4-1_bin.tar.bz2 -C $SW     >> $LOG 2>> $LOG
$SW/speechbridge_4-1-1.bin --silent                     >> $LOG 2>> $LOG
$SW/speechbridge_4-2-1.bin --silent                     >> $LOG 2>> $LOG
$SW/speechbridge_5-0-1.bin --silent                     >> $LOG 2>> $LOG
$SW/speechbridge_6-4-1.bin --silent                     >> $LOG 2>> $LOG

echo ""
echo "Configuring desktop and restarting GDM..."
echo ""
echo ""                                                 >> $LOG
echo "Configuring desktop and restarting GDM..."        >> $LOG

cp -f $SW/misc/_etc_gdm_custom.conf /etc/gdm/custom.conf             >> $LOG 2>> $LOG
cp -f $SW/misc/Speechbridge*.png /usr/share/backgrounds/images       >> $LOG 2>> $LOG
tar -xzf $SW/misc/gnomefiles_c56.tar.gz -C /root                     >> $LOG 2>> $LOG

/usr/sbin/gdm-restart >> $LOG 2>> $LOG


echo "-------------------------------------------------------------------------------"
echo "- SpeechBridge post-install now complete.                                     -"
echo "-------------------------------------------------------------------------------"
echo ""
echo "-------------------------------------------------------------------------------" >> $LOG
echo "- SpeechBridge post-install now complete.                                     -" >> $LOG
echo "-------------------------------------------------------------------------------" >> $LOG
echo "" >> $LOG
