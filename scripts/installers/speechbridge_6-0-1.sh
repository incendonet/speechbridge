#!/bin/bash

# tar with:		tar -cPf foo.tar fullpath
# untar with:  tr -xPf foo.tar

#---------------------------------------------------------------------------------------------------------
# Installer script
#---------------------------------------------------------------------------------------------------------
TODAYSTR="$(/bin/date --date=today +%Y%m%d)"

THISVER0="5.1.1"
THISVER1="6.0.1"
THISVERDASH="6-0-1"
OLDVERDASH0="5-0-1"
OLDVERDASH1="5-1-1"
#MINORREV="a"

SBHOME=/home/speechbridge
SBSOFT=$SBHOME/software
SBINST=$SBSOFT/speechbridge-$THISVER1
SBBASE=/opt/speechbridge
SBBAK=/$SBBASE/backup/speechbridge-$THISVERDASH
SBBIN=$SBBASE/bin
SBCONF=$SBBASE/config
SBLIC=$SBBASE/license
SBLOGS=$SBBASE/logs
SBSBCONFIG=$SBBASE/SBConfig
SBVDS=$SBBASE/VoiceDocStore
LOGFILE=$SBLOGS/InstallLog_speechbridge_$THISVER1.log
BAKFILE=$SBLOGS/sbbackup_$TODAYSTR.tar.gz
THISFILES=$SBINST/speechbridge-$THISVER1.tar.bz2
HTTPDCONF=/etc/httpd/conf
MONOBIN=/opt/novell/mono/bin
SILENT="--silent"
UPGRADEABLE_VER0="5.0.1.13242"
UPGRADEABLE_VER1="5.1.1.69"
RPM_VER1="speechbridge-5.1.1-69.i686.rpm"
INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
INSTALLMODE="$1"
TEMPLATESDIFFER=""
TEMPLATEUPGRADED=0


