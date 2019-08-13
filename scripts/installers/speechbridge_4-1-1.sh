#!/bin/sh

# tar with:    tar -cPf foo.tar fullpath
# untar with:  tr -xPf foo.tar

#---------------------------------------------------------------------------------------------------------
# Installer script
#---------------------------------------------------------------------------------------------------------
THISVER="4.1.1"
SBHOME=/home/speechbridge
SBINST=$SBHOME/software/speechbridge-$THISVER
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBCONF=$SBCONF/config
SBLOGS=$SBBASE/logs
SBVDOCS=$SBBASE/VoiceDocStore
LOGFILE=$SBLOGS/speechbridge_$THISVER.log
HTTPDCONF=/etc/httpd/conf
MONOBIN=/opt/novell/mono/bin
SILENT="--silent"
UPGRADEABLE_VER="4.0.1.157"
#LAST_BUILD=`tail -n 1 $SBBASE/build.txt`
#LAST_BUILD=`$MONOBIN/mono --config $SBCONF/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver`
LAST_BUILD=`$MONOBIN/mono --config $SBCONF/sbdbutils.exe.config ./sbdbutils.exe --get-swver SpeechBridge`
INSTALLMODE="$1"

Cleanup()
{
    rm -f sbdbutils.exe
    rm -f sbdbutils.exe.config
    rm -f SBConfigStor.dll
}

if [ `whoami` != "root" ]; then
	echo "----------------------------------------------"
    echo "You must be logged in as root to upgrade SpeechBridge.  Please log in as root (or sudo) and run this script again."
    echo "----------------------------------------------"
    echo
    Cleanup
    exit 0
fi

# Check dependencies
if [ "$UPGRADEABLE_VER" != "$LAST_BUILD" ]; then
	echo "----------------------------------------------"
	echo "Your system must be running SpeechBridge '$UPGRADEABLE_VER' to install this update."
	echo "This system's version is '$LAST_BUILD'."
    echo "You can find your version number on the SpeechBridge login web page."
    echo "Aborting the upgrade now."
	echo "----------------------------------------------"
	echo
    Cleanup
	exit 0
fi

# Issue warning if we are not running in 'silent' mode
if [ $INSTALLMODE != $SILENT ]; then
    echo "----------------------------------------------"
    echo "You are about to upgrade SpeechBridge to"
    echo "version $THISVER, and this will disrupt all calls"
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
    mkdir -p $SBINST
    mv -f speechbridge-$THISVER-*.i686.rpm $SBINST

    echo "----------------------------------------------"
    echo "About to begin SpeechBridge upgrade..."
    rpm -ivh --force $SBINST/speechbridge-$THISVER-*.i686.rpm
    #rpm -Uvh $SBINST/speechbridge-$THISVER-*.i686.rpm
	echo ""

#    if [ $INSTALLMODE != $SILENT ]; then
#        echo "----------------------------------------------"
#        echo "There have been a number of improvements to the"
#        echo "default VoiceXML templates, but if yours have been"
#        echo "customized and you wish to take advantage of these"
#        echo "improvements, you will have to redo your changes."
#        echo "(All your files were backed up earlier in the"
#        echo "update process, so you won't lose anything.)"
#        echo
#        echo "Do you want to upgrade your VoiceXML templates?"
#        echo "Type 'yes' (without the quotes) to continue:"
#        read answer2
#        echo ""
#    fi
#    if [ "x$answer2" = "xyes"  -o  $INSTALLMODE = $SILENT ]; then
#        mv -f $SBVDOCS/AAMain_Template.vxml.xml.up $SBVDOCS/AAMain_Template.vxml.xml
#        mv -f $SBVDOCS/EmailMain_Template.vxml.xml.up $SBVDOCS/EmailMain_Template.vxml.xml
#        mv -f $SBVDOCS/CalendarMain_Template.vxml.xml.up $SBVDOCS/CalendarMain_Template.vxml.xml
#        echo
#        echo "Template updated.  Remember to click the 'Generate"
#        echo "VoiceXML' button in the web admin for these changes"
#        echo "to take effect."
#    else
#        echo "Skipping VoiceXML template update."
#        echo ""
#    fi
#    echo ""

    echo "----------------------------------------------"
    echo "Done!"
    echo "----------------------------------------------"
    echo ""
else
    echo "- Skipping SpeechBridge upgrade."
    echo ""
fi
Cleanup
