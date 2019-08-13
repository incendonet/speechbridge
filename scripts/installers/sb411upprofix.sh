#####################################################################################
# Copyright 2012, Incendonet Inc.
#
# 4.1.1		- 10/23/2012 BDA - Fixes TTS in 4.1.1 SpeechBridge Pro upgrades
#------------------------------------------------------------------------------------
# NOTE:  This will only set up the Kate voice.  If there are any others installed,
# please contact support@incendonet.com for assistance enabling the others.
#####################################################################################
#!/bin/sh

SBHOME=/home/speechbridge
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBCONF=$SBCONF/config
SBLOGS=$SBBASE/logs
WEB=$SBBASE/SBConfig

echo "#####################################################################################"
echo "This script will fix the TTS problem after a SpeechBridge Pro upgrade to 4.1.1"
echo "NOTE:  This will only set up the American English female voice.  If there are any"
echo "others installed, please contact support@incendonet.com for assistance enabling them."
echo "WARNING:  This install will temporarily disrupt SpeechBridge operations!  Are"
echo "you sure you want to continue?"
echo ""
echo "Type 'yes' to continue: "
read ANSWER
echo ""
if [ "$ANSWER" != "yes" ]; then
    echo "Aborting sb411upprofix.sh, try again when you're ready to proceed."
    echo "#####################################################################################"
    echo ""
    exit 0
fi

echo "Adding path to verification.txt..."
echo "" >> /usr/vt/verify/verification.txt
echo "" >> /usr/vt/verify/verification.txt
echo "Path100:/usr/vt/kate/L08/" >> /usr/vt/verify/verification.txt
echo "" >> /usr/vt/verify/verification.txt

echo "Fixing EOLs in neospeechd..."
cd $SBBIN
dos2unix < neospeechd > neospeechd.unix
mv -f neospeechd neospeechd.OLD
mv -f neospeechd.unix neospeechd

echo "Installing neospeechd..."
chmod ug+x neospeechd
ln -s /opt/speechbridge/bin/neospeechd /etc/init.d/neospeechd
chkconfig --add neospeechd
chkconfig neospeechd on
service neospeechd start

echo "Fixing references to NeospeechCmd_L08..."
cp $SBBIN/AudioMgr.exe.config $SBBIN/AudioMgr.exe_BAK.config
sed 's/NeospeechCmd_L08/NeospeechCmd/g' $SBBIN/AudioMgr.exe_BAK.config > $SBBIN/AudioMgr.exe.config

cp $WEB/Web.config $WEB/Web_BAK.config
sed 's/NeospeechCmd_L08/NeospeechCmd/g' $WEB/Web_BAK.config > $WEB/Web.config

echo ""
echo "Almost done!  If there were any errors printed out ABOVE this line, please email the"
echo "message or a picture of the screen to support@incendonet.com.  If you see any errors"
echo "below this point, you can safely ignore them."
echo "#####################################################################################"
echo ""

echo "Restarting SpeechBridge..."
service httpd reload
service speechbridged restart

echo "Finished!"
echo "#####################################################################################"
echo ""
