#!/bin/sh

# tar with:    tar -cPf foo.tar fullpath
# untar with:  tr -xPf foo.tar

#---------------------------------------------------------------------------------------------------------
# Installer script
#---------------------------------------------------------------------------------------------------------
SBHOME=/home/speechbridge
SBINST=$SBHOME/software/sbupdate-3-0-1
SBMONO=$SBHOME/software/mono/2.6.7
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBLOGS=$SBBASE/logs
SBVDOCS=$SBBASE/VoiceDocStore
LOGFILE=$SBLOGS/sbupdate_3-0-1.log
HTTPDCONF=/etc/httpd/conf
SILENT="--silent"

INSTALLMODE="$1"

if [ `whoami` != "root" ]; then
    echo "You must be logged in as root to upgrade SpeechBridge.  Please log in as root (or sudo) and run this script again."
    echo "----------------------------------------------"
    echo
    exit 0
fi

if [ $INSTALLMODE != $SILENT ]; then
    echo "----------------------------------------------"
    echo "You are about to upgrade SpeechBridge to"
    echo "version 3.0.1, but this will disrupt all calls"
    echo "and web access.  Do you want to continue the"
    echo "installation?"
    echo ""
    echo "WARNING:"
    echo "THIS WILL DISRUPT SPEECHBRIDGE OPERATIONS!!!"
    echo ""
    echo "Type 'yes' (without the quotes) to continue:"
    read answer1
    echo
fi

if [ "x$answer1" = "xyes"  -o  $INSTALLMODE = $SILENT ]; then
# Move RPMs where we want them
    mkdir -p $SBMONO
    mv -f mod_mono-addon-2.6.3-6.4.i386.rpm $SBMONO
    mv -f mono-addon-core-2.6.7-6.1.i386.rpm $SBMONO
    mv -f mono-addon-data-2.6.7-6.1.i386.rpm $SBMONO
    mv -f mono-addon-data-postgresql-2.6.7-6.1.i386.rpm $SBMONO
    mv -f mono-addon-data-sqlite-2.6.7-6.1.i386.rpm $SBMONO
    mv -f mono-addon-extras-2.6.7-6.1.i386.rpm $SBMONO
    mv -f mono-addon-libgdiplus0-2.6.7-6.1.i386.rpm $SBMONO
    mv -f mono-addon-nunit-2.6.7-6.1.i386.rpm $SBMONO
    mv -f mono-addon-wcf-2.6.7-6.1.i386.rpm $SBMONO
    mv -f mono-addon-web-2.6.7-6.1.i386.rpm $SBMONO
    mv -f mono-addon-winforms-2.6.7-6.1.i386.rpm $SBMONO
    mv -f mono-addon-xsp-2.6.5-4.6.noarch.rpm $SBMONO
    mkdir -p $SBINST
    mv -f sbupdate-3.0.1-*.i686.rpm $SBINST

    cd $SBMONO

    echo "Removing old Mono..."
    rpm -ev mod_mono.i386 mono-core.i586 mono-data.i586 mono-data-postgresql.i586 mono-data-sqlite.i586 mono-nunit.i586 mono-web.i586 mono-winforms.i586 mono-core.i386 xsp.i386 libgdiplus.i386

    cd ..
	
    echo "Stopping services and cleaning up old Mono files..."
    service speechbridged stop
    service httpd stop
    ps -C mono -eo pid,args | grep "mod-mono-server" | awk '{ print $1}' | xargs kill -9
    rm -Rf /tmp/apache*
    rm -Rf /tmp/root*
    rm -Rf /tmp/mod_mono*
    rm -Rf /tmp/.wapi*
    rm -Rf /root/.wapi*

    echo "Creating new links..."
    ln -fs $SBBIN/nspring.dll $SBBIN/NSpring.dll
    ln -fs $SBBIN/nspring.dll $SBBASE/SBConfig/bin/NSpring.dll

    echo "Changing permissions..."
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

#    echo "Backing up old Apache conf files, copying new..."
#    mv -f $HTTPDCONF/httpd.conf $HTTPDCONF/httpd_BAK.conf
#    cp -f $SBHOME/software/mono/2.6.7/httpd-mono-2-6.conf $HTTPDCONF/httpd.conf

    echo "CDing to Mono 2.6.7 dir..."
    cd $SBHOME/software/mono/2.6.7

    echo "Installing Mono 2.6.7 RPMs..."
    rpm -ivh \
        mod_mono-addon-2.6.3-6.4.i386.rpm \
        mono-addon-core-2.6.7-6.1.i386.rpm \
        mono-addon-data-2.6.7-6.1.i386.rpm \
        mono-addon-data-postgresql-2.6.7-6.1.i386.rpm \
        mono-addon-data-sqlite-2.6.7-6.1.i386.rpm \
        mono-addon-extras-2.6.7-6.1.i386.rpm \
        mono-addon-libgdiplus0-2.6.7-6.1.i386.rpm \
        mono-addon-nunit-2.6.7-6.1.i386.rpm \
        mono-addon-wcf-2.6.7-6.1.i386.rpm \
        mono-addon-web-2.6.7-6.1.i386.rpm \
        mono-addon-winforms-2.6.7-6.1.i386.rpm \
        mono-addon-xsp-2.6.5-4.6.noarch.rpm

    export PATH=$PATH:/opt/novell/mono/bin
    #/etc/profile is updated in sbupdate-3.0.1-X.rpm

    echo "Starting Apache..."
    service httpd start

    echo "----------------------------------------------"
    echo "About to begin SpeechBridge upgrade..."
    rpm -ivh --force $SBINST/sbupdate-3.0.1-*.i686.rpm
	echo ""

    if [ $INSTALLMODE != $SILENT ]; then
        echo "----------------------------------------------"
        echo "There have been a number of improvements to the"
        echo "default VoiceXML templates, but if yours have been"
        echo "customized and you wish to take advantage of these"
        echo "improvements, you will have to redo your changes."
        echo "(All your files were backed up earlier in the"
        echo "update process, so you won't lose anything.)"
        echo
        echo "Do you want to upgrade your VoiceXML templates?"
        echo "Type 'yes' (without the quotes) to continue:"
        read answer2
        echo ""
    fi
    if [ "x$answer2" = "xyes"  -o  $INSTALLMODE = $SILENT ]; then
        mv -f $SBVDOCS/AAMain_Template.vxml.xml.up $SBVDOCS/AAMain_Template.vxml.xml
        mv -f $SBVDOCS/EmailMain_Template.vxml.xml.up $SBVDOCS/EmailMain_Template.vxml.xml
        mv -f $SBVDOCS/CalendarMain_Template.vxml.xml.up $SBVDOCS/CalendarMain_Template.vxml.xml
        echo
        echo "Template updated.  Remember to click the 'Generate"
        echo "VoiceXML' button in the web admin for these changes"
        echo "to take effect."
    else
        echo "Skipping VoiceXML template update."
        echo ""
    fi
    echo

    echo "----------------------------------------------"
    echo "Done!"
    echo "----------------------------------------------"
    echo ""
else
    echo "- Skipping SpeechBridge upgrade."
    echo
fi
