#!/bin/sh

BUILDDASH="4-1-1"
BUILDDOT="4.1.1"

if [[ -z "$1" ]]; then
	echo "Please specify the build number as an argument."
else

	cd /home/speechbridge/src/rpmbuild
	rpmbuild -bb speechbridge_$BUILDDASH-$1.spec
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