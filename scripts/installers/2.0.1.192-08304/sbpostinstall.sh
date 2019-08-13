#####################################################################################
# sbpostinstall.sh
#
# Revision info:
# - 9/102008 BDA - Moved all posgtresql operations from sbinstall.sh to here.
#####################################################################################
#!/bin/sh

SB=/opt/speechbridge
SBHOME=/home/speechbridge
SW=$SBHOME/software
BIN=$SB/bin
CONFIG=$SB/config
LOG=$SB/logs/sbinstall.log

LVSRE_DIR=/opt/lumenvox/engine
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$LVSRE_DIR/lib


echo "-------------------------------------------------------------------------------"
echo "- SpeechBridge post-install                                                   -"
echo "-------------------------------------------------------------------------------"

echo ""
echo "Generating LumenVox Info.bts..."
/opt/lumenvox/licenseserver/bin/getlvsystem_info >> $LOG 2>> $LOG
mv -f /opt/lumenvox/licenseserver/bin/Info.bts /home/speechbridge >> $LOG 2>> $LOG

echo ""
echo "Configuring PostgreSql..."
chkconfig postgresql on >> $LOG 2>> $LOG
service postgresql start >> $LOG 2>> $LOG

usermod -G speechbridge postgres >> $LOG 2>> $LOG

rm -f /var/lib/pgsql/data/pg_hba.conf >> $LOG 2>> $LOG
cp -f $CONFIG/pg_hba.conf /var/lib/pgsql/data >> $LOG 2>> $LOG
chown postgres:postgres /var/lib/pgsql/data/pg_hba.conf >> $LOG 2>> $LOG
chmod u+rw /var/lib/pgsql/data/pg_hba.conf >> $LOG 2>> $LOG
chown postgres:postgres $CONFIG/SBPreCreate_pgsql.sql >> $LOG 2>> $LOG
chown postgres:postgres $CONFIG/SBCreate_pgsql.sql >> $LOG 2>> $LOG
service postgresql stop >> $LOG 2>> $LOG
service postgresql start >> $LOG 2>> $LOG

echo ""
echo "Configuring PostgreSql database..."
echo ""

cp -f /etc/sudoers /etc/sudoers.1 2>> $LOG
sed 's/Defaults    requiretty/#Defaults    requiretty/g' /etc/sudoers.1 > /etc/sudoers 2>> $LOG
sudo -u postgres psql -f $CONFIG/SBPreCreate_pgsql.sql >> $LOG 2>> $LOG
cp -f /etc/sudoers.1 /etc/sudoers 2>> $LOG
rm -f /etc/sudoers.1 2>> $LOG

mono $BIN/sbdbutils.exe --run-script $CONFIG/SBCreate_pgsql.sql >> $LOG 2>> $LOG
mono $BIN/sbdbutils.exe --reset-admin >> $LOG 2>> $LOG

echo "-------------------------------------------------------------------------------"
echo "- SpeechBridge post-install now complete.  You now need to configure your     -"
echo "- ASR and TTS licenses.  You will need to reboot the system after completing  -"
echo "- these final tasks.                                                          -"
echo "-------------------------------------------------------------------------------"
echo ""
