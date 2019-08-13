#!/bin/bash

# tar with:		tar -cPf foo.tar fullpath
# untar with:  tr -xPf foo.tar

#---------------------------------------------------------------------------------------------------------
# Installer script
#---------------------------------------------------------------------------------------------------------
TODAYSTR="$(/bin/date --date=today +%Y%m%d)"

OLDVER0="5.1.1"
OLDVER1="6.0.1"
OLDVER2="6.1.1"
OLDVER3="6.2.1"
OLDVER4="6.3.1"
OLDVER5="6.3.2"
THISVER="6.4.1"
OLDVERDASH0="5-0-1"
OLDVERDASH1="5-1-1"
OLDVERDASH1="6-0-1"
OLDVERDASH2="6-1-1"
OLDVERDASH3="6-2-1"
OLDVERDASH4="6-3-1"
OLDVERDASH5="6-3-2"
OLDVERDASH5A="6-3-2a"
THISVERDASH="6-4-1"
#MINORREV="a"

SBHOME=/home/speechbridge
SBSOFT=$SBHOME/software
SBINST=$SBSOFT/speechbridge-$THISVER
SBBASE=/opt/speechbridge
SBBAK=/$SBBASE/backup/speechbridge-$THISVERDASH
SBBIN=$SBBASE/bin
SBCONF=$SBBASE/config
SBLIC=$SBBASE/license
SBLOGS=$SBBASE/logs
SBSBCONFIG=$SBBASE/SBConfig
SBVDS=$SBBASE/VoiceDocStore
SBVDSPROMPTS=$SBVDS/Prompts
SBVDSNAMES=$SBVDSPROMPTS/Names
LOGFILE=$SBLOGS/InstallLog_speechbridge_$THISVER.log
BAKFILE=$SBLOGS/sbbackup_$TODAYSTR.tar.gz
OLDVER1FILES=$SBINST/speechbridge-$OLDVER1.tar.bz2
OLDVER2FILES=$SBINST/speechbridge-$OLDVER2.tar.bz2
OLDVER3FILES=$SBINST/speechbridge-$OLDVER3.tar.bz2
OLDVER4FILES=$SBINST/speechbridge-$OLDVER4.tar.bz2
OLDVER5FILES=$SBINST/speechbridge-$OLDVER5.tar.bz2
THISVERFILES=$SBINST/speechbridge-$THISVER.tar.bz2
HTTPDCONF=/etc/httpd/conf
MONOBIN=/opt/novell/mono/bin
SILENT="--silent"
UPGRADEABLE_VER0="5.0.1.13242"
UPGRADEABLE_VER1="5.1.1.69"
UPGRADEABLE_VER2="6.0.1.14261"
UPGRADEABLE_VER3="6.1.1.14282"
UPGRADEABLE_VER4="6.2.1.43"
UPGRADEABLE_VER5="6.3.1.143"
UPGRADEABLE_VER6="6.3.2.15296"
RPM_VER1="speechbridge-5.1.1-69.i686.rpm"
INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
INSTALLMODE="$1"
TEMPLATESDIFFER=""
TEMPLATEUPGRADED=0


