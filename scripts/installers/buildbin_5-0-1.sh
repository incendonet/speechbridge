#!/bin/sh

BUILDDASH="5-0-1"
BUILDDOT="5.0.1"

GetSbconfig ()
{
    cd speechbridge-$BUILDDOT-$1-buildroot/opt/speechbridge/installer/SBConfig
    rm -Rf ../SBConfig/*
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
    rm -f SpeechBridgeAdminGuide_4-0.pdf
    rm -f SpeechBridgeAdminGuide_4-1.pdf
    rm -f SpeechBridgeAdminGuide_4-2.pdf
    cd ../images
    rm -Rf .svn
    cd ../js
    rm -Rf .svn
}

if [[ -z "$1" ]]; then
	echo "Please specify the build number as an argument."
else

	cd /home/speechbridge/src/rpmbuild
    GetSbconfig $1
	cd /home/speechbridge/src/rpmbuild
	rpmbuild -bb speechbridge_$BUILDDASH.spec
	tar -czf speechbridge_$BUILDDASH-0_files.tar.gz speechbridge*$BUILDDOT-*buildroot *$BUILDDASH*.spec /usr/src/redhat/RPMS/i686/speechbridge-$BUILDDOT-$1.i686.rpm
	mv /usr/src/redhat/RPMS/i686/speechbridge-$BUILDDOT-$1.i686.rpm ./speechbridge-$BUILDDOT-$1-installer/files
	cd speechbridge-$BUILDDOT-$1-installer
	./buildinstaller.sh
	mv *.bin ..
	cd ..
	chmod ug+x *.bin
	tar -czf speechbridge_"$BUILDDASH"_bin.tar.gz speechbridge_"$BUILDDASH".bin
	tar -cjf speechbridge_"$BUILDDASH"_bin.tar.bz2 speechbridge_"$BUILDDASH".bin

	echo "Done!"
fi