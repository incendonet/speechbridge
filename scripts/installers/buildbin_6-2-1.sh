#!/bin/sh

BUILDDASH="6-2-1"
BUILDDOT="6.2.1"
BUILDDIR=/home/speechbridge/src/rpmbuild

GetSbconfig ()
{
#	cd $BUILDDIR/speechbridge-$BUILDDOT-buildroot/opt/speechbridge/SBConfig
	cd $BUILDDIR/speechbridge-$BUILDDOT-buildroot/opt/speechbridge/SBConfig
	rm -Rf ../SBConfig/*
#	 svn checkout https://build2.incendonethq.local/svn/SpeechBridge/branches/5.1.1_Release/SBConfig . --no-auth-cache --non-interactive --username builduser --password "justF)Rnvs"
	svn checkout https://build2.incendonethq.local/svn/SpeechBridge/trunk/SBConfig . --no-auth-cache --non-interactive --username builduser --password "justF)Rnvs"
	rm -Rf .svn
	rm -f *.cs
	rm -f build
	rm -f Web*.config
	rm -f SBConfig.csproj
	rm -f index-rootdir.html
	cd assets
	rm -Rf .svn
	cd content
	rm -Rf .svn
	cd ../docs
	rm -Rf .svn
	rm -f SpeechBridgeUserGuide_4-0.pdf
	rm -f SpeechBridgeUserCheatSheet-Side2_4-0.pdf
	rm -f SpeechBridgeUserCheatSheet-Side1_4-0.pdf
	rm -f SpeechBridgeAdminGuide_4-0.pdf
	rm -f SpeechBridgeAdminGuide_4-1.pdf
	rm -f SpeechBridgeAdminGuide_4-2.pdf
	rm -f SpeechBridgeAdminGuide_5-0.pdf
	cd ../images
	rm -Rf .svn
	cd ../js
	rm -Rf .svn
}

GetSbbins ()
{
	cd $BUILDDIR/speechbridge-$BUILDDOT-buildroot/opt/speechbridge/bin
	SMBGET="smbget --dots --nonprompt --username=builduser --workgroup=INCENDONETHQ --password=justF)Rnvs"
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

	cd $BUILDDIR
	
	echo ""
	echo "Getting web code from SVN..."
	cd $BUILDDIR
	GetSbconfig $1

	echo ""
	echo "Getting binaries from Jenkins..."
	cd $BUILDDIR
	GetSbbins $1

#	echo ""
#	echo "Running rpmbuild..."
#	cd $BUILDDIR
#	rpmbuild -bb speechbridge_$BUILDDASH.spec --define "BUILDNUM $1"

	echo ""
	echo "Creating the installation tarball..."
	cd $BUILDDIR/speechbridge-$BUILDDOT-buildroot/opt/speechbridge
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

	echo ""
	echo "Done!"
	echo ""
fi
