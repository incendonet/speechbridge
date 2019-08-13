#!/bin/bash

OUTFILE=speechbridge_6-4-1.bin

cd files
tar -czf ../files.tar.gz *

cd ..
cat extract.sh files.tar.gz > $OUTFILE
chown speechbridge:speechbridge $OUTFILE
chmod ug+x $OUTFILE
