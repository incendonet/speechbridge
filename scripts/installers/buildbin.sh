#!/bin/sh

cd /home/speechbridge/src/rpmbuild
rpmbuild -bb sbupdate_3-0-1-0.spec
tar -czf sbupdate_3-0-1-0_files.tar.gz *3.0.1-*buildroot *3-0-1*.spec /usr/src/redhat/RPMS/i686/sbupdate-3.0.1-*.i686.rpm
mv /usr/src/redhat/RPMS/i686/sbupdate-3.0.1*.rpm ./sbupdate-3.0.1-installer/files
cd sbupdate-3.0.1-installer
./buildinstaller.sh
mv *.bin ..
cd ..
chmod ug+x *.bin
tar -czf sbupdate_3-0-1_bin.tar.gz sbupdate_3-0-1.bin

echo "Done!"
