#!/bin/sh

SB=/opt/speechbridge
SBHOME=/home/speechbridge
VDS=$SB/VoiceDocStore
SBLIC=$SB/license
SW=$SBHOME/software
BIN=$SB/bin
WEB=$SB/SBConfig
CONFIG=$SB/config
VT=/usr/vt
VTV=$VT/verify
DEFAULTNUMPORTS=64

echo "-------------------------------------------------------------------------------"
echo "- Neospeech install                                                           -"
echo "-------------------------------------------------------------------------------"
echo ""

if [ ! -e $SBLIC/verification.txt ]; then
    echo "Please copy your verification.txt file to $SBLIC/ and try again."
    echo ""
    exit 0
fi

echo "WARNING:  This install will temporarily disrupt SpeechBridge operations!  Are"
echo "you sure you want to continue?"
echo ""
echo "Type 'yes' to continue: "
read ANSWER
echo ""
if [ "$ANSWER" != "yes" ]; then
    echo "Exiting Neospeech install."
    echo ""
    exit 0
fi

echo ""
echo "How many total ports will this system run?  (Press 'Enter' if you don't know.)"
read NUMPORTS
echo ""
if [ -z "$NUMPORTS" ]; then
    echo "Please find out the number of ports and try again."
    echo ""
    exit 0
fi

echo ""
echo "Installing Neospeech..."
cp $VDS/Prompts/neospeech-ified/* $VDS/Prompts
cd $SW

tar -xzf neo-Kate*.bin.tar.gz
tar -xzf neo-vtserver*.bin.tar.gz
chmod ugo+x kate_l08_setup_engine.bin
chmod ugo+x vtserver*_setup.bin
./kate_l08_setup_engine.bin
./vtserver*_setup.bin

# Additional Neospeech configuration
mv $VTV/verification.txt $VTV/path.txt
cat $VTV/path.txt >> $SBLIC/verification.txt
ln -s $SBLIC/verification.txt $VTV/verification.txt

# If the num ports is greater than the default (64), change the setting.
# If we were to always set it (when lower), we would have to remember to change it when increasing the number of ports in the system, so it is safer to leave it alone.
if [ "$NUMPORTS" -gt "$DEFAULTNUMPORTS" ]; then
    mv -f /etc/ttssrv.ini /etc/ttssrv_BAK.ini
    sed "s/MaxChannel	64/MaxChannel	$NUMPORTS/g" /etc/ttssrv_BAK.ini > /etc/ttssrv.ini
fi

# Speechbridge configuration
cp $BIN/AudioMgr.exe.config $BIN/AudioMgr.exe_BAK.config
sed 's/CepstralCmd/NeospeechCmd/g' $BIN/AudioMgr.exe_BAK.config > $BIN/AudioMgr.exe.config

cp $WEB/Web.config $WEB/Web_BAK.config
sed 's/CepstralCmd/NeospeechCmd/g' $WEB/Web_BAK.config > $WEB/Web.config

# Set up daemon script
ln -s $BIN/neospeechd /etc/init.d/neospeechd
chkconfig --add neospeechd
chkconfig neospeechd on

# Restart services
service neospeechd start
service httpd reload
service speechbridged restart
