#!/bin/sh

SBHOME=/home/speechbridge
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin

HTTPDCONF=/etc/httpd/conf

#---------------------------------------------------------------------------------------------------------
#- Mono update
#---------------------------------------------------------------------------------------------------------

echo Stopping services and cleaning up old Mono files...
service speechbridged stop
service httpd stop
ps -C mono -eo pid,args | grep "mod-mono-server" | awk '{ print $1}' | xargs kill -9
#ps -C mono -eo pid,args | grep "mod-mono-server2.exe" | awk '{ print $1}' | xargs kill -9
rm -Rf /tmp/apache*
rm -Rf /tmp/root*
rm -Rf /tmp/mod_mono*
rm -Rf /tmp/.wapi*
rm -Rf /root/.wapi*

echo Removing old Mono...
rpm -ev mod_mono.i386 mono-core.i586 mono-data.i586 mono-data-postgresql.i586 mono-data-sqlite.i586 mono-nunit.i586 mono-web.i586 mono-winforms.i586 mono-core.i386 xsp.i386 libgdiplus.i386

echo Creating new links...
ln -s /opt/speechbridge/bin/nspring.dll /opt/speechbridge/bin/NSpring.dll
ln -s /opt/speechbridge/bin/nspring.dll /opt/speechbridge/SBConfig/bin/NSpring.dll

