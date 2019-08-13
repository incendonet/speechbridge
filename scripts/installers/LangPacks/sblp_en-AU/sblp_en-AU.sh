#!/bin/sh

# This script is for LumenVox <= 9.5.  If it is replacing the en-US model,
# it moves the en-US model out of active use (so it doesn't take up memory),
# and installs the en-AU (Australian) model.  Otherwise it adds the model
# to the directory of languages to load.

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

LOGFILE=$SBLOGS/sblp_"$THISLANGCODE"_$THISVER.log

MODELS_BASE=/opt/lumenvox/engine/Lang/Dict
MODEL_ENUS=$MODELS_BASE/OtherLanguages/en-US
MODEL_THIS=$MODELS_BASE/OtherLanguages/$THISLANGCODE/AustralianEnglish.model
MODELDIGITS_THIS=$MODELS_BASE/OtherLanguages/$THISLANGCODE/AustralianEnglishDigits.model

Cleanup()
{
    echo "Running Cleanup..."                                                      >> $LOGFILE 2>> $LOGFILE
	rm -f $SBCONF/sblp_$THISLANGCODE.sql                                                   >> $LOGFILE 2>> $LOGFILE
	rm -f $SBCONF/sblp_replace.sql                                                 >> $LOGFILE 2>> $LOGFILE
	rm -Rf $SBINST                                                                 >> $LOGFILE 2>> $LOGFILE
	echo "Finished Cleanup."                                                       >> $LOGFILE 2>> $LOGFILE
}

echo "Starting installation of $THISVER $THISLANGCODE at `date`."                  >> $LOGFILE 2>> $LOGFILE

# Check if logged in as root
if [ `whoami` != "root" ]; then
    echo "User was not logged in as root, aborting installation."                  >> $LOGFILE 2>> $LOGFILE
	echo "----------------------------------------------"
    echo "You must be logged in as root to to install this Language Pack.  Please log in as root and run this script again (or sudo it.)"
    echo "----------------------------------------------"
    echo
    Cleanup
    exit 0
fi

# Check if running proper version of SpeechBridge
INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
if [ "$THISVER" != "$INSTALLED_VER" ]; then
    echo "Not running a version this installer can install to."                  >> $LOGFILE 2>> $LOGFILE
	echo "----------------------------------------------"
	echo "Your system must be running SpeechBridge '$THISVER' to install this Language Pack."
	echo "This system's version is '$INSTALLED_VER'."
    echo "You can find your version number on the SpeechBridge login web page."
    echo "Aborting the installation now."
	echo "----------------------------------------------"
	echo
    Cleanup
	exit 0
fi

echo "User input - Replacing en-US, or adding this lang?"                         >> $LOGFILE 2>> $LOGFILE
echo "Is this language replacing the one currently installed?  If so, type 'yes' (without"
echo "the quotes) followed by the Enter key.  If you are adding this new language and keeping"
echo "the old one, type 'no'.  If you would like to abort the installation, press Ctrl-c."
read answer1

# Move files where we want them
#mkdir -p $SBINST

echo ""
echo "Installing SpeechBridge $THISLANGNAME Language Pack ($THISLANGCODE) for SB $THISVER..."
echo ""
echo "--------------------------------------------------------------------------------------"   >> $LOGFILE 2>> $LOGFILE
echo "Installing SpeechBridge $THISLANGNAME Language Pack ($THISLANGCODE) for SB $THISVER..."   >> $LOGFILE 2>> $LOGFILE
echo "Starting at: `date`"                                                         >> $LOGFILE 2>> $LOGFILE

echo "Move en-US models out of the way..."                                         >> $LOGFILE 2>> $LOGFILE
mkdir -p $MODEL_ENUS                                                             >> $LOGFILE 2>> $LOGFILE
mv -f $MODELS_BASE/AmericanEnglish*.model $MODEL_ENUS                          >> $LOGFILE 2>> $LOGFILE

echo "Move template(s) where we want them..."                                      >> $LOGFILE 2>> $LOGFILE
mv -f AAMain_"$THISLANGCODE"_Template.vmxl.xml $VDS                              >> $LOGFILE 2>> $LOGFILE
mv -f AAMain_"$THISLANGCODE"_Template.vxml.xml $VDS                              >> $LOGFILE 2>> $LOGFILE
chown speechbridge:speechbridge $VDS/AAMain_"$THISLANGCODE"_Template.vxml.xml    >> $LOGFILE 2>> $LOGFILE
chmod ug+r $VDS/AAMain_"$THISLANGCODE"_Template.vxml.xml                         >> $LOGFILE 2>> $LOGFILE

