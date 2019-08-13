#!/bin/sh
#---------------------------------------------------------------------------------------------------------
# Copyright 2013 Incendonet Inc.
#
# This SQL script will update a "fresh" 4.2.1.13007 installation.  It is dependent on the following files
# which much be in the current directory:
#   sb_4-2-1_patch_pgsql.sql, SBCreate_pgsql.sql, SBUpdate_4-2-1_pgsql.sql, AAMain_Template_4-2-1_ORIG.vxml.xml
#---------------------------------------------------------------------------------------------------------

MONOBIN=/opt/novell/mono/bin
THISVER="4.2.1.13007a"
SBHOME=/home/speechbridge
#SBINST=$SBHOME/software/speechbridge-$THISVER
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBCONF=$SBBASE/config
SBLOGS=$SBBASE/logs
SBVDOCS=$SBBASE/VoiceDocStore
ORIGVXML=AAMain_Template_4-2-1_ORIG.vxml.xml
LOGFILE=$SBLOGS/InstallLog_speechbridge_$THISVER\_patch.log

UPGRADEABLE_VER="4.2.1.1300700"
INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
INSTALLMODE="$1"
TEMPLATESDIFFER=0

#---------------------------------------------------------------------------------------------------------
# Cleanup function
Cleanup()
{
    echo "Running Cleanup..." >> $LOGFILE
    rm -f $ORIGVXML                                                          >> $LOGFILE  2>> $LOGFILE
    rm -f sb_4-2-1_patch_pgsql.sql                                            >> $LOGFILE  2>> $LOGFILE
    rm -f SBCreate_pgsql.sql                                                  >> $LOGFILE  2>> $LOGFILE
    rm -f SBUpdate_4-2-1_pgsql.sql                                            >> $LOGFILE  2>> $LOGFILE
    echo "Finished Cleanup." >> $LOGFILE
}

echo "Starting installation of $THISVER patch at `date`."                     >> $LOGFILE
echo "Previously installed version of SpeechBridge is '$INSTALLED_VER.'"      >> $LOGFILE
echo "" >> $LOGFILE

#---------------------------------------------------------------------------------------------------------
# Verify that user is logged in as root
if [ `whoami` != "root" ]; then
    echo "User was not logged in as root, aborting installation."             >> $LOGFILE
	echo "----------------------------------------------"
    echo "You must be logged in as root to upgrade SpeechBridge.  Please log in as root (or sudo) and run this script again."
    echo "----------------------------------------------"
    echo
    Cleanup
    exit 0
fi

#---------------------------------------------------------------------------------------------------------
# Check dependencies
if [ "$UPGRADEABLE_VER" != "$INSTALLED_VER" ]; then
    echo "Not running a version this installer can upgrade from."             >> $LOGFILE
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

echo "----------------------------------------------"
echo "Starting $THISVER patch..."
echo "Starting $THISVER patch..."         >> $LOGFILE

#---------------------------------------------------------------------------------------------------------
# Copy updated files
echo "Copying updated files..."           >> $LOGFILE
echo "Copying updated files..."

cp -f SBCreate_pgsql.sql $SBCONF/        >> $LOGFILE 2>> $LOGFILE
cp -f SBUpdate_4-2-1_pgsql.sql $SBCONF/  >> $LOGFILE 2>> $LOGFILE

#---------------------------------------------------------------------------------------------------------
# Update database
echo "Updating the database..."           >> $LOGFILE
echo "Updating the database..."
$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script sb_4-2-1_patch_pgsql.sql         >> $LOGFILE 2>> $LOGFILE

#---------------------------------------------------------------------------------------------------------
# Update AAMain_Template.vxml.xml
diff $SBVDOCS/AAMain_Template.vxml.xml $ORIGVXML > /dev/null 2>> $LOGFILE
TEMPLATESDIFFER=$?

if [ $TEMPLATESDIFFER -eq 0 ]; then
    echo "The template is unmodified, upgrading..." >> $LOGFILE
    echo "The template is unmodified, upgrading..."
    sed 's:3.0.1:4.2.1:' < $SBVDOCS/AAMain_Template.vxml.xml > $SBVDOCS/AAMain_Template_PATCHED.vxml.xml                       2>> $LOGFILE
    rm -f $SBVDOCS/AAMain_Template.vxml.xml                                                                                      >> $LOGFILE 2>> $LOGFILE
    sed 's:Copyright 2010:Copyright 2013:' < $SBVDOCS/AAMain_Template_PATCHED.vxml.xml > $SBVDOCS/AAMain_Template.vxml.xml     2>> $LOGFILE
    rm -f $SBVDOCS/AAMain_Template_PATCHED.vxml.xml                                                                              >> $LOGFILE 2>> $LOGFILE
    chown speechbridge:speechbridge $SBVDOCS/AAMain_Template.vxml.xml                                                            >> $LOGFILE 2>> $LOGFILE
else
    echo "The template has been modified, we'll leave it alone." >> $LOGFILE
    echo "The template has been modified, we'll leave it alone."
fi

Cleanup

echo "Done."
echo "----------------------------------------------"
echo ""
echo "Done at `date`." >> $LOGFILE