echo Changing permissions...
chmod ugo=rx  /opt/speechbridge
chmod ugo=rx  /opt/speechbridge/bin
chmod  ug=r   /opt/speechbridge/bin/*.exe
chmod  ug=rw  /opt/speechbridge/bin/*.dll
chmod   o=r   /opt/speechbridge/bin/*.dll
chmod ugo=rwx /opt/speechbridge/config
chmod ugo=rw  /opt/speechbridge/config/*.cfg
chmod ugo=rw  /opt/speechbridge/config/ProxySrv.config
chmod -R ugo=r /opt/speechbridge/SBConfig
chmod ugo=rx  /opt/speechbridge/SBConfig
chmod ugo=rx  /opt/speechbridge/SBConfig/assets
chmod ugo=rx  /opt/speechbridge/SBConfig/assets/content
chmod ugo=rx  /opt/speechbridge/SBConfig/assets/images
chmod ugo=rx  /opt/speechbridge/SBConfig/bin
chmod  ug=rx  /opt/speechbridge/logs
chmod ugo=rx  /opt/speechbridge/VoiceDocStore
chmod ugo=r   /opt/speechbridge/VoiceDocStore/*_Template.vxml.xml
chmod ugo=rw  /opt/speechbridge/VoiceDocStore/SBRoot.vxml.xml
chmod ugo=rw  /opt/speechbridge/VoiceDocStore/AAMain.vxml.xml
chmod ugo=rw  /opt/speechbridge/VoiceDocStore/EmailMain.vxml.xml
chmod ugo=rw  /opt/speechbridge/VoiceDocStore/CalendarMain.vxml.xml
chmod ugo=rwx /opt/speechbridge/VoiceDocStore/Prompts
chmod ugo=rw  /opt/speechbridge/VoiceDocStore/Prompts/AHGreeting2.wav
chmod ugo=rw  /opt/speechbridge/VoiceDocStore/Prompts/PleaseSayTheName.wav
chmod ugo=rw  /opt/speechbridge/VoiceDocStore/Prompts/ThankYouForCalling.wav
chmod ugo=rwx /opt/speechbridge/VoiceDocStore/Prompts/Names
chown -R speechbridge:speechbridge /opt/speechbridge

echo CDing to Mono 2.6.4 dir...
cd $SBHOME/software/mono/2.6.4

echo Backing up old httpd.conf, copying new...
mv /etc/httpd/conf/httpd.conf /etc/httpd/conf/httpd_BAK.conf
cp -f $SBHOME/software/mono/2.6.4/httpd-mono-2-6.conf /etc/httpd/conf/httpd.conf

echo Installing Mono 2.6.4 build dependencies...
tar -xzf mono_2-6-4_builddeps.tar.gz
cd deps
#rpm -Uvh cairo-1.2.4-5.el5.i386.rpm cairo-devel-1.2.4-5.el5.i386.rpm fontconfig-devel-2.4.1-7.el5.i386.rpm freetype-2.2.1-21.el5_3.i386.rpm freetype-devel-2.2.1-21.el5_3.i386.rpm giflib-4.1.3-7.1.el5_3.1.i386.rpm giflib-devel-4.1.3-7.1.el5_3.1.i386.rpm libexif-0.6.13-4.0.2.el5_1.1.i386.rpm libexif-devel-0.6.13-4.0.2.el5_1.1.i386.rpm libjpeg-devel-6b-37.i386.rpm libpng-1.2.10-7.1.el5_3.2.i386.rpm libpng-devel-1.2.10-7.1.el5_3.2.i386.rpm libtiff-3.8.2-7.el5_2.2.i386.rpm libtiff-devel-3.8.2-7.el5_2.2.i386.rpm libX11-1.0.3-11.el5.i386.rpm libX11-devel-1.0.3-11.el5.i386.rpm libXau-devel-1.0.1-3.1.i386.rpm libXdmcp-devel-1.0.1-2.1.i386.rpm libXrender-devel-0.9.1-3.1.i386.rpm mesa-libGL-6.5.1-7.7.el5.i386.rpm mesa-libGL-devel-6.5.1-7.7.el5.i386.rpm xorg-x11-proto-devel-7.1-13.el5.centos.i386.rpm httpd-devel-2.2.3-31.el5.centos.1.i386.rpm apr-util-1.2.7-7.el5.i386.rpm httpd-2.2.3-31.el5.centos.1.i386.rpm httpd-manual-2.2.3-31.el5.centos.1.i386.rpm mod_ssl-2.2.3-31.el5.centos.1.i386.rpm apr-devel-1.2.7-11.i386.rpm apr-util-devel-1.2.7-7.el5.i386.rpm
rpm -Uvh \
apr-1.2.7-11.el5_3.1.i386.rpm \
apr-devel-1.2.7-11.el5_3.1.i386.rpm \
apr-util-1.2.7-7.el5_3.2.i386.rpm \
apr-util-devel-1.2.7-7.el5_3.2.i386.rpm \
cairo-1.2.4-5.el5.i386.rpm \
cairo-devel-1.2.4-5.el5.i386.rpm \
fontconfig-devel-2.4.1-7.el5.i386.rpm \
freetype-2.2.1-21.el5_3.i386.rpm \
freetype-devel-2.2.1-21.el5_3.i386.rpm \
giflib-4.1.3-7.1.el5_3.1.i386.rpm \
giflib-devel-4.1.3-7.1.el5_3.1.i386.rpm \
httpd-2.2.3-31.el5.centos.i386.rpm \
httpd-devel-2.2.3-31.el5.centos.i386.rpm \
httpd-manual-2.2.3-31.el5.centos.i386.rpm \
libexif-0.6.13-4.0.2.el5_1.1.i386.rpm \
libexif-devel-0.6.13-4.0.2.el5_1.1.i386.rpm \
libjpeg-devel-6b-37.i386.rpm \
libpng-1.2.10-7.1.el5_3.2.i386.rpm \
libpng-devel-1.2.10-7.1.el5_3.2.i386.rpm \
libtiff-3.8.2-7.el5_3.4.i386.rpm \
libtiff-devel-3.8.2-7.el5_3.4.i386.rpm \
libX11-1.0.3-11.el5.i386.rpm \
libX11-devel-1.0.3-11.el5.i386.rpm \
libXau-devel-1.0.1-3.1.i386.rpm \
libXdmcp-devel-1.0.1-2.1.i386.rpm \
libXext-devel-1.0.1-2.1.i386.rpm \
libXft-devel-2.1.10-1.1.i386.rpm \
libXrender-devel-0.9.1-3.1.i386.rpm \
mesa-libGL-6.5.1-7.7.el5.i386.rpm \
mesa-libGL-devel-6.5.1-7.7.el5.i386.rpm \
mod_ssl-2.2.3-31.el5.centos.i386.rpm \
pango-1.14.9-6.el5.centos.i386.rpm \
pango-devel-1.14.9-6.el5.centos.i386.rpm \
xorg-x11-proto-devel-7.1-13.el5.i386.rpm


echo Installing Mono 2.6.4...
#rpm -ivh libgdiplus-2.4-5.i386.rpm mod_mono-2.4-5.i386.rpm mono-core-2.4-17.i386.rpm mono-data-2.4-17.i386.rpm mono-data-postgresql-2.4-17.i386.rpm mono-data-sqlite-2.4-17.i386.rpm monodoc-2.4-17.i386.rpm mono-extras-2.4-17.i386.rpm mono-jscript-2.4-17.i386.rpm mono-nunit-2.4-17.i386.rpm mono-web-2.4-17.i386.rpm mono-winforms-2.4-17.i386.rpm xsp-2.4-8.i386.rpm
cd ..
tar -xzf mono_2-6-4_builddir.tar.gz
cd libgdiplus-2.6.4
make install
cd ../mono-2.6.4
make install
cd ../xsp-2.6.4
make install
cd ../mod_mono-2.6.3
make install

mv /etc/httpd/conf/mod_mono.conf /etc/httpd/conf.d/mod_mono.conf

#---------------------------------------------------------------------------------------------------------
#- SpeechBridge update
#---------------------------------------------------------------------------------------------------------
SBHOME=/home/speechbridge
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin

echo Updating Database...
echo Running DB upgrade script...
mono sbdbutils --run-script $SBHOME/software/sbupdate_2-4-0/SBUpdate_2-4-0_pgsql.sql

echo Setting collaboration server in DB...
sCfgEmail=`grep EmailServer /opt/speechbridge/bin/DialogMgr.exe.config | cut -d\" -f4`
if [ "$sCfgEmail" = "Exchange2003" ]
then
    sDbEmail="Microsoft Exchange 2003"
elif [ "$sCfgEmail" = "Exchange2007" ]
then
    sDbEmail="Microsoft Exchange 2007"
else
    sDbEmail="[none]"
fi
echo "UPDATE tblConfigParams SET sValue = \'$sDbEmail\' WHERE sComponent=\'Collaboration:0000\'\;" > collab0.sql
mono sbdbutils --run-script collab0.sql
rm collab0.sql

echo Installing report CRON job and Apache settings...
mkdir $SBBASE/SBReports
chown speechbridge:speechbridge $SBBASE/SBReports
chmod ugo+rx $SBBASE/SBReports
ln -s $SBBIN/sbreportgen.cron /etc/cron.daily/sbreportgen.cron

ln -s $SBBASE/SBReports /var/www/html/SBReports
cp $HTTPDCONF/httpd.conf $HTTPDCONF/httpd.conf.sb_3-0-0.bak
echo "" >> $HTTPDCONF/httpd.conf
echo "" >> $HTTPDCONF/httpd.conf
echo "#SpeechBridge reports" >> $HTTPDCONF/httpd.conf
echo "<Directory \"/var/www/html/VoiceDocStore\">" >> $HTTPDCONF/httpd.conf
echo "        AllowOverride None" >> $HTTPDCONF/httpd.conf
echo "        Order deny,allow" >> $HTTPDCONF/httpd.conf
echo "#        deny from all" >> $HTTPDCONF/httpd.conf
echo "        Allow from all" >> $HTTPDCONF/httpd.conf
echo "</Directory>" >> $HTTPDCONF/httpd.conf
echo "" >> $HTTPDCONF/httpd.conf
echo "" >> $HTTPDCONF/httpd.conf

echo Copying new SpeechBridge assemblies and files...
# copy old files to backup dir
# copy new files
# set permissions

echo Restarting services...
service httpd start
service speechbridged start

echo Done!
echo

#---------------------------------------------------------------------------------------------------------
#- SpeechBridge update uninstall
#---------------------------------------------------------------------------------------------------------
