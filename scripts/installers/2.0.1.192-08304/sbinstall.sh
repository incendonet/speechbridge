#####################################################################################
# 2.0.1.192 - 7/15/2008 21:07
#           - 9/10/2008 BDA - Moved all postgresql related setup to sbpostinstall.sh
#           - 9/17/2008 BDA - Moved Neospeech install to neoinstall.sh
#
# SpeechBridge Manual Installation Instructions for CentOS-5
#------------------------------------------------------------------------------------
#- Install CentOS-5.1 with the provided anaconda-ks.cfg
#- Follow network setup instructions from admin guide
#- Download the LumenVox, Cepstral, Neospeech, and Mono packages to a temporary directory
#  (For example, /home/speechbridge/software)
#- Copy speechbridge.tar.gz to /opt and "tar -xzf' it.
#- 
#####################################################################################
#!/bin/sh

SB=/opt/speechbridge
SBHOME=/home/speechbridge
SW=$SBHOME/software
BIN=$SB/bin
CONFIG=$SB/config

LVSRE_DIR=/opt/lumenvox/engine
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$LVSRE_DIR/lib

if [ `whoami` != "root" ]; then
    echo "You must be logged in as root to install SpeechBridge.  Now exiting."
    exit 0
fi

echo "-------------------------------------------------------------------------------"
echo "- SpeechBridge install (generic SIP)                                          -"
echo "-------------------------------------------------------------------------------"
#echo ""
#echo "Have you completed a full 'yum update', copied all packages to /home/speechbridge/software"
#echo "and configured the network setting according to the admin guide?"
#echo "Type 'yes' to continue: "
#read answer
#echo ""
#if [ "x$answer" != "xyes" ]; then
#    echo "Please do so, then re-run this script."
#    exit 0
#fi

echo ""
echo "Configuring CentOS services..."
chkconfig bluetooth off
chkconfig cups off
chkconfig pcscd off
chkconfig sendmail off
chkconfig httpd on
#chkconfig postgresql on
chkconfig yum-updatesd off
service bluetooth stop
service cups stop
service pcscd stop
service sendmail stop
service yum-updatesd stop
service httpd start
#service postgresql start

chmod ugo-rwx /etc/cron.daily/makewhatis.cron
chmod ugo-rwx /etc/cron.weekly/makewhatis.cron

