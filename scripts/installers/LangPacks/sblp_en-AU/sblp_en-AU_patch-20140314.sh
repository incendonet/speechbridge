#!/bin/sh

###################################################################################
# This script will update the initial en-AU Language Pack
# 2014-03-14
# Copyright 2014 Incendonet 
###################################################################################

THISVER="5.1.1.69"
THISLANGCODE="en-AU"
THISLANGNAME="English - Australian"

MONOBIN=/opt/novell/mono/bin
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBCONF=$SBBASE/config
SBLOGS=$SBBASE/logs
VDS=$SBBASE/VoiceDocStore
SBINST=$SBHOME/software/speechbridge_$THISLANGCODE

LOGFILE=$SBLOGS/sblp_"$THISLANGCODE"_"$THISVER"_patch.log


Cleanup()
{
	echo "Running Cleanup..."														>> $LOGFILE 2>> $LOGFILE
	echo "Finished Cleanup."														>> $LOGFILE 2>> $LOGFILE
}

echo "Starting patch to $THISVER $THISLANGCODE at `date`."							>> $LOGFILE 2>> $LOGFILE

# Check if logged in as root
if [ `whoami` != "root" ]; then
    echo "User was not logged in as root, aborting installation."					>> $LOGFILE 2>> $LOGFILE
	echo "----------------------------------------------"
    echo "You must be logged in as root to to install this patch.  Please log in as root and run this script again (or use sudo.)"
    echo "----------------------------------------------"
    echo
    Cleanup
    exit 0
fi

# Check if running proper version of SpeechBridge
INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
if [ "$THISVER" != "$INSTALLED_VER" ]; then
    echo "Not running a version this installer can install to."						>> $LOGFILE 2>> $LOGFILE
	echo "----------------------------------------------"
	echo "Your system must be running SpeechBridge '$THISVER' to install this patch."
	echo "This system's version is '$INSTALLED_VER'."
    echo "You can find your version number on the SpeechBridge login web page."
    echo "Aborting the installation now."
	echo "----------------------------------------------"
	echo
    Cleanup
	exit 0
fi

echo "User input - Install this patch?"												>> $LOGFILE 2>> $LOGFILE
echo "This patch will update the initial en-AU Language Pack, do you wish to continue?  If"
echo "so, type 'yes' (without the quotes) followed by the Enter key.  Otherwise press the"
echo "'Enter' key."
read answer1

echo ""
echo "Installing patch to SpeechBridge $THISLANGNAME Language Pack ($THISLANGCODE) for $THISVER..."
echo ""
echo "--------------------------------------------------------------------------------------"									>> $LOGFILE 2>> $LOGFILE
echo "Installing patch to SpeechBridge $THISLANGNAME Language Pack ($THISLANGCODE) for $THISVER..."								>> $LOGFILE 2>> $LOGFILE
echo "Starting at: `date`"                                                         >> $LOGFILE 2>> $LOGFILE

echo "Move files where we want them..."												>> $LOGFILE 2>> $LOGFILE
mv sblp_en-AU_pgsql.sql $SBCONF													>> $LOGFILE 2>> $LOGFILE

echo "Update grammar files..."														>> $LOGFILE 2>> $LOGFILE
mv $VDS/ABNFBoolean.gram		$VDS/ABNFBoolean_en-US.gram							>> $LOGFILE 2>> $LOGFILE
mv $VDS/ABNFDate.gram			$VDS/ABNFDate_en-US.gram							>> $LOGFILE 2>> $LOGFILE
mv $VDS/ABNFDigits.gram		$VDS/ABNFDigits_en-US.gram							>> $LOGFILE 2>> $LOGFILE

#/bin/sed 's:en-US:en-AU:' < $VDS/ABNFBoolean_en-US.gram > $VDS/ABNFBoolean_en-AU.gram		2>> $LOGFILE
#/bin/sed 's:en-US:en-AU:' < $VDS/ABNFDate_en-US.gram > $VDS/ABNFDate_en-AU.gram			2>> $LOGFILE
#/bin/sed 's:en-US:en-AU:' < $VDS/ABNFDigits_en-US.gram > $VDS/ABNFDigits_en-AU.gram		2>> $LOGFILE
/bin/sed 's:en-US:en-AU:' < $VDS/ABNFBoolean_en-US.gram > $VDS/ABNFBoolean.gram			2>> $LOGFILE
/bin/sed 's:en-US:en-AU:' < $VDS/ABNFDate_en-US.gram > $VDS/ABNFDate.gram					2>> $LOGFILE
/bin/sed 's:en-US:en-AU:' < $VDS/ABNFDigits_en-US.gram > $VDS/ABNFDigits.gram				2>> $LOGFILE

echo "Change permissions on residual files..."										>> $LOGFILE 2>> $LOGFILE
/bin/chown speechbridge:speechbridge $VDS/*.gram									>> $LOGFILE 2>> $LOGFILE
/bin/chown speechbridge:speechbridge $SBCONF/sblp_en-AU_pgsql.sql					>> $LOGFILE 2>> $LOGFILE
/bin/chmod ug+r $VDS/*.gram														>> $LOGFILE 2>> $LOGFILE
/bin/chmod ug+r $SBCONF/sblp_en-AU_pgsql.sql										>> $LOGFILE 2>> $LOGFILE

echo "Run SQL script(s)..."															>> $LOGFILE 2>> $LOGFILE
$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script $SBCONF/sblp_en-AU_pgsql.sql			>> $LOGFILE 2>> $LOGFILE

echo ""
echo "All done!  See $LOGFILE for details."
echo ""

Cleanup
