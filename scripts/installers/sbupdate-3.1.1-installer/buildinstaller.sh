#!/bin/bash

OUTFILE=sbupdate_3-1-1.bin

cd files
tar -czf ../files.tar.gz *
#tar -cf ../files.tar *
#gzip ../files.tar

cd ..
#cat extract.sh files.tar.gz > $1
cat extract.sh files.tar.gz > $OUTFILE
chown speechbridge:speechbridge $OUTFILE
chmod ug+x $OUTFILE
