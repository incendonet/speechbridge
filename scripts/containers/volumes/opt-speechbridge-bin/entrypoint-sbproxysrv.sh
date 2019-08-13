#!/bin/bash

set -e

SBHOME=/opt/speechbridge
SBBIN=$SBHOME/bin
SBCFG=$SBHOME/config
SBLOG=$SBHOME/logs

until [ -e ${SBCFG}/ProxySrv.config ]; do
  >&2 echo "Waiting for ProxySrv.config to be written - sleeping..."
  sleep 1
done

# Start ProxySrv.
$SBBIN/ProxySrv -x -i $SBCFG/ProxySrv.config -l $SBLOG/ProxySrv.log