Cleanup()
{
	echo "Running Cleanup..."																		>> $LOGFILE
	rm -Rf $SBINST/*
	rm -f $SBINST/AAMain_Template_$OLDVERDASH_ORIG.vxml.xml
	echo "Finished Cleanup."																		>> $LOGFILE
}

Backup()
{
	mkdir -p $SBBAK
	
	echo "Backing up the current DB..."																>> $LOGFILE
	/bin/bash $SBBIN/sbbackup.cron
	mv -f $BAKFILE $SBBAK/sbdb-bak_$TODAYSTR.tar.gz													>> $LOGFILE 2>> $LOGFILE
	
	echo "Backing up the files in the directories affected by the installer..."						>> $LOGFILE
	tar -cjf $SBBAK/sbbak_before_$THISVER1_$TODAYSTR.tar.bz2 $SBBIN $SBCONF $SBSBCONFIG $SBVDS		>> $LOGFILE 2>> $LOGFILE

	echo "Backups completed."																		>> $LOGFILE
}

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
			cp -f $SBVDS/ABNFBoolean_en-US.gram		$SBVDS/ABNFBoolean_en-AU.gram					>> $LOGFILE 2>> $LOGFILE
			cp -f $SBVDS/ABNFDate_en-US.gram		$SBVDS/ABNFDate_en-AU.gram						>> $LOGFILE 2>> $LOGFILE
			cp -f $SBVDS/ABNFDigits_en-US.gram		$SBVDS/ABNFDigits_en-AU.gram					>> $LOGFILE 2>> $LOGFILE
		fi
		ln -s $SBVDS/ABNFBoolean_en-AU.gram			$SBVDS/ABNFBoolean.gram							>> $LOGFILE 2>> $LOGFILE
		ln -s $SBVDS/ABNFBooleanSA_en-AU.gram		$SBVDS/ABNFBooleanSA.gram						>> $LOGFILE 2>> $LOGFILE	# This file is new with 6.0.1
		ln -s $SBVDS/ABNFDate_en-AU.gram			$SBVDS/ABNFDate.gram							>> $LOGFILE 2>> $LOGFILE
		ln -s $SBVDS/ABNFDigits_en-AU.gram			$SBVDS/ABNFDigits.gram							>> $LOGFILE 2>> $LOGFILE

		echo "Removing leftover files..."
		rm -f $SBVDS/ABNFBooleanSA_en-US.gram														>> $LOGFILE 2>> $LOGFILE
	fi
	
	echo "Done updating Language Packs"																>> $LOGFILE
}

Install601()
{
	echo "Starting Install601..."																	>> $LOGFILE

	echo "Removing previous versions' detritus..."													>> $LOGFILE
	rm -f SBInstall_DD_pgsql.sql																	>> $LOGFILE 2>> $LOGFILE
	rm -f sbpostinstall_POSTFIRSTBOOT.sh															>> $LOGFILE 2>> $LOGFILE
	rm -f sbpostinstall.sh~																			>> $LOGFILE 2>> $LOGFILE

	echo "Extracting new files..."																	>> $LOGFILE
	tar -xjf $THISFILES -C $SBBASE																	>> $LOGFILE 2>> $LOGFILE
	mv -f $SBCONF/sbpostinstall.sh $SBSOFT															>> $LOGFILE 2>> $LOGFILE
	
	echo "Creating new directory(ies)..."															>> $LOGFILE
	mkdir -p $SBLIC																				>> $LOGFILE 2>> $LOGFILE
	
	echo "Updating config files..."																	>> $LOGFILE
	sed 's:<add key="SqlConnStr":<add key="License" value="/opt/speechbridge/license/Incendonet.lic"/>\
\t\t<add key="SqlConnStr":' < $SBBIN/DialogMgr.exe.config > $SBBIN/DialogMgr.exe2.config			2>> $LOGFILE
	mv -f $SBBIN/DialogMgr.exe2.config $SBBIN/DialogMgr.exe.config									>> $LOGFILE 2>> $LOGFILE

	sed 's:<add key="SqlConnStr":<add key="License" value="/opt/speechbridge/license/Incendonet.lic"/>\
\t\t<add key="SqlConnStr":' < $SBBIN/sbdbutils.exe.config > $SBBIN/sbdbutils.exe2.config				2>> $LOGFILE
	mv -f $SBBIN/sbdbutils.exe2.config $SBBIN/sbdbutils.exe.config										>> $LOGFILE 2>> $LOGFILE

	sed 's:<add key="SqlConnStr":<add key="License" value="/opt/speechbridge/license/Incendonet.lic"/>\
\t\t<add key="SqlConnStr":' < $SBBIN/SBSched.exe.config > $SBBIN/SBSched.exe2.config				2>> $LOGFILE
	mv -f $SBBIN/SBSched.exe2.config $SBBIN/SBSched.exe.config										>> $LOGFILE 2>> $LOGFILE

	sed 's:<add key="SqlConnStr":<add key="License" value="/opt/speechbridge/license/Incendonet.lic"/>\
\t\t<add key="SqlConnStr":' < $SBSBCONFIG/Web.config > $SBSBCONFIG/Web2.config						2>> $LOGFILE
	mv -f $SBSBCONFIG/Web2.config $SBSBCONFIG/Web.config											>> $LOGFILE 2>> $LOGFILE
	
	echo "Updating symlinks..."																		>> $LOGFILE
	rm -f $SBSBCONFIG/assets/docs/SpeechBridgeAdminGuide.pdf										>> $LOGFILE 2>> $LOGFILE
	ln -s $SBSBCONFIG/assets/docs/SpeechBridgeAdminGuide_6-0.pdf $SBSBCONFIG/assets/docs/SpeechBridgeAdminGuide.pdf		>> $LOGFILE 2>> $LOGFILE
	
	echo "Updating DB..."																			>> $LOGFILE
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --run-script $SBCONF/SBUpdate_pgsql.sql	>> $LOGFILE 2>> $LOGFILE

	echo "Running MigrationTool..."																	>> $LOGFILE
	$MONOBIN/mono $SBBIN/MigrationTool.exe															>> $LOGFILE 2>> $LOGFILE
	mv -f $SBVDS/AAMain_en-US_Template_UPDATED.vxml.xml $SBVDS/AAMain_en-US_Template.vxml.xml		>> $LOGFILE 2>> $LOGFILE
	mv -f $SBVDS/AAMain_en-AU_Template_UPDATED.vxml.xml $SBVDS/AAMain_en-AU_Template.vxml.xml		>> $LOGFILE 2>> $LOGFILE
	
	echo "Updating language packs..."																>> $LOGFILE
	UpdateLangPacks

	echo "Running sbdbutils to generate VoiceXML from templates..."									>> $LOGFILE
	$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --generate-vxml			>> $LOGFILE 2>> $LOGFILE

	echo "Restarting services..."																	>> $LOGFILE

	/sbin/service httpd reload																		>> $LOGFILE 2>> $LOGFILE
	/sbin/service speechbridged restart																>> $LOGFILE 2>> $LOGFILE

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	echo "$INSTALLED_VER" >> $SBBASE/build.txt
	echo "Install of $INSTALLED_VER complete." >> $LOGFILE
}

FixPermissions()
{
	/bin/bash		$SBBASE/config/fixpermissions.sh												>> $LOGFILE 2>> $LOGFILE
}

#-------------------------------------------------------------------------------------------------------------------------------------------
#-------------------------------------------------------------------------------------------------------------------------------------------
INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`

echo "Starting installation of $THISVER1$MINORREV at `date`." >> $LOGFILE
echo "Previously installed version of SpeechBridge is '$INSTALLED_VER.'" >> $LOGFILE
echo "" >> $LOGFILE

if [ `whoami` != "root" ]; then
	echo "User was not logged in as root, aborting installation." >> $LOGFILE
	echo "----------------------------------------------"
	echo "You must be logged in as root to upgrade SpeechBridge.  Please log in as root and run this script again."
	echo "----------------------------------------------"
	echo
	Cleanup
	exit 0
fi

# Check dependencies
if [ "$UPGRADEABLE_VER0" != "$INSTALLED_VER" ] && [ "$UPGRADEABLE_VER1" != "$INSTALLED_VER" ]; then
	echo "Not running a version this installer can upgrade from." >> $LOGFILE
	echo "----------------------------------------------"
	echo "Your system must be running SpeechBridge '$UPGRADEABLE_VER0' or '$UPGRADEABLE_VER1'"
	echo "to install this update.  This system's version is '$INSTALLED_VER'."
	echo "You can also find your version number on the SpeechBridge login web page."
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
	echo "version $THISVER1, and this will disrupt all calls"
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
mv -f $RPM_VER1 $SBINST
mv -f speechbridge-$THISVER1.tar.bz2 $SBINST

if [ "$answer1" == "yes" ] || [ $INSTALLMODE = $SILENT ]; then

	echo "About to begin SpeechBridge upgrade..." >> $LOGFILE
	echo "----------------------------------------------"
	echo ""
	echo "About to begin SpeechBridge upgrade..."

	# We are allowing upgrades from multiple previous versions, so check for each
	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	if [ "$INSTALLED_VER" == "$UPGRADEABLE_VER0" ]; then
		echo "Installing $THISVER0..."
		echo "About to install $RPM_VER1..." >> $LOGFILE
		rpm -ivh --force --nodeps $SBINST/$RPM_VER1 >> $LOGFILE 2>> $LOGFILE
		echo "RPM install completed with status '$?'." >> $LOGFILE
	fi

	INSTALLED_VER=`$MONOBIN/mono --config $SBBIN/sbdbutils.exe.config $SBBIN/sbdbutils.exe --get-swver SpeechBridge`
	if [ "$INSTALLED_VER" != "$UPGRADEABLE_VER1" ]; then
		echo "The installation for $UPGRADEABLE_VER0 was unsucessful, aborting." >> $LOGFILE
		echo "----------------------------------------------"
		echo "The installation for $UPGRADEABLE_VER0 was unsucessful."
		echo "Aborting the upgrade now."
		echo "----------------------------------------------"
		echo
		Cleanup
		exit 0
	fi

	echo "Backing up files and database..."
	Backup
	echo "Installing $THISVER1..."
	Install601
	echo "Final tweaks..."
	FixPermissions

	echo ""
	echo "All done!  You can find the install"
	echo "log in: $LOGFILE"
	echo ""
	echo "If you haven't done so already, please copy"
	echo "your license(s) to: $SBLIC"
	echo ""
	echo "----------------------------------------------"
	echo ""
	echo "Done at `date`." >> $LOGFILE
else
	echo "Skipping SpeechBridge upgrade." >> $LOGFILE
	echo "- Skipping SpeechBridge upgrade."
	echo ""
fi

Cleanup
