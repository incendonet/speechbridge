#!/bin/sh

# tar with:    tar -cPf foo.tar fullpath
# untar with:  tr -xPf foo.tar

#---------------------------------------------------------------------------------------------------------
# Installer script
#---------------------------------------------------------------------------------------------------------
SBHOME=/home/speechbridge
SBINST=$SBHOME/software/sbupdate-3-0-1
SBMONO=$SBHOME/software/mono/2.6.4
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBLOGS=$SBBASE/logs
SBDOCS=$SBBASE/VoiceDocStore
LOGFILE=$SBLOGS/sbupdate_3-0-1.log
HTTPDCONF=/etc/httpd/conf

echo --------------------------------------------

if [ `whoami` != "root" ]; then
    echo "You must be logged in as root to install SpeechBridge.  Please log in as root (or sudo) and run this script again."
    echo --------------------------------------------
    echo
    exit 0
fi

echo - Do you wish to upgrade Mono?  (Mono 2.6.4 is
echo - required for the SpeechBridge upgrade.  You must
echo - upgrade Mono before upgrading SpeechBridge.)
echo - THIS WILL DISRUPT SPEECHBRIDGE OPERATIONS!!!
echo - (Including calls and web access.)
echo
echo - Type 'yes' to continue:
read answer
echo
if [ "x$answer" = "xyes" ]; then
	cd $SBMONO

    echo Removing old Mono...
    rpm -ev mod_mono.i386 mono-core.i586 mono-data.i586 mono-data-postgresql.i586 mono-data-sqlite.i586 mono-nunit.i586 mono-web.i586 mono-winforms.i586 mono-core.i386 xsp.i386 libgdiplus.i386

    echo Installing Mono 2.6.4 build dependencies...
    #tar -xzf mono_2-6-4_builddeps.tar.gz
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

    cd ..
	
    echo Stopping services and cleaning up old Mono files...
    service speechbridged stop
    service httpd stop
    ps -C mono -eo pid,args | grep "mod-mono-server" | awk '{ print $1}' | xargs kill -9
    rm -Rf /tmp/apache*
    rm -Rf /tmp/root*
    rm -Rf /tmp/mod_mono*
    rm -Rf /tmp/.wapi*
    rm -Rf /root/.wapi*

    echo Creating new links...
    ln -fs $SBBIN/nspring.dll $SBBIN/NSpring.dll
    ln -fs $SBBIN/nspring.dll $SBBASE/SBConfig/bin/NSpring.dll

    echo Changing permissions...
    chmod ugo=rx  $SBBASE
    chmod ugo=rx  $SBBASE/bin
    chmod  ug=r   $SBBASE/bin/*.exe
    chmod  ug=rw  $SBBASE/bin/*.dll
    chmod   o=r   $SBBASE/bin/*.dll
    chmod ugo=rwx $SBBASE/config
    chmod ugo=rw  $SBBASE/config/*.cfg
    chmod ugo=rw  $SBBASE/config/ProxySrv.config
    chmod -R ugo=r $SBBASE/SBConfig
    chmod ugo=rx  $SBBASE/SBConfig
    chmod ugo=rx  $SBBASE/SBConfig/assets
    chmod ugo=rx  $SBBASE/SBConfig/assets/content
    chmod ugo=rx  $SBBASE/SBConfig/assets/images
    chmod ugo=rx  $SBBASE/SBConfig/bin
    chmod  ug=rx  $SBLOGS
    chmod ugo=rx  $SBBASE/VoiceDocStore
    chmod ugo=r   $SBBASE/VoiceDocStore/*_Template.vxml.xml
    chmod ugo=rw  $SBBASE/VoiceDocStore/SBRoot.vxml.xml
    chmod ugo=rw  $SBBASE/VoiceDocStore/AAMain.vxml.xml
    chmod ugo=rw  $SBBASE/VoiceDocStore/EmailMain.vxml.xml
    chmod ugo=rw  $SBBASE/VoiceDocStore/CalendarMain.vxml.xml
    chmod ugo=rwx $SBBASE/VoiceDocStore/Prompts
    chmod ugo=rw  $SBBASE/VoiceDocStore/Prompts/AHGreeting2.wav
    chmod ugo=rw  $SBBASE/VoiceDocStore/Prompts/PleaseSayTheName.wav
    chmod ugo=rw  $SBBASE/VoiceDocStore/Prompts/ThankYouForCalling.wav
    chmod ugo=rwx $SBBASE/VoiceDocStore/Prompts/Names
    chown -R speechbridge:speechbridge $SBBASE

    echo Backing up old Apache conf files, copying new...
    mv -f $HTTPDCONF/httpd.conf $HTTPDCONF/httpd_BAK.conf
    cp -f $SBHOME/software/mono/2.6.4/httpd-mono-2-6.conf $HTTPDCONF/httpd.conf

    echo CDing to Mono 2.6.4 dir...
    cd $SBHOME/software/mono/2.6.4

    echo Installing Mono 2.6.4...
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

    mv -f /etc/httpd/conf/mod_mono.conf /etc/httpd/conf.d/mod_mono.conf
else
    echo Skipping Mono upgrade.
    echo
fi

echo --------------------------------------------
echo - About to begin SpeechBridge upgrade.
echo - THIS WILL DISRUPT SPEECHBRIDGE OPERATIONS!!!
echo - (Including calls and web access.)
echo
echo - Type 'yes' to continue:
read answer
echo
if [ "x$answer" = "xyes" ]; then
    rpm -Uvh sbupdate_3-0-1.rpm
fi

echo --------------------------------------------
echo


#---------------------------------------------------------------------------------------------------------
#- SpeechBridge update
#---------------------------------------------------------------------------------------------------------
SBHOME=/home/speechbridge
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin

HTTPDCONF=/etc/httpd/conf
HTTPDCONFD=/etc/httpd/conf.d

echo Backing up prior version...
# Copy to bak

echo Prepping binary files...
# Copy Mono bins

# Redo permissions changes?

# Create new AudioRtr link
rm $SBBIN/AudioRtr
ln -fs $SBBIN/AudioRtr_C5_20100607 $SBBIN/AudioRtr

echo Updating Database...
echo Running DB upgrade script...
mono sbdbutils --run-script $SBHOME/software/sbupdate_3-0/SBUpdate_3-0_pgsql.sql

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
ln -fs $SBBIN/sbreportgen.cron /etc/cron.daily/sbreportgen.cron

ln -fs $SBBASE/SBReports /var/www/html/SBReports
cp $HTTPDCONF/httpd.conf $HTTPDCONF/httpd.conf.sb_3-0-1.bak
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

echo Copying UTF8 mod_mono.conf
mv -f $HTTPDCONFD/mod_mono.conf $HTTPDCONFd/mod_mono.conf.sb_3-0-1.bak
cp $SBHOME/software/mono/2.6.4/mod_mono-2-6-UTF8.conf $HTTPDCONFd/mod_mono-2-6-UTF8.conf.sb_3-0-1.bak


echo Restarting services...
service httpd start
service speechbridged start

echo --------------------------------------------
echo "There have been a number of improvements to the"
echo "default VoiceXML templates, but if yours have been"
echo "customized and you wish to take advantage of these"
echo "improvements, you will have to redo your changes."
echo "(All your files were backed up earlier in the"
echo "update process, so you won't lose anything.)"
echo
echo "Do you want to upgrade your VoiceXML templates?"
echo "- Type 'yes' to continue:"
read answer
echo
if [ "x$answer" = "xyes" ]; then
    mv -f $SBINST/VoiceDocStore/AAMain_Template.vxml.xml $SBDOCS/AAMain_Template.vxml.xml
	echo
    echo "Template updated.  Remember to click the 'Generate"
	echo "VoiceCML' button in the web admin for these changes"
    echo "to take effect."
else
    echo Skipping VoiceXML template update.
    echo
fi
echo

echo --------------------------------------------
echo Done!
echo
