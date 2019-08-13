#!/bin/sh

cd /home/speechbridge/src/rpmbuild
rpmbuild -bb sbupdate_3-1-1-0.spec
mv /usr/src/redhat/RPMS/i686/sbupdate-3.1.1-3.1.1-$1.i686.rpm /usr/src/redhat/RPMS/i686/sbupdate-3.1.1-$1.i686.rpm
tar -czf sbupdate_3-1-1-0_files.tar.gz *3.1.1-*buildroot *3-1-1*.spec /usr/src/redhat/RPMS/i686/sbupdate-3.1.1-$1.i686.rpm
mv /usr/src/redhat/RPMS/i686/sbupdate-3.1.1-$1.i686.rpm ./sbupdate-3.1.1-installer/files
cd sbupdate-3.1.1-installer
./buildinstaller.sh
mv *.bin ..
cd ..
chmod ug+x *.bin
tar -czf sbupdate_3-1-1_bin.tar.gz sbupdate_3-1-1.bin

echo "Done!"
