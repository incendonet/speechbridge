#!/bin/bash
#
# Copyright 2013, All rights reserved, Incendonet Inc.
#
# sbimportlatestmenubackup.sh - SpeechBridge 
#
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBCONF=$SBBASE/config
SBLOGS=$SBBASE/logs
CONFIG_FILE="$SBCONF/sbmenupusher.conf"

[ -r "$CONFIG_FILE" ] && . "$CONFIG_FILE"

# Get the most recent export that was pushed over by sbmenupusher.sh
LATESTEXPORT=`ls $SBLOGS/dialogs_*.sql | tail -n 1`

echo "Are you sure you want to import $LATESTEXPORT?  This"
echo "will overwrite any menus already on this system."
echo ""
echo "Continue? (y/n)"
echo ""

read answer1

if [ "$answer1" = "y" ]; then
	echo "Starting import..."
	echo ""
	echo $DBPASS | $SBBIN/importdialog.sh $LATESTEXPORT
else
	echo "Import aborted."
fi

echo ""
echo "Press 'Enter' to finish."
read exit1
