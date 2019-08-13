#!/bin/sh

BUILDDASH="6-4-1"
BUILDDOT="6.4.1"
BUILDDIR=/home/speechbridge/src/rpmbuild
SBDIR=$BUILDDIR/speechbridge-$BUILDDOT-buildroot/opt/speechbridge
CREDSFILE=$BUILDDIR/buildbin-creds.pwd
ARTIFACTSTORSRV="nas1.incendonethq.local"
ARTIFACTSTORDIR="builds/speechbridge/$BUILDDOT"

GITUSER=""
GITPASS=""
SMBUSER=""
SMBPASS=""

if [ ! -f $CREDSFILE ]; then
	echo ""
	echo "ERROR: Credentials file not found: $CREDSFILE"
	echo ""
	exit 0
fi

[ -r "$CREDSFILE" ] && . "$CREDSFILE"

GITPATH="https://$GITUSER:$GITPASS@incendonet.visualstudio.com/DefaultCollection/_git/speechbridge/"

Init ()
{
	# Delete the old repo and do a sparse checkout of SBConfig
	rm -Rf $SBDIR/*
	rmdir $SBDIR
	mkdir -p $SBDIR
}

CleanupTemp ()
{
	cd $SBDIR
	rm -Rf scripts
	rm -Rf .git
}

GetSbconfig ()
{
	cd $SBDIR
	
	git init
	git remote add -f speechbridge $GITPATH
	git config core.sparsecheckout true
	echo SBConfig >>					.git/info/sparse-checkout
	echo SBConfig/assets >>				.git/info/sparse-checkout
	echo SBConfig/assets/content >>		.git/info/sparse-checkout
	echo SBConfig/assets/docs >>		.git/info/sparse-checkout
	echo SBConfig/assets/images >>		.git/info/sparse-checkout
	echo SBConfig/assets/js >>			.git/info/sparse-checkout
	git pull $GITPATH

	# Delete files we don't want to include
	rm -f SBConfig/*.cs
	rm -f SBConfig/build
	rm -f SBConfig/Web*.config
	rm -f SBConfig/SBConfig.csproj
	rm -f SBConfig/index-rootdir.html
	rm -f SBConfig/assets/docs/SpeechBridgeUserGuide_4-0.pdf
	rm -f SBConfig/assets/docs/SpeechBridgeUserCheatSheet-Side2_4-0.pdf
	rm -f SBConfig/assets/docs/SpeechBridgeUserCheatSheet-Side1_4-0.pdf
	rm -f SBConfig/assets/docs/SpeechBridgeAdminGuide_4-0.pdf
	rm -f SBConfig/assets/docs/SpeechBridgeAdminGuide_4-1.pdf
	rm -f SBConfig/assets/docs/SpeechBridgeAdminGuide_4-2.pdf
	rm -f SBConfig/assets/docs/SpeechBridgeAdminGuide_5-0.pdf
}

GetLatestIndividualFiles ()
{
	# Checkout just the files we want from the scripts directory
	cd $SBDIR

	git fetch $GITPATH
	
	GetLatestSBUpdatePgsql
	GetLatestVoiceXML
	GetLatestPrompts
	GetLatestBinScripts
}

GetLatestSBUpdatePgsql ()
{
	cd $SBDIR
	mkdir config

	git checkout FETCH_HEAD -- scripts/SBCreate_pgsql.sql
	git checkout FETCH_HEAD -- scripts/SBUpdate_pgsql.sql
	
	mv -f scripts/*.sql config/
}

GetLatestVoiceXML ()
{
	cd $SBDIR

	git checkout FETCH_HEAD -- VoiceDocStore/AAMain_en-US_Template.vxml.xml
	git checkout FETCH_HEAD -- VoiceDocStore/AAMain_en-US_Template_6-3-2.vxml.xml
	git checkout FETCH_HEAD -- VoiceDocStore/AAMain_en-US_Template_6-3-2a.vxml.xml
	git checkout FETCH_HEAD -- VoiceDocStore/AAMain_en-AU_Template.vxml.xml
	git checkout FETCH_HEAD -- VoiceDocStore/SB-NumberDialer_Template.vxml.xml
	git checkout FETCH_HEAD -- VoiceDocStore/SB-NumberDialer_Template_6-3-2.vxml.xml
	
	mv VoiceDocStore/AAMain_en-US_Template.vxml.xml VoiceDocStore/AAMain_en-US_Template_$BUILDDASH.vxml.xml
	mv VoiceDocStore/AAMain_en-AU_Template.vxml.xml VoiceDocStore/AAMain_en-AU_Template_$BUILDDASH.vxml.xml
	mv VoiceDocStore/SB-NumberDialer_Template.vxml.xml VoiceDocStore/SB-NumberDialer_Template_$BUILDDASH.vxml.xml
}

GetLatestPrompts ()
{
	cd $SBDIR
	mkdir -p VoiceDocStore/Prompts

	git checkout FETCH_HEAD -- VoiceDocStore/Prompts/sbprompts_$BUILDDASH.tar.bz2
}

GetLatestBinScripts ()
{
	cd $SBDIR
	mkdir bin

	git checkout FETCH_HEAD -- scripts/sbfailover.sh
	git checkout FETCH_HEAD -- scripts/sbmergedailylogs.sh
	git checkout FETCH_HEAD -- scripts/sbreportgen.cron
	git checkout FETCH_HEAD -- scripts/sbrmoldlogs.cron
	git checkout FETCH_HEAD -- scripts/installers/sbpostinstall_FIRSTBOOT.sh
	git checkout FETCH_HEAD -- scripts/installers/sbpostinstall_POSTFIRSTBOOT.sh

	mv scripts/sbfailover.sh								bin/sbfailover.sh
	mv scripts/sbmergedailylogs.sh							bin/sbmergedailylogs.sh
	mv scripts/sbreportgen.cron								bin/sbreportgen.cron
	mv scripts/sbrmoldlogs.cron								bin/sbrmoldlogs.cron
	mv scripts/installers/sbpostinstall_FIRSTBOOT.sh		bin/sbpostinstall_FIRSTBOOT.sh
	mv scripts/installers/sbpostinstall_POSTFIRSTBOOT.sh	bin/sbpostinstall_POSTFIRSTBOOT.sh
}

GetSbbins ()
{
	cd $SBDIR/bin

	# Copy files built locally - NOTE:  Release specific!
	cp /home/speechbridge/src/poco/poco-1.7.3/lib/Linux/i686/libPocoFoundation.so.43 .
	cp /home/speechbridge/src/poco/poco-1.7.3/Foundation/samples/SBLauncher/bin/Linux/i686/SBLauncher .

	# Get .Net assemblied built on Windows server
	SMBGET="smbget --dots --nonprompt --username=$SMBUSER --workgroup=INCENDONETHQ --password=$SMBPASS"
	BUILTBINS="smb://build2.incendonethq.local/bin/dotfuscated"
	rm -f *.exe
	rm -f *.dll
	$SMBGET $BUILTBINS/asrtest.exe
	$SMBGET $BUILTBINS/AudioMgr.exe
	$SMBGET $BUILTBINS/CleanNamesFolder.exe
	$SMBGET $BUILTBINS/DialogMgr.exe
	$SMBGET $BUILTBINS/MigrationTool.exe
	$SMBGET $BUILTBINS/sbdbutils.exe
	$SMBGET $BUILTBINS/SBLocalRM.exe
	$SMBGET $BUILTBINS/SBSched.exe
	$SMBGET $BUILTBINS/sbtest.exe
	$SMBGET $BUILTBINS/AppGenerator.dll
	$SMBGET $BUILTBINS/AsrFacadeLumenvox.dll
	$SMBGET $BUILTBINS/AsrFacadeNull.dll
	$SMBGET $BUILTBINS/Incendonet.Utilities.LogClient.dll
	$SMBGET $BUILTBINS/Incendonet.Utilities.StringHelper.dll
	$SMBGET $BUILTBINS/Incendonet.Plugins.UserDirectory.dll
	$SMBGET $BUILTBINS/ISMessaging.dll
	$SMBGET $BUILTBINS/PromptSelector.dll
	$SMBGET $BUILTBINS/SBConfig.dll
	$SMBGET $BUILTBINS/SBConfigStor.dll
	$SMBGET $BUILTBINS/SBEmail.dll
	$SMBGET $BUILTBINS/SBLdapConn.dll
	$SMBGET $BUILTBINS/SBResourceMgr.dll
	$SMBGET $BUILTBINS/SBTTS.dll
	$SMBGET $BUILTBINS/SimpleAES.dll
	$SMBGET $BUILTBINS/XmlDocParser.dll
}

if [[ -z "$1" ]]; then
	echo "Please specify the build number as an argument."
else

	Init

	echo ""
	echo "Getting web code..."
	GetSbconfig
	
	echo ""
	echo "Getting other files..."
	GetLatestIndividualFiles

	echo ""
	echo "Getting binaries from Jenkins..."
	GetSbbins

	echo ""
	echo "Cleaning up temporary files..."
	CleanupTemp

#	echo ""
#	echo "Running rpmbuild..."
#	cd $BUILDDIR
#	rpmbuild -bb speechbridge_$BUILDDASH.spec --define "BUILDNUM $1"

	echo ""
	echo "Creating the installation tarball..."
	cd $SBDIR
	TARBALL=speechbridge-$BUILDDOT.tar.bz2
	tar -cjf ../../../$TARBALL *

	echo ""
	echo "Packaging installer..."
	cd $BUILDDIR
	tar -cjf speechbridge_$BUILDDASH\_files.tar.bz2 speechbridge*$BUILDDOT-*buildroot
	mv $BUILDDIR/$TARBALL ./speechbridge-$BUILDDOT-installer/files
	cd speechbridge-$BUILDDOT-installer
	./buildinstaller.sh
	mv *.bin ..
	cd ..
	chmod ug+x *.bin
	tar -cjf speechbridge_"$BUILDDASH"_bin.tar.bz2 speechbridge_"$BUILDDASH".bin
	
	echo "Pushing installer to $ARTIFACTSTORSRV"
	smbclient "//$ARTIFACTSTORSRV/Engineering" "$SMBPASS" -c "cd $ARTIFACTSTORDIR; put speechbridge_"$BUILDDASH"_bin.tar.bz2; put speechbridge_"$BUILDDASH"_files.tar.bz2" -U$SMBUSER -WINCENDONETHQ

	echo ""
	echo "Done!"
	echo ""
fi
