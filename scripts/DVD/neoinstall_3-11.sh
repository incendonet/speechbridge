#!/bin/sh

SB=/opt/speechbridge
SBHOME=/home/speechbridge
SW=$SBHOME/software
BIN=$SB/bin
WEB=$SB/SBConfig
CONFIG=$SB/config
VT=/usr/vt
VTV=$VT/verify
DEFAULTNUMPORTS=64

service neospeechd stop

if [ -e /usr/vt ]; then
    echo "-------------------------------------------------------------------------------"
    echo "- Uninstalling previous Neospeech install                                     -"
    echo "-------------------------------------------------------------------------------"
    echo ""
    cd /usr/vt/bin
    sh ./uninstall
    cd /usr/vt/kate/L08/bin
    sh ./uninstall
fi

echo "-------------------------------------------------------------------------------"
echo "- Neospeech install                                                           -"
echo "-------------------------------------------------------------------------------"
echo ""

if [ ! -e /home/speechbridge/verification.txt ]; then
    echo "Please copy your verification.txt file to /home/speechbridge and try again."
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
cd $SW

if [ ! -e neo-vtserver*.bin.tar.gz ]; then
    echo ""
    echo "Install error:  Neospeech server not found, exiting now."
    echo ""
    exit -2
fi

if [ ! -e neo-Ashley*.gz ]; then
	echo ""
	echo "Neospeech Ashley voice not found, skipping."
	echo ""
else
	tar -xzf neo-Ashley*.bin.tar.gz
	tar -xzf neo-vtserver*.bin.tar.gz
	chmod ugo+x ashley*.bin
	chmod ugo+x vtserver*_setup.bin
	./ashley*.bin
	./vtserver*_setup.bin
fi
if [ ! -e neo-Julie*.gz ]; then
	echo ""
	echo "Neospeech Julie voice not found, skipping."
	echo ""
else
	tar -xzf neo-Julie*.bin.tar.gz
	tar -xzf neo-vtserver*.bin.tar.gz
	chmod ugo+x julie*.bin
	chmod ugo+x vtserver*_setup.bin
	./julie*.bin
	./vtserver*_setup.bin
fi
if [ ! -e neo-Kate*.gz ]; then
	echo ""
	echo "Neospeech Kate voice not found, skipping."
	echo ""
else
	tar -xzf neo-Kate*.bin.tar.gz
	tar -xzf neo-vtserver*.bin.tar.gz
	chmod ugo+x kate*.bin
	chmod ugo+x vtserver*_setup.bin
	./kate*.bin
	./vtserver*_setup.bin
fi

# Additional Neospeech configuration
mv $VTV/verification.txt $VTV/path.txt
mv -f $SBHOME/verification.txt $VTV/verification.txt
cat $VTV/path.txt >> $VTV/verification.txt

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
ln -s /opt/speechbridge/bin/neospeechd /etc/init.d/neospeechd
chkconfig --add neospeechd
chkconfig neospeechd on

# Restart services
service neospeechd start
service httpd reload
service speechbridged restart