if [ "$answer1" = "yes" ]; then
	echo "Installer chose to replace the current lang."                            >> $LOGFILE 2>> $LOGFILE
	
	echo "Updating the DB to remove en-US..."                                      >> $LOGFILE 2>> $LOGFILE
	# NOTE - The case where we are replacing another language besides the default of en-US is probably very rare, and not adequately handled here.  In fact, we're replacing the DEFAULT DID mapping, which may not be desired.
	echo "DELETE FROM tblLanguages WHERE sLanguageCode = 'en-US';"                  > $SBCONF/sblp_replace.sql
	echo "DELETE FROM tblDtmfKeyToSpokenEquivalent WHERE sLanguageCode = 'en-US';" >> $SBCONF/sblp_replace.sql


	# IMPORTANT - Starting with SB 6.2 the sVoicexmlUrl entry actually only contains the name of the VXML mapped to the DID.  
	#             This needs to be taken into consideration if the value of THISVER (line 8) and/or the check on line 51 is ever changed.

	echo "UPDATE tblDIDMap SET sVoicexmlUrl='file:///opt/speechbridge/VoiceDocStore/AAMain_$THISLANGCODE.vxml.xml' where sDID='DEFAULT';" >>	$SBCONF/sblp_replace.sql
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script $SBCONF/sblp_replace.sql               >> $LOGFILE 2>> $LOGFILE

	echo "Deleting the American speech-attendant files..."                         >> $LOGFILE 2>> $LOGFILE
	rm -f $VDS/AAMain_en-US*.vxml.xml                                            >> $LOGFILE 2>> $LOGFILE

elif [ "$answer1" = "no" ]; then
	echo "Installer chose to add to the current lang, creating symlinks."         >> $LOGFILE 2>> $LOGFILE
	# NOTE - The case where we are adding to another language besides the default of en-US is probably very rare, and not handled here.
    ln -sf $MODEL_ENUS/AmericanEnglish.model $MODELS_BASE                      >> $LOGFILE 2>> $LOGFILE
    ln -sf $MODEL_ENUS/AmericanEnglishDigits.model $MODELS_BASE                >> $LOGFILE 2>> $LOGFILE
else
	echo "Aborting the upgrade at installer's request."                            >> $LOGFILE 2>> $LOGFILE
    echo "Aborting the upgrade now."
	echo "----------------------------------------------"
	Cleanup
	exit 0
fi

echo "Create SQL script to add the lang..."                                        >> $LOGFILE 2>> $LOGFILE
echo "INSERT INTO tblComponentUpdates (sDatabaseVersion, sSoftwareModule, sSoftwareVersion, sComments) VALUES ('5', 'SpeechBridge LanguagePack $THISLANGCODE', '5.1.1.69', 'SpeechBridge Language Pack for $THISLANGNAME, version 5.1.1.69');" >		$SBCONF/sblp_$THISLANGCODE.sql
echo "INSERT INTO tblTTSLanguageCodeMapping (sRequestedLanguageCode, sMappedLanguageCode) VALUES ('$THISLANGCODE', 'en-US');" >>	$SBCONF/sblp_$THISLANGCODE.sql
echo "INSERT INTO tblLanguages (sLanguageName, sLanguageCode) VALUES ('$THISLANGNAME', '$THISLANGCODE');"              >>	$SBCONF/sblp_$THISLANGCODE.sql

echo "Run SQL script..."                                                           >> $LOGFILE 2>> $LOGFILE
$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script $SBCONF/sblp_$THISLANGCODE.sql                     >> $LOGFILE 2>> $LOGFILE

echo "Generate vxml..."                                                          >> $LOGFILE 2>> $LOGFILE
$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --generate-vxml $SBCONF/sblp_$THISLANGCODE.sql                     >> $LOGFILE 2>> $LOGFILE
chown speechbridge:speechbridge $VDS/AAMain_$THISLANGCODE.vxml.xml             >> $LOGFILE 2>> $LOGFILE
chmod ug+rw $VDS/AAMain_$THISLANGCODE.vxml.xml                                 >> $LOGFILE 2>> $LOGFILE

echo "Create symlinks for this language model..."                                 >> $LOGFILE 2>> $LOGFILE
ln -sf $MODEL_THIS $MODELS_BASE                                                >> $LOGFILE 2>> $LOGFILE
ln -sf $MODELDIGITS_THIS $MODELS_BASE                                         >> $LOGFILE 2>> $LOGFILE

echo "Restarting lvdaemon..."                                                     >> $LOGFILE 2>> $LOGFILE
service lvdaemon restart                                                           >> $LOGFILE 2>> $LOGFILE

echo ""
echo "All done!  See $LOGFILE for details."
echo ""

Cleanup
