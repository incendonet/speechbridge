#!/bin/sh

# tar with:    tar -cPf foo.tar fullpath
# untar with:  tr -xPf foo.tar

#---------------------------------------------------------------------------------------------------------
# Installer script
#---------------------------------------------------------------------------------------------------------
SBHOME=/home/speechbridge
SBINST=$SBHOME/software/sbupdate-3-1-1
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBLOGS=$SBBASE/logs
SBVDOCS=$SBBASE/VoiceDocStore
LOGFILE=$SBLOGS/sbupdate_3-1-1.log
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
    echo "version 3.1.1, but this will disrupt all calls"
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
    mv -f sbupdate-3.1.1-*.i686.rpm $SBINST

    echo "----------------------------------------------"
    echo "About to begin SpeechBridge upgrade..."
    rpm -ivh --force $SBINST/sbupdate-3.1.1-*.i686.rpm
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

