#!/bin/bash

SILENT="--silent"
INSTALLMODE="$1"

export SBHOME=/home/speechbridge
CWD=`pwd`
ARCHIVE=`awk '/^__MARK__/ {print NR + 1; exit 0; }' $0`

if [ ! $INSTALLMODE ]; then
    INSTALLMODE="--prompt"
fi

tail -n+$ARCHIVE $0 | tar -xzv -C $SBHOME

cd $SBHOME
./sbupdate_3-1-1.sh $INSTALLMODE

cd $CWD

if [ $INSTALLMODE != $SILENT ]; then
    echo "Press 'Enter' to finish."
    read exit1
fi

exit 0

__MARK__
