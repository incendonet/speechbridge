#!/bin/sh

SB=/opt/speechbridge
SBHOME=/home/speechbridge
SW=$SBHOME/software
BIN=$SB/bin
WEB=$SB/SBConfig
CONFIG=$SB/config

echo "-------------------------------------------------------------------------------"
echo "- Neospeech install                                                           -"
echo "-------------------------------------------------------------------------------"
echo ""
echo "WARNING:  This install will temporarily disrupt SpeechBridge operations!  Are"
echo "you sure you want to continue?"
echo ""
echo "Type 'yes' to continue: "
read answer
echo ""
if [ "x$answer" != "xyes" ]; then
    echo "Exiting Neospeech install."
    echo ""
    exit 0
fi

echo ""
echo "Installing Neospeech..."
cp /opt/speechbridge/VoiceDocStore/Prompts/neospeech-ified/* /opt/speechbridge/VoiceDocStore/Prompts
cd $SW
gunzip kate_l08_setup_engine.bin.gz
gunzip vtserver37_setup.bin.gz
chmod ugo+x kate_l08_setup_engine.bin
chmod ugo+x vtserver37_setup.bin
./kate_l08_setup_engine.bin
./vtserver37_setup.bin

cp $BIN/AudioMgr.exe.config $BIN/AudioMgr.exe_BAK.config
sed 's/CepstralCmd/NeospeechCmd_L08/g' $BIN/AudioMgr.exe_BAK.config > $BIN/AudioMgr.exe.config

cp $WEB/Web.config $WEB/Web_BAK.config
sed 's/CepstralCmd/NeospeechCmd_L08/g' $WEB/Web_BAK.config > $WEB/Web.config

service httpd reload
service speechbridged restart
