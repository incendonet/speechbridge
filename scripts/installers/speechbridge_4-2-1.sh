#!/bin/sh

# tar with:    tar -cPf foo.tar fullpath
# untar with:  tr -xPf foo.tar

#---------------------------------------------------------------------------------------------------------
# Installer script
#---------------------------------------------------------------------------------------------------------
THISVER="4.2.1"
SBHOME=/home/speechbridge
SBINST=$SBHOME/software/speechbridge-$THISVER
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBCONF=$SBCONF/config
SBLOGS=$SBBASE/logs
SBVDOCS=$SBBASE/VoiceDocStore
LOGFILE=$SBLOGS/InstallLog_speechbridge_$THISVER.log
HTTPDCONF=/etc/httpd/conf
MONOBIN=/opt/novell/mono/bin
SILENT="--silent"
UPGRADEABLE_VER="4.1.1.243"
INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
INSTALLMODE="$1"
TEMPLATESDIFFER=""
TEMPLATEUPGRADED=0

Cleanup()
{
    echo "Running Cleanup..." >> $LOGFILE
    rm -f $SBINST/speechbridge-$THISVER-*.i686.rpm
    rm -f $SBINST/AAMain_Template_4-1-1_ORIG.vxml.xml
	echo "Finished Cleanup." >> $LOGFILE
}

echo "Starting installation of $THISVER at `date`." >> $LOGFILE
echo "Previously installed version of SpeechBridge is '$INSTALLED_VER.'" >> $LOGFILE
echo "" >> $LOGFILE

if [ `whoami` != "root" ]; then
    echo "User was not logged in as root, aborting installation." >> $LOGFILE
	echo "----------------------------------------------"
    echo "You must be logged in as root to upgrade SpeechBridge.  Please log in as root (or sudo) and run this script again."
    echo "----------------------------------------------"
    echo
    Cleanup
    exit 0
fi

# Check dependencies
if [ "$UPGRADEABLE_VER" != "$INSTALLED_VER" ]; then
    echo "Not running a version this installer can upgrade from." >> $LOGFILE
	echo "----------------------------------------------"
	echo "Your system must be running SpeechBridge '$UPGRADEABLE_VER' to install this update."
	echo "This system's version is '$INSTALLED_VER'."
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

# Move files where we want them
mkdir -p $SBINST
mv -f speechbridge-$THISVER-*.i686.rpm $SBINST
mv -f AAMain_Template_4-1-1_ORIG.vxml.xml $SBINST

if [ "x$answer1" = "xyes"  -o  $INSTALLMODE = $SILENT ]; then
    if [ $INSTALLMODE != $SILENT ]; then
        diff $SBVDOCS/AAMain_Template.vxml.xml $SBINST/AAMain_Template_4-1-1_ORIG.vxml.xml > /dev/null
        TEMPLATESDIFFER=$?

        if [ $TEMPLATESDIFFER -eq 0 ]; then
            echo "The template is unmodified, upgrading..." >> $LOGFILE
            echo "The template is unmodified, upgrading..."
            answer2="yes"
        else
            echo "----------------------------------------------"
            echo "It looks like your VoiceXML templates have been"
            echo "customized.  This release brings a number of"
            echo "improvements to the default templates, but in"
            echo "order to take advantage of them AND use your"
            echo "customizations, you would need to reapply them"
            echo "to the new templates.  Your current files will"
            echo "be backed up, so if you upgrade and change your"
            echo "mind later, you can still go back.  If you are"
            echo "not sure what to do, please contact your"
            echo "Incendonet authorized SpeechBridge reseller."
            echo ""
            echo "If you want to keep your old VoiceXML "
            echo "templates, type 'keep' (without the quotes),"
            echo "otherwise just press the Enter key."
            read answer2
            echo ""
        fi
    fi

    echo "About to begin SpeechBridge RPM upgrade..." >> $LOGFILE
    echo "----------------------------------------------"
    echo "About to begin SpeechBridge RPM upgrade..."
    rpm -ivh --force $SBINST/speechbridge-$THISVER-*.i686.rpm
    #rpm -Uvh $SBINST/speechbridge-$THISVER-*.i686.rpm				# I can't get the rpm upgrade option to work the way I want...
	echo ""
    echo "RPM install completed with status '$?'." >> $LOGFILE

    echo "About to update template..." >> $LOGFILE
    if [ "$answer2" = "keep" ]; then
        echo "Skipping VoiceXML template update." >> $LOGFILE
        echo "Skipping VoiceXML template update."
        echo ""
    else
#    if [ "x$answer2" = "xyes"  -o  $INSTALLMODE = $SILENT ]; then
        mv -f $SBVDOCS/AAMain_Template_4-2-1.vxml.xml $SBVDOCS/AAMain_Template.vxml.xml
        TEMPLATEUPGRADED=1
        echo "Template updated." >> $LOGFILE
    fi
    echo ""

    echo "----------------------------------------------"
    echo "Done!"

    if [ $TEMPLATEUPGRADED -eq 1 ]; then
        echo ""
        echo "##########################################################################"
        echo "# VERY IMPORTANT:  Remember to click the 'Generate                       #"
        echo "# VoiceXML' button in the web admin for these changes                    #"
        echo "# to take effect.                                                        #"
        echo "##########################################################################"
        echo ""
    fi

    # Check the logs for errors
    if [ `grep "ERROR" $LOGFILE | wc -l` -gt 0 ]; then
        echo ""
        echo "##########################################################################"
        echo "# WARNING:  Issues were detected during the installation, please contact #"
        echo "# your Incendonet authorized reseller for assistance.  Please copy:      #"
        echo "#   $LOGFILE"
        echo "# for their reference.  Errors include, but not limited to:              #"
        echo ""
        grep "ERROR" $LOGFILE
        echo ""
        echo "##########################################################################"
        echo ""
    fi

    echo "----------------------------------------------"
    echo ""
    echo "Done at `date`." >> $LOGFILE
else
    echo "Skipping SpeechBridge upgrade." >> $LOGFILE
    echo "- Skipping SpeechBridge upgrade."
    echo ""
fi
Cleanup
