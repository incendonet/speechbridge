#####################################################################################
# Copyright 2012, Incendonet Inc.
#
# 4.1.1		- 10/24/2012 BDA - Updates TTS in 4.1.1 SpeechBridge Pro upgrades
#------------------------------------------------------------------------------------
# NOTE:  This will only set up the Kate voice.  If there are any others installed,
# please contact support@incendonet.com for assistance enabling the others.
#####################################################################################
#!/bin/sh

THISFILE="sb411upprotts.sh"

TODAYSTR="$(/bin/date --date=today +%Y%m%d)"
SBHOME=/home/speechbridge
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBCONF=$SBCONF/config
SBLOGS=$SBBASE/logs
WEB=$SBBASE/SBConfig
NEOLICFILE=/usr/vt/verify/verification.txt
NEODFILE=$SBBIN/neospeechd

# If we were passed a log filename, use it, otherwise use a default.
if [ $# -gt 0 ]; then
    LOGFILE=$1
else
    LOGFILE="$SBLOGS/$THISFILE"_"$TODAYSTR.log"
fi

echo ""                                                                        >> $LOGFILE
echo "Starting $THISFILE..."                                                   >> $LOGFILE

# Move neoinstall.sh to the correct location (/home/speechbridge/software)
#echo "Moving neoinstall.sh..."                                                 >> $LOGFILE
#mv -f $SBHOME/software/neoinstall.sh $SBHOME/software/neoinstall.sh.BAK        >> $LOGFILE 2>> $LOGFILE
#mv -f $SBCONF/neoinstall.sh $SBHOME/software                                   >> $LOGFILE 2>> $LOGFILE

# Check if Neospeech is already installed (does /usr/vt/verify/verification.txt exist)
if [ ! -e $NEOLICFILE ]; then
    echo "Neospeech is not installed, nothing to upgrade."                     >> $LOGFILE
    exit 0
else
    echo "Neospeech is installed, starting the upgrade..."                     >> $LOGFILE
fi

# Check if verification.txt needs updating (does it include a "Path" line)
bHASPATH=`grep Path $NEOLICFILE | wc -l`
if [ $bHASPATH -eq 0 ]; then
    echo "Adding path to verification.txt..."                                  >> $LOGFILE
    echo "" >> /usr/vt/verify/verification.txt
    echo "" >> /usr/vt/verify/verification.txt
    echo "Path100:/usr/vt/kate/L08/" >> /usr/vt/verify/verification.txt
    echo "" >> /usr/vt/verify/verification.txt
else
    echo "Path already in verification.txt..."                                 >> $LOGFILE
fi

#echo "Fixing EOLs in neospeechd..."      # This should have been fixed in the RPM
#cd $SBBIN
#dos2unix < neospeechd > neospeechd.unix
#mv -f neospeechd neospeechd.OLD
#mv -f neospeechd.unix neospeechd

echo "Installing neospeechd..."                                                >> $LOGFILE
chmod ug+x $NEODFILE                                                           >> $LOGFILE 2>> $LOGFILE
ln -s $NEODFILE /etc/init.d/neospeechd                                         >> $LOGFILE 2>> $LOGFILE
chkconfig --add neospeechd                                                     >> $LOGFILE 2>> $LOGFILE
chkconfig neospeechd on                                                        >> $LOGFILE 2>> $LOGFILE
service neospeechd start                                                       >> $LOGFILE 2>> $LOGFILE

echo "Fixing references to NeospeechCmd_L08..."                                >> $LOGFILE
cp $SBBIN/AudioMgr.exe.config $SBBIN/AudioMgr.exe_BAK.config                   >> $LOGFILE 2>> $LOGFILE
sed 's/NeospeechCmd_L08/NeospeechCmd/g' $SBBIN/AudioMgr.exe_BAK.config > $SBBIN/AudioMgr.exe.config          2>> $LOGFILE

cp $WEB/Web.config $WEB/Web_BAK.config                                         >> $LOGFILE 2>> $LOGFILE
sed 's/NeospeechCmd_L08/NeospeechCmd/g' $WEB/Web_BAK.config > $WEB/Web.config               2>> $LOGFILE


#echo "Restarting SpeechBridge..."        # This should be done in the calling script
#service httpd reload
#sservice speechbridged restart

echo "Finished $THISFILE."                                                   >> $LOGFILE
echo ""                                                                        >> $LOGFILE

exit 0