echo ""
echo "Configuring firstboot..."
mkdir -p /usr/share/firstboot/modules/BAK
mv /usr/share/firstboot/modules/* /usr/share/firstboot/modules/BAK
mv /usr/share/firstboot/modules/BAK/welcome.py /usr/share/firstboot/modules
mv /usr/share/firstboot/modules/BAK/rootpassword.py /usr/share/firstboot/modules
#mv /usr/share/firstboot/modules/BAK/networking.py /usr/share/firstboot/modules		# We have our own custom version that gets moved over below.
mv /usr/share/firstboot/modules/BAK/timezone.py /usr/share/firstboot/modules
mv /usr/share/firstboot/modules/BAK/date.py /usr/share/firstboot/modules
mv $SW/firstbootWindow.py /usr/share/firstboot
mv $SW/networking.py /usr/share/firstboot/modules
mv $SW/sbcomplete.py /usr/share/firstboot/modules
cp -f $SW/*.png /usr/share/firstboot/pixmaps
touch /etc/reconfigSys

echo ""
echo "Configuring SpeechBridge users and groups..."
groupadd -f speechbridge
useradd --create-home -n -p speechbridge -r -g speechbridge speechbridge
usermod -G speechbridge apache
#postusermod -G speechbridge postgres

echo ""
echo "Setting up Speechbridge files..."
cd /opt
cp -f $SW/speechbridge_c5.tar.gz .
tar -xzf speechbridge_c5.tar.gz
mkdir -p $SB/logs
mkdir -p $SB/VoiceDocStore/Prompts/Names
chown -R speechbridge:speechbridge $SB
chmod -R ug+rw $SB
chmod -R o-rwx $SB
cp -f $SB/SBConfig/index-rootdir.html /var/www/html/index.html
chown speechbridge:speechbridge /var/www/html/index.html
chmod ug+r /var/www/html/index.html

#echo ""
#echo "Configuring PostgreSql..."
#rm -f /var/lib/pgsql/data/pg_hba.conf
#cp -f $CONFIG/pg_hba.conf /var/lib/pgsql/data
#chown postgres:postgres /var/lib/pgsql/data/pg_hba.conf
#chmod u+rw /var/lib/pgsql/data/pg_hba.conf
#chown postgres:postgres $CONFIG/SBPreCreate_pgsql.sql
#chown postgres:postgres $CONFIG/SBCreate_pgsql.sql
#service postgresql stop
#service postgresql start

echo ""
echo "Configuring Apache..."
rm -f /etc/httpd/conf/httpd.conf
cp -f $CONFIG/httpd.conf /etc/httpd/conf

ln -s $SB/SBConfig /var/www/html/SBConfig
ln -s $SB/VoiceDocStore /var/www/html/VoiceDocStore

chmod ug+x $BIN/speechbridgemon.cron
ln -s $BIN/speechbridgemon.cron /etc/cron.hourly/speechbridgemon.cron

echo ""
echo "Installing Mono..."
rpm -ivh $SW/libgdiplus-1.2.5-1.el5.centos.i386.rpm $SW/mod_mono-1.2.1-1.el5.centos.i386.rpm $SW/mono-core-1.2.6-4.novell.i586.rpm $SW/mono-data-1.2.6-4.novell.i586.rpm $SW/mono-data-postgresql-1.2.6-4.novell.i586.rpm $SW/mono-nunit-1.2.6-4.novell.i586.rpm $SW/mono-data-sqlite-1.2.6-4.novell.i586.rpm $SW/mono-web-1.2.6-4.novell.i586.rpm $SW/mono-winforms-1.2.6-4.novell.i586.rpm $SW/xsp-1.2.1-1.el5.centos.i386.rpm
ln -s /usr/lib/libgdiplus.so.0.0.0 /usr/lib/libgdiplus.so

service httpd restart

echo ""
echo "Installing SpeechBridge daemon..."

ln -s $BIN/speechbridged /etc/init.d
chkconfig speechbridged --add
chkconfig speechbridged on

echo ""
echo "Installing LumenVox..."
rpm -i $SW/LumenVoxLicenseServer-8.0-106.el5.i386.rpm
rpm -i $SW/LumenVoxSRE-8.0-106.el5.i386.rpm

echo ""
echo "Do you want to install the Cepstral TTS?"
echo "Type 'yes' to continue: "
read answer
echo ""
if [ "x$answer" = "xyes" ]; then
    echo "Installing Cepstral..."
    cp /opt/speechbridge/VoiceDocStore/Prompts/cepstral-ified/* /opt/speechbridge/VoiceDocStore/Prompts
    cd $SW
    tar -xzf $SW/Cepstral_Callie-8kHz_i386-linux_4.2.1.tar.gz
    cd Cepstral_Callie-8kHz_i386-linux_4.2.1
    ./install.sh
    cp $CONFIG/cepstral.conf /etc/ld.so.conf.d
    ldconfig
    cd $SW
fi

#echo ""
#echo "Do you want to install the Neospeech TTS?"
#echo "Type 'yes' to continue: "
#read answer
#echo ""
#if [ "x$answer" = "xyes" ]; then
#    echo "Installing Neospeech..."
#    cp /opt/speechbridge/VoiceDocStore/Prompts/neospeech-ified/* /opt/speechbridge/VoiceDocStore/Prompts
#    cd $SW
#    gunzip kate_l08_setup_engine.bin.gz
#    gunzip vtserver37_setup.bin.gz
#    chmod ugo+x kate_l08_setup_engine.bin
#    chmod ugo+x vtserver37_setup.bin
#    ./kate_l08_setup_engine.bin
#    ./vtserver37_setup.bin
#fi


#echo ""
#echo "Configuring PostgreSql database..."
#echo ""
#sudo -u postgres psql -f $CONFIG/SBPreCreate_pgsql.sql
#mono $BIN/sbdbutils.exe --run-script $CONFIG/SBCreate_pgsql.sql
#mono $BIN/sbdbutils.exe --reset-admin

#####################################################################################
#####################################################################################
# Manual steps:
# Install LumenVox license
# Install TTS license

echo ""
echo "-------------------------------------------------------------------------------"
echo "- SpeechBridge initial setup now complete.  Press <Alt-F1> to reboot the      -"
echo "- system.  The system will then walk you through setting some system settings,-"
echo "- and you will then install your licenses.                                    -"
echo "-------------------------------------------------------------------------------"
echo ""

#shutdown -r now
#####################################################################################

#####################################################################################
# Optional steps:
#- Change "CKey" value to reflect a new 32-byte key in: /opt/speechbridge/bin/DialogMgr.exe.config,
#  /opt/speechbridge/bin/SBSched.exe.config, /opt/speechbridge/SBConfig/Web.config
#  (All three must match.)
#- Change sbuser1 postgresql username and/or password.  The sql install script and
#  SQL connect string in all .config files must be updated.
#- Disable makewhatis.cron, prelink from cron.daily, cron.weekly
#- Create and install certificate for Apache
#- Additional Apache configuration (disabling root page, etc.)
#- Configure firewall to only allow: 443:tcp, 5060:udp, 5062-5065:udp, 10000-10007:udp