##############################################################################################################
##############################################################################################################
Cleanup()
{
	echo "Running Cleanup..."																		>> $LOGFILE
	rm -Rf $SBINST/*																				>> $LOGFILE
	rmdir $SBINST																					>> $LOGFILE
	rm -f $SBVDS/*$THISVERDASH*																		>> $LOGFILE
	rm -f $SBVDS/*$OLDVERDASH5*																		>> $LOGFILE
	rm -f $SBVDS/*$OLDVERDASH5A*																	>> $LOGFILE
	echo "Finished Cleanup."																		>> $LOGFILE
} # Cleanup

##############################################################################################################
##############################################################################################################
Backup()
{
	# First clean out files we don't want
	find $SBBIN/logs/ -mtime +1 -exec rm {} \;														>> /dev/null 2>> $LOGFILE

	mkdir -p $SBBAK

	echo "Backing up the current DB..."																>> $LOGFILE
	/bin/bash $SBBIN/sbbackup.cron
	mv -f $BAKFILE $SBBAK/sbdb-bak_$TODAYSTR.tar.gz													>> $LOGFILE 2>> $LOGFILE

	echo "Backing up the files in the directories affected by the installer..."						>> $LOGFILE
	tar -cjf $SBBAK/sbbak_before_$THISVER_$TODAYSTR.tar.bz2 $SBBIN $SBCONF $SBSBCONFIG $SBVDS		>> $LOGFILE 2>> $LOGFILE

	echo "Backups completed."																		>> $LOGFILE
} # Backup

##############################################################################################################
# Checks if there is an Incendonet.lic licencse, prompt if not.
##############################################################################################################
LicenseFileCheck()
{
	if [ ! -f $SBLIC/Incendonet.lic ] && [ $INSTALLMODE != $SILENT ]; then
		echo "Incendonet.lic not found in /opt/speechbridge/license/"								>> $LOGFILE
		echo ""
		echo "Heads up - You don't have the Incendonet.lic license file that you'll need to run SpeechBridge"
		echo "in /opt/speechbridge/license/.  Do you want to stop and correct this now, or come back to it"
		echo "later?  If you copy your license file over now, you won't need to restart SpeechBridge later."
		echo ""
		echo "  Press 'q' and <Enter> to (q)uit now.  Press 'c' and <Enter> to (c)ontinue:"
		echo ""
		
		read STOPFORLIC
		
		if [ "$STOPFORLIC" == "q" ]; then
			echo "Stopping the install at user request - license. ('q')"							>> $LOGFILE
			echo "Ok, we're stopping the upgrade.  Please copy your Incendonet.lic file to /opt/speechbridge/logs/"
			echo "with a tool like WinSCP, and run this installer again to continue."
			echo ""
			echo "Upgrade cancelled at user's request to install license."							>> $LOGFILE
			exit 0
		elif [ "$STOPFORLIC" == "c" ]; then
			echo "Inoring missing license at user request."											>> $LOGFILE
			echo "Ok, continuing with the upgrade."
			echo ""
		else
			echo "Stopping the install at user request - license. ('')"								>> $LOGFILE
			echo "Ok, we're stopping the upgrade.  Please copy your Incendonet.lic file to /opt/speechbridge/logs/"
			echo "with a tool like WinSCP, and run this installer again to continue."
			echo ""
			echo "Upgrade cancelled at user's request to install license."							>> $LOGFILE
			exit 0
		fi
	else
		echo "Found Incendonet.lic in /opt/speechbridge/license/"									>> $LOGFILE
	fi
}

##############################################################################################################
##############################################################################################################
UpdatePrompts()
{
	tar -xjf $SBVDSPROMPTS/sbprompts_$THISVERDASH.tar.bz2 -C $SBVDS/Prompts							>> $LOGFILE 2>> $LOGFILE

	rm -f $SBVDS/Prompts/DID/DEFAULT/ThankYouForCallingPleaseSayTheName.wav							>> $LOGFILE 2>> $LOGFILE
	rm -f $SBVDS/Prompts/DID/DEFAULT/ThankYouForCallingAfterHoursGreeting.wav						>> $LOGFILE 2>> $LOGFILE

	ln -s $SBVDS/Prompts/ThankYouForCallingPleaseSayTheName.wav $SBVDS/Prompts/DID/DEFAULT/ThankYouForCallingPleaseSayTheName.wav			>> $LOGFILE 2>> $LOGFILE
	ln -s $SBVDS/Prompts/ThankYouForCallingPleaseSayTheName.wav $SBVDS/Prompts/DID/DEFAULT/ThankYouForCallingAfterHoursGreeting.wav			>> $LOGFILE 2>> $LOGFILE
	
	chown -R speechbridge:speechbridge $SBVDS/Prompts/*.wav											>> $LOGFILE 2>> $LOGFILE
}

##############################################################################################################
##############################################################################################################
UpdateLangPacks()
{
	# Work only on currently installed localization files
	# Note - A language may be installed that isn't supported by the speech-attendant, but is being used by a custom application,
	# in which case we don't need to make any changes for its files.
	echo "Updating installed Language Packs..."														>> $LOGFILE

	if [ -f $SBVDS/AAMain_en-US_Template.vxml.xml ]; then
		echo "Found en-US LangPack, updating..."													>> $LOGFILE

		if [ ! -f $SBVDS/ABNFBoolean_en-US.gram ]; then
			echo "Setting up symlinks for en-US..."													>> $LOGFILE
			mv -f $SBVDS/ABNFBoolean.gram			$SBVDS/ABNFBoolean_en-US.gram					>> $LOGFILE 2>> $LOGFILE
			mv -f $SBVDS/ABNFDate.gram				$SBVDS/ABNFDate_en-US.gram						>> $LOGFILE 2>> $LOGFILE
			mv -f $SBVDS/ABNFDigits.gram			$SBVDS/ABNFDigits_en-US.gram					>> $LOGFILE 2>> $LOGFILE
			ln -s $SBVDS/ABNFBoolean_en-US.gram		$SBVDS/ABNFBoolean.gram							>> $LOGFILE 2>> $LOGFILE
			ln -s $SBVDS/ABNFBooleanSA_en-US.gram	$SBVDS/ABNFBooleanSA.gram						>> $LOGFILE 2>> $LOGFILE	# This file is new with 6.0.1
			ln -s $SBVDS/ABNFDate_en-US.gram		$SBVDS/ABNFDate.gram							>> $LOGFILE 2>> $LOGFILE
			ln -s $SBVDS/ABNFDigits_en-US.gram		$SBVDS/ABNFDigits.gram							>> $LOGFILE 2>> $LOGFILE
		fi

		echo "Removing leftover files..."															>> $LOGFILE
		rm -f $SBVDS/ABNFBooleanSA_en-AU.gram														>> $LOGFILE 2>> $LOGFILE
	fi

	if [ -f $SBVDS/AAMain_en-AU_Template.vxml.xml ]; then
		echo "Found en-AU LangPack, updating..."													>> $LOGFILE
		if [ -f $SBVDS/ABNFBoolean.gram ]; then
			mv -f $SBVDS/ABNFBoolean.gram			$SBVDS/ABNFBoolean_en-AU.gram					>> $LOGFILE 2>> $LOGFILE
			mv -f $SBVDS/ABNFDate.gram				$SBVDS/ABNFDate_en-AU.gram						>> $LOGFILE 2>> $LOGFILE
			mv -f $SBVDS/ABNFDigits.gram			$SBVDS/ABNFDigits_en-AU.gram					>> $LOGFILE 2>> $LOGFILE
		else
			cp -f $SBVDS/ABNFBoolean_en-US.gram		$SBVDS/ABNFBoolean_en-AU.gram				>> $LOGFILE 2>> $LOGFILE
			cp -f $SBVDS/ABNFDate_en-US.gram		$SBVDS/ABNFDate_en-AU.gram						>> $LOGFILE 2>> $LOGFILE
			cp -f $SBVDS/ABNFDigits_en-US.gram		$SBVDS/ABNFDigits_en-AU.gram					>> $LOGFILE 2>> $LOGFILE
		fi
		ln -s $SBVDS/ABNFBoolean_en-AU.gram			$SBVDS/ABNFBoolean.gram					>> $LOGFILE 2>> $LOGFILE
		ln -s $SBVDS/ABNFBooleanSA_en-AU.gram		$SBVDS/ABNFBooleanSA.gram						>> $LOGFILE 2>> $LOGFILE	# This file is new with 6.0.1
		ln -s $SBVDS/ABNFDate_en-AU.gram			$SBVDS/ABNFDate.gram							>> $LOGFILE 2>> $LOGFILE
		ln -s $SBVDS/ABNFDigits_en-AU.gram			$SBVDS/ABNFDigits.gram							>> $LOGFILE 2>> $LOGFILE

		echo "Removing leftover files..."
		rm -f $SBVDS/ABNFBooleanSA_en-US.gram														>> $LOGFILE 2>> $LOGFILE
	fi

	echo "Done updating Language Packs"																>> $LOGFILE
} # UpdateLangPacks

##############################################################################################################
##############################################################################################################
CreateSymlinks()
{
	echo "Starting to create symlinks...."															>> $LOGFILE

	# Create links for license files, moving them if they already exist.

	# Cepstral
	CEPLICDIR=/opt/swift/voices/Callie-8kHz
	CEPLICFILE=license.txt
	if [ -f $CEPLICDIR/$CEPLICFILE ]; then
		mv -f $CEPLICDIR/$CEPLICFILE $SBLIC/$CEPLICFILE										>> $LOGFILE 2>> $LOGFILE
	fi
	ln -s $SBLIC/$CEPLICFILE $CEPLICDIR/$CEPLICFILE											>> $LOGFILE 2>> $LOGFILE

	# Neospeech
	NEOLICDIR=/usr/vt/verify
	NEOLICFILE=verification.txt
	if [ ! -f $NEOLICDIR ]; then
		mkdir -p $NEOLICDIR																		>> $LOGFILE 2>> $LOGFILE
	fi
	if [ -f $NEOLICDIR/$NEOLICFILE ]; then
		mv -f $NEOLICDIR/$NEOLICFILE $SBLIC/$NEOLICFILE										>> $LOGFILE 2>> $LOGFILE
	fi
	ln -s $SBLIC/$NEOLICFILE $NEOLICDIR/$NEOLICFILE											>> $LOGFILE 2>> $LOGFILE

	# LumenVox
	LVLICDIR=/opt/lumenvox/licenseserver/bin
	LVLICFILE=License.bts
	if [ -f $LVLICDIR/$LVLICFILE ]; then
		mv -f $LVLICDIR/$LVLICFILE $SBLIC/$LVLICFILE											>> $LOGFILE 2>> $LOGFILE
	fi
	ln -s $SBLIC/$LVLICFILE $LVLICDIR/$LVLICFILE												>> $LOGFILE 2>> $LOGFILE

	echo "Done creating symlinks."																	>> $LOGFILE
} # CreateSymlinks

##############################################################################################################
##############################################################################################################
UpgradeTemplates()
{
	diff $SBVDS/AAMain_en-US_Template.vxml.xml $SBVDS/AAMain_en-US_Template_$OLDVERDASH5.vxml.xml > /dev/null
	TEMPLATES1DIFFER=$?
	diff $SBVDS/AAMain_en-US_Template.vxml.xml $SBVDS/AAMain_en-US_Template_$OLDVERDASH5A.vxml.xml > /dev/null
	TEMPLATES2DIFFER=$?

	diff $SBVDS/SB-NumberDialer_Template.vxml.xml $SBVDS/SB-NumberDialer_Template_$OLDVERDASH5.vxml.xml > /dev/null
	TEMPLATES3DIFFER=$?
	
	if [ $TEMPLATES1DIFFER -eq 0 ] || [ $TEMPLATES2DIFFER -eq 0 ] && [ $TEMPLATES3DIFFER -eq 0 ]; then
		echo "The templates are unmodified, upgrading..." >> $LOGFILE
		echo "The templates are unmodified, upgrading..."
		answer2="c"
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
		echo "  If you want to (k)eep your old VoiceXML "
		echo "  templates, press 'k' (without the quotes), and"
		echo "  <Enter>, otherwise just press the <Enter> key."
		echo ""
		read answer2
	fi

    if [ "$answer2" == "k" ]; then
        echo "Skipping VoiceXML template update." >> $LOGFILE
        echo "Skipping VoiceXML template update."
        echo ""
    else
		echo "About to update template..." >> $LOGFILE
        mv -f $SBVDS/AAMain_en-US_Template_$THISVERDASH.vxml.xml $SBVDS/AAMain_en-US_Template.vxml.xml 2>> $LOGFILE
        echo "AAMain template updated." >> $LOGFILE
        echo "AAMain template updated."
        mv -f $SBVDS/SB-NumberDialer_Template_$THISVERDASH.vxml.xml $SBVDS/SB-NumberDialer_Template.vxml.xml 2>> $LOGFILE
        echo "Number-dialer template updated." >> $LOGFILE
        echo "Number-dialer template updated."

        $MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --generate-vxml >> $LOGFILE 2>> $LOGFILE
        echo "VoiceXML generated from new templates." >> $LOGFILE
        echo "VoiceXML generated from new templates."
    fi

    echo ""
} # UpgradeTemplates

##############################################################################################################
# UpdateShoretelProfile
#
# ShoreTel updated the way they do the SIP Refer sequence in 14.1, and the SIP profile we use needs to
# change as a result.  Going forward, there will be profiles for "ShoreTel" and "ShoreTel legacy (...)"
# Note - This needs to be run after the assemblies for SB 6.4.1 are installed.
##############################################################################################################
UpdateShoretelProfile()
{
	echo  "About to check for ShoreTel..." >> $LOGFILE

	# Check if SB is currently set to ShoreTel
	PBXTYPE=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-sqlstring-query "SELECT sValue FROM tblConfigParams WHERE sName = 'IppbxType';"`
	if [ "$PBXTYPE" == "ShoreTel" ]; then
	
		# If ShoreTel, then prompt if >= 14.1
		if [ $INSTALLMODE != $SILENT ]; then
			echo "Your SpeechBridge is configured to use ShoreTel.  Is your ShoreTel running 14.1 or higher?"
			echo ""
			echo "  Please press 'y' or 'n' (without the quotes) and press <Enter>."
			echo ""
            read STNEW
			
			if [ "$STNEW" == "n" ]; then
				# Set DB for ShoreTel legacy
				echo "Setting ShoreTel SIP profile to legacy..." >> $LOGFILE
				echo "Setting ShoreTel SIP profile to legacy..."
				$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-sqlstring-query "UPDATE tblConfigParams SET sValue = 'ShoreTel legacy (14.0 and earlier)' WHERE sName = 'IppbxType';"
			else
				echo "Keeping PBX as 'ShoreTel'." >> $LOGFILE
				echo "Keeping PBX as 'ShoreTel'."
			fi
		fi
	fi

	# Generate SIP config files
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --write-configs-sip

	echo ""
	echo "SIP configuration generated." >> $LOGFILE
	echo "SIP configuration generated."
	echo ""
} # UpdateShoretelProfile

##############################################################################################################
##############################################################################################################
Install641()
{
	echo "Starting Install641..."																	>> $LOGFILE

	echo "Extracting new files..."																	>> $LOGFILE

	tar -xjf $THISVERFILES -C $SBBASE																>> $LOGFILE 2>> $LOGFILE

	# Link and move as necessary
	ln -s $SBBIN/libPocoFoundation.so.43		/usr/lib/libPocoFoundation.so.43					>> $LOGFILE 2>> $LOGFILE
	mv -f $SBBIN/sbpostinstall_FIRSTBOOT.sh		$SBSOFT												>> $LOGFILE 2>> $LOGFILE
	cp -f $SBBIN/sbpostinstall_POSTFIRSTBOOT.sh	$SBSOFT												>> $LOGFILE 2>> $LOGFILE
	mv -f $SBBIN/sbpostinstall_POSTFIRSTBOOT.sh	$SBSOFT/sbpostinstall.sh							>> $LOGFILE 2>> $LOGFILE
	chmod +x $SBSOFT/*.sh																			>> $LOGFILE 2>> $LOGFILE

	UpdatePrompts

	echo "Updating DB..."																			>> $LOGFILE
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-sqlscript-commands $SBCONF/SBUpdate_pgsql.sql	>> $LOGFILE 2>> $LOGFILE
	
#	echo "Updating VoiceXML templates..."															>> $LOGFILE

	echo "Running MigrationTool..."																	>> $LOGFILE
	$MONOBIN/mono $SBBIN/MigrationTool.exe															>> $LOGFILE 2>> $LOGFILE

#	echo "Updating AudioMgr.exe.config..."															>> $LOGFILE 2>> $LOGFILE

#	echo "Updating Web.config..."																	>> $LOGFILE 2>> $LOGFILE

	echo "Restarting services..."																	>> $LOGFILE

	/sbin/service httpd reload																		>> $LOGFILE 2>> $LOGFILE
	/sbin/service speechbridged restart																>> $LOGFILE 2>> $LOGFILE

	# Fix ShoreTel >= 14.1 settings
	UpdateShoretelProfile

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	echo "$INSTALLED_VER" >> $SBBASE/build.txt
	echo "Install of $INSTALLED_VER complete."
} # Install641

##############################################################################################################
##############################################################################################################
Install632()
{
	echo "Starting Install632..."																	>> $LOGFILE

	echo "Extracting new files..."																	>> $LOGFILE
	tar -xjf $OLDVER5FILES -C $SBBASE																>> $LOGFILE 2>> $LOGFILE
	cp -f $SBCONF/sbpostinstall_POSTFIRSTBOOT.sh $SBSOFT/sbpostinstall_POSTFIRSTBOOT.sh				>> $LOGFILE 2>> $LOGFILE
	chmod +w  $SBSOFT/sbpostinstall.sh																>> $LOGFILE 2>> $LOGFILE
	mv -f $SBCONF/sbpostinstall_POSTFIRSTBOOT.sh $SBSOFT/sbpostinstall.sh							>> $LOGFILE 2>> $LOGFILE
	chmod -w  $SBSOFT/sbpostinstall.sh																>> $LOGFILE 2>> $LOGFILE


	echo "Updating DB..."																			>> $LOGFILE
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script $SBCONF/SBUpdate_pgsql.sql	>> $LOGFILE 2>> $LOGFILE
	
	echo "Updating VoiceXML templates..."															>> $LOGFILE

	if [ `grep "2007" $SBVDS/SBRoot.vxml.xml | wc -l` -ge 1 ]; then
		sed 's:2007:2015:' < $SBVDS/SBRoot.vxml.xml > $SBVDS/SBRoot2.vxml.xml							2>> $LOGFILE
		mv -f $SBVDS/SBRoot2.vxml.xml $SBVDS/SBRoot.vxml.xml											>> $LOGFILE 2>> $LOGFILE
	fi

	if [ `grep "Set default system properties" $SBVDS/SBRoot.vxml.xml | wc -l` -eq 0 ]; then
		sed 's:\t<!-- Application variables\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t-->:\t<!-- Set default system properties\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t-->\
\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\t-->\
\t<property name="completetimeout"\tvalue="1s" />\
\t<property name="interdigittimeout"\tvalue="3s" />\
\t<property name="termchar"\t\t\tvalue="" />\
\
\t<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\t-->\
\t<!-- Application variables\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t-->:' < $SBVDS/SBRoot.vxml.xml > $SBVDS/SBRoot2.vxml.xml			2>> $LOGFILE
		mv -f $SBVDS/SBRoot2.vxml.xml $SBVDS/SBRoot.vxml.xml										>> $LOGFILE 2>> $LOGFILE
	fi

#	echo "Running MigrationTool..."																	>> $LOGFILE
#	$MONOBIN/mono $SBBIN/MigrationTool.exe															>> $LOGFILE 2>> $LOGFILE

#	echo "Updating AudioMgr.exe.config..."															>> $LOGFILE 2>> $LOGFILE

#	echo "Updating Web.config..."																	>> $LOGFILE 2>> $LOGFILE

	echo "Restarting services..."																	>> $LOGFILE

	/sbin/service httpd reload																		>> $LOGFILE 2>> $LOGFILE
	/sbin/service speechbridged restart																>> $LOGFILE 2>> $LOGFILE

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	echo "$INSTALLED_VER" >> $SBBASE/build.txt
	echo "Install of $INSTALLED_VER complete."
} # Install632

##############################################################################################################
##############################################################################################################
Install631()
{
	echo "Starting Install631..."																	>> $LOGFILE

	echo "Extracting new files..."																	>> $LOGFILE
	tar -xjf $OLDVER4FILES -C $SBBASE																>> $LOGFILE 2>> $LOGFILE
	mv -f $SBCONF/neoinstall.sh $SBSOFT/neoinstall.sh												>> $LOGFILE 2>> $LOGFILE

	echo "Updating DB..."																			>> $LOGFILE
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script $SBCONF/SBUpdate_pgsql.sql	>> $LOGFILE 2>> $LOGFILE

	echo "Running MigrationTool..."																	>> $LOGFILE
	$MONOBIN/mono $SBBIN/MigrationTool.exe														>> $LOGFILE 2>> $LOGFILE

	echo "Updating AudioMgr.exe.config..."															>> $LOGFILE 2>> $LOGFILE

	echo "Updating Web.config..."																	>> $LOGFILE 2>> $LOGFILE

	echo "Restarting services..."																	>> $LOGFILE

	/sbin/service httpd reload																		>> $LOGFILE 2>> $LOGFILE
	/sbin/service speechbridged restart																>> $LOGFILE 2>> $LOGFILE

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	echo "$INSTALLED_VER" >> $SBBASE/build.txt
	echo "Install of $INSTALLED_VER complete."														>> $LOGFILE
} # Install631

##############################################################################################################
##############################################################################################################
Install621()
{
	echo "Starting Install621..."																	>> $LOGFILE

	echo "Extracting new files..."																	>> $LOGFILE
	tar -xjf $OLDVER3FILES -C $SBBASE																>> $LOGFILE 2>> $LOGFILE

	echo "Updating DB..."																			>> $LOGFILE
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script $SBCONF/SBUpdate_pgsql.sql	>> $LOGFILE 2>> $LOGFILE

	echo "Running MigrationTool..."																	>> $LOGFILE
	$MONOBIN/mono $SBBIN/MigrationTool.exe														>> $LOGFILE 2>> $LOGFILE

	echo "Updating DialogMgr.exe.config..."															>> $LOGFILE 2>> $LOGFILE
	sed 's:<add key="LogFilePath":<add key="VxmlLocation" value="/opt/speechbridge/VoiceDocStore/"/>\
\t\t<add key="LogFilePath":' < $SBBIN/DialogMgr.exe.config > $SBBIN/DialogMgr.exe2.config			2>> $LOGFILE
	mv -f $SBBIN/DialogMgr.exe2.config $SBBIN/DialogMgr.exe.config								>> $LOGFILE 2>> $LOGFILE

	echo "Updating Web.config..."																	>> $LOGFILE 2>> $LOGFILE
	sed 's:<location path="UserEmailProps.aspx">:<location path="LicensedFeatures.aspx">\
\t\t<system.web>\
\t\t\t<authorization>\
\t\t\t\t<allow users="admin"/>\
\t\t\t\t<deny users="*"/>\
\t\t\t</authorization>\
\t\t</system.web>\
\t</location>\
\t<location path="UserEmailProps.aspx">:' < $SBSBCONFIG/Web.config > $SBSBCONFIG/Web2.config	2>> $LOGFILE
	mv -f $SBSBCONFIG/Web2.config $SBSBCONFIG/Web.config										>> $LOGFILE 2>> $LOGFILE

	sed 's:<configuration>:<configuration>\
\t<system.web>\
\t\t<pages>\
\t\t\t<controls>\
\t\t\t\t<add tagPrefix="sb" namespace="SBConfig" assembly="SBConfig"/>\
\t\t\t</controls>\
\t\t</pages>\
\t</system.web>\
:' < $SBSBCONFIG/Web.config > $SBSBCONFIG/Web2.config											2>> $LOGFILE
	mv -f $SBSBCONFIG/Web2.config $SBSBCONFIG/Web.config										>> $LOGFILE 2>> $LOGFILE

	echo "Restarting services..."																	>> $LOGFILE

	/sbin/service httpd reload																		>> $LOGFILE 2>> $LOGFILE
	/sbin/service speechbridged restart																>> $LOGFILE 2>> $LOGFILE

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	echo "$INSTALLED_VER" >> $SBBASE/build.txt
	echo "Install of $INSTALLED_VER complete."														>> $LOGFILE
} # Install621

##############################################################################################################
##############################################################################################################
Install611()
{
	echo "Starting Install611..."																	>> $LOGFILE

	echo "Extracting new files..."																	>> $LOGFILE
	tar -xjf $OLDVER2FILES -C $SBBASE															>> $LOGFILE 2>> $LOGFILE

	echo "Fixing the FBW..."																		>> $LOGFILE
	# Copy the correct sbpostinstall.sh script(s) to the correct place(s)
	mv -f $SBCONF/sbpostinstall_FIRSTBOOT.sh $SBSOFT												>> $LOGFILE 2>> $LOGFILE
	mv -f $SBCONF/sbpostinstall_POSTFIRSTBOOT.sh $SBSOFT											>> $LOGFILE 2>> $LOGFILE
	cp -f $SBSOFT/sbpostinstall_POSTFIRSTBOOT.sh $SBSOFT/sbpostinstall.sh							>> $LOGFILE 2>> $LOGFILE
	# Make sure there is at least one language installed
	if [ `/opt/novell/mono/bin/mono /opt/speechbridge/bin/sbdbutils.exe --get-langs | wc -l` -eq 0 ]; then
		$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script $SBCONF/sblp_en-US_pgsql.sql	>> $LOGFILE 2>> $LOGFILE
	fi

	echo "Updating DB..."																			>> $LOGFILE
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script $SBCONF/SBUpdate_pgsql.sql	>> $LOGFILE 2>> $LOGFILE

	echo "Restarting services..."																	>> $LOGFILE

	/sbin/service httpd reload																		>> $LOGFILE 2>> $LOGFILE
	/sbin/service speechbridged restart																>> $LOGFILE 2>> $LOGFILE

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	echo "$INSTALLED_VER" >> $SBBASE/build.txt
	echo "Install of $INSTALLED_VER complete."														>> $LOGFILE
} # Install611

##############################################################################################################
##############################################################################################################
Install601()
{
	echo "Starting Install601..."																	>> $LOGFILE

	echo "Removing previous versions' detritus..."													>> $LOGFILE
	rm -f SBInstall_DD_pgsql.sql																	>> $LOGFILE 2>> $LOGFILE
	rm -f sbpostinstall_POSTFIRSTBOOT.sh															>> $LOGFILE 2>> $LOGFILE
	rm -f sbpostinstall.sh~																			>> $LOGFILE 2>> $LOGFILE

	echo "Extracting new files..."																	>> $LOGFILE
	tar -xjf $OLDVER1FILES -C $SBBASE															>> $LOGFILE 2>> $LOGFILE
	mv -f $SBCONF/sbpostinstall.sh $SBSOFT														>> $LOGFILE 2>> $LOGFILE

	echo "Creating new directory(ies)..."															>> $LOGFILE
	mkdir -p $SBLIC																				>> $LOGFILE 2>> $LOGFILE

	echo "Updating config files..."																	>> $LOGFILE
	sed 's:<add key="SqlConnStr":<add key="License" value="/opt/speechbridge/license/Incendonet.lic"/>\
\t\t<add key="SqlConnStr":' < $SBBIN/DialogMgr.exe.config > $SBBIN/DialogMgr.exe2.config			2>> $LOGFILE
	mv -f $SBBIN/DialogMgr.exe2.config $SBBIN/DialogMgr.exe.config								>> $LOGFILE 2>> $LOGFILE

	sed 's:<add key="SqlConnStr":<add key="License" value="/opt/speechbridge/license/Incendonet.lic"/>\
\t\t<add key="SqlConnStr":' < $SBBIN/sbdbutils.exe.config > $SBBIN/sbdbutils.exe2.config			2>> $LOGFILE
	mv -f $SBBIN/sbdbutils.exe2.config $SBBIN/sbdbutils.exe.config								>> $LOGFILE 2>> $LOGFILE

	sed 's:<add key="SqlConnStr":<add key="License" value="/opt/speechbridge/license/Incendonet.lic"/>\
\t\t<add key="SqlConnStr":' < $SBBIN/SBSched.exe.config > $SBBIN/SBSched.exe2.config				2>> $LOGFILE
	mv -f $SBBIN/SBSched.exe2.config $SBBIN/SBSched.exe.config									>> $LOGFILE 2>> $LOGFILE

	sed 's:<add key="SqlConnStr":<add key="License" value="/opt/speechbridge/license/Incendonet.lic"/>\
\t\t<add key="SqlConnStr":' < $SBSBCONFIG/Web.config > $SBSBCONFIG/Web2.config					2>> $LOGFILE
	mv -f $SBSBCONFIG/Web2.config $SBSBCONFIG/Web.config										>> $LOGFILE 2>> $LOGFILE

	echo "Updating symlinks..."																		>> $LOGFILE
	rm -f $SBSBCONFIG/assets/docs/SpeechBridgeAdminGuide.pdf										>> $LOGFILE 2>> $LOGFILE
	ln -s $SBSBCONFIG/assets/docs/SpeechBridgeAdminGuide_6-0.pdf $SBSBCONFIG/assets/docs/SpeechBridgeAdminGuide.pdf		>> $LOGFILE 2>> $LOGFILE

	echo "Updating DB..."																			>> $LOGFILE
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script $SBCONF/SBUpdate_pgsql.sql	>> $LOGFILE 2>> $LOGFILE

	echo "Running MigrationTool..."																	>> $LOGFILE
	$MONOBIN/mono $SBBIN/MigrationTool.exe														>> $LOGFILE 2>> $LOGFILE
	mv -f $SBVDS/AAMain_en-US_Template_UPDATED.vxml.xml $SBVDS/AAMain_en-US_Template.vxml.xml		>> $LOGFILE 2>> $LOGFILE
	mv -f $SBVDS/AAMain_en-AU_Template_UPDATED.vxml.xml $SBVDS/AAMain_en-AU_Template.vxml.xml		>> $LOGFILE 2>> $LOGFILE

	echo "Updating language packs..."																>> $LOGFILE
	UpdateLangPacks

	echo "Running sbdbutils to generate VoiceXML from templates..."									>> $LOGFILE
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --generate-vxml		>> $LOGFILE 2>> $LOGFILE

	echo "Restarting services..."																	>> $LOGFILE

	/sbin/service httpd reload																		>> $LOGFILE 2>> $LOGFILE
	/sbin/service speechbridged restart																>> $LOGFILE 2>> $LOGFILE

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	echo "$INSTALLED_VER" >> $SBBASE/build.txt
	echo "Install of $INSTALLED_VER complete."														>> $LOGFILE
} # Install601

##############################################################################################################
##############################################################################################################
FixPermissions()
{
	/bin/bash		$SBBASE/config/fixpermissions.sh												>> $LOGFILE 2>> $LOGFILE
}

##############################################################################################################
##############################################################################################################
#-------------------------------------------------------------------------------------------------------------------------------------------
#-------------------------------------------------------------------------------------------------------------------------------------------
INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`

# Check for Incendonet.lic before continuing
LicenseFileCheck

echo "Starting installation of $THISVER$MINORREV at `date`."										>> $LOGFILE
echo "Previously installed version of SpeechBridge is '$INSTALLED_VER.'"							>> $LOGFILE
echo ""																								>> $LOGFILE

if [ `whoami` != "root" ]; then
	echo "User was not logged in as root, aborting installation."									>> $LOGFILE
	echo "----------------------------------------------"
	echo "You must be logged in as root to upgrade SpeechBridge.  Please log in as root and run this script again."
	echo "----------------------------------------------"
	echo
	Cleanup
	exit 0
fi

# Check dependencies
if [ "$UPGRADEABLE_VER0" != "$INSTALLED_VER" ] && [ "$UPGRADEABLE_VER1" != "$INSTALLED_VER" ] && [ "$UPGRADEABLE_VER2" != "$INSTALLED_VER" ] && [ "$UPGRADEABLE_VER3" != "$INSTALLED_VER" ] && [ "$UPGRADEABLE_VER4" != "$INSTALLED_VER" ] && [ "$UPGRADEABLE_VER5" != "$INSTALLED_VER" ] && [ "$UPGRADEABLE_VER6" != "$INSTALLED_VER" ]; then
	echo "Not running a version this installer can upgrade from."									>> $LOGFILE
	echo "----------------------------------------------"
	echo "Your system must be running SpeechBridge '$UPGRADEABLE_VER0' or '$UPGRADEABLE_VER1' or '$UPGRADEABLE_VER2' or '$UPGRADEABLE_VER3' or '$UPGRADEABLE_VER4' or '$UPGRADEABLE_VER5' or '$UPGRADEABLE_VER6'"
	echo "to install this update.  This system's version is '$INSTALLED_VER'."
	echo "You can also find your version number on the SpeechBridge login web page."
	echo "Aborting the upgrade now."
	echo "----------------------------------------------"
	echo
	Cleanup
	exit 0
fi

# Issue warning if we are not running in 'silent' mode
if [ ! $INSTALLMODE == $SILENT ]; then
	echo "----------------------------------------------"
	echo "You are about to upgrade SpeechBridge to"
	echo "version $THISVER, and this will disrupt all calls"
	echo "and web access.  Do you want to continue the"
	echo "installation?"
	echo ""
	echo "WARNING:"
	echo "THIS WILL DISRUPT SPEECHBRIDGE OPERATIONS!!!"
	echo ""
	echo "  Press 'c' (without the quotes) and <Enter> to (c)ontinue, or"
	echo "  Press 'q' (without the quotes) and <Enter> to (q)uit:"
	echo ""
	read answer1
fi

# Move files where we want them
mkdir -p $SBINST
mv -f $RPM_VER1 $SBINST
mv -f speechbridge-*.tar.bz2 $SBINST

if [ "$answer1" == "c" ] || [ $INSTALLMODE == $SILENT ]; then

	echo "About to begin SpeechBridge upgrade..."													>> $LOGFILE
	echo "----------------------------------------------"
	echo ""
	echo "About to begin SpeechBridge upgrade..."
	
	# Create symlinks
	CreateSymlinks

	# We are allowing upgrades from multiple previous versions, so check for each
	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	if [ "$INSTALLED_VER" == "$UPGRADEABLE_VER0" ]; then
		echo "Installing $OLDVER0..."
		echo "About to install $RPM_VER1..."														>> $LOGFILE
		rpm -ivh --force --nodeps $SBINST/$RPM_VER1												>> $LOGFILE 2>> $LOGFILE
		echo "RPM install completed with status '$?'."												>> $LOGFILE
	fi

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	if [ "$INSTALLED_VER" == "$UPGRADEABLE_VER1" ]; then
		echo "Backing up files and database..."
		Backup
		echo "Installing $OLDVER1..."
		Install601
		FixPermissions
	fi

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	if [ "$INSTALLED_VER" == "$UPGRADEABLE_VER2" ]; then
		echo "Backing up files and database..."
		Backup
		echo "Installing $OLDVER2..."
		Install611
		FixPermissions
	fi

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	if [ "$INSTALLED_VER" == "$UPGRADEABLE_VER3" ]; then
		echo "Backing up files and database..."
		Backup
		echo "Installing $OLDVER3..."
		Install621
		FixPermissions
	fi

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	if [ "$INSTALLED_VER" == "$UPGRADEABLE_VER4" ]; then
		echo "Backing up files and database..."
		Backup
		echo "Installing $OLDVER4..."
		Install631
		FixPermissions
	fi

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	if [ "$INSTALLED_VER" == "$UPGRADEABLE_VER5" ]; then
		echo "Backing up files and database..."
		Backup
		echo "Installing $OLDVER5..."
		Install632
		FixPermissions
	fi

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	if [ "$INSTALLED_VER" != "$UPGRADEABLE_VER6" ]; then
		echo "The installation for $UPGRADEABLE_VER6 was unsuccessful, aborting."					>> $LOGFILE
		echo "----------------------------------------------"
		echo "The installation for $UPGRADEABLE_VER6 was unsuccessful.  Please"
		echo "see the log in: $LOGFILE for more details."
		echo "Aborting the upgrade now."
		echo "----------------------------------------------"
		echo
		Cleanup
		exit 0
	fi

	echo "Backing up files and database..."
	Backup
	echo "Installing $THISVER..."
	Install641
	echo "Final tweaks..."
	UpgradeTemplates
	FixPermissions

	echo ""
	echo "All done!  You can find the install"
	echo "log in: $LOGFILE"
	echo ""
	echo "----------------------------------------------"
	echo ""
	echo "Done at `date`."																			>> $LOGFILE
else
	echo "Skipping SpeechBridge upgrade."															>> $LOGFILE
	echo "- Skipping SpeechBridge upgrade."
	echo ""
fi

Cleanup
