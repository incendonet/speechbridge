#!/bin/bash
#
# Copyright 2013, All rights reserved, Incendonet Inc.
#
# sbmenupusher.sh - SpeechBridge Menu-Pusher script
# 
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBCONF=$SBBASE/config
SBLOGS=$SBBASE/logs
CONFIG_FILE="$SBCONF/sbmenupusher.conf"
OUTPUT_FILE=""
IPADDR=`ifconfig eth0 | grep 'inet addr:' | cut -d: -f2 | awk '{ print $1}'`


[ -r "$CONFIG_FILE" ] && . "$CONFIG_FILE"

REMOTE_SERVER=$SERVER_B

if [ "$IPADDR" = "$REMOTE_SERVER" ]; then
    REMOTE_SERVER=$SERVER_A
fi

# Export menus to be imported on $REMOTE_SERVER and move sql file to configs
echo $DBPASS | $SBBIN/exportdialog.sh
OUTPUT_FILE=`ls dialogs_*.sql | tail -n 1`
mv -f "$OUTPUT_FILE" $SBLOGS/"$OUTPUT_FILE"

# Copy exported menus sql file to $REMOTE_SERVER
rsync --progress -avhe ssh $SBLOGS/"$OUTPUT_FILE" root@$REMOTE_SERVER:$SBLOGS/"$OUTPUT_FILE"

# Copy VoiceXML files for menus to $REMOTE_SERVER
rsync --progress -avhe ssh $SBBASE/VoiceDocStore/*.xml root@$REMOTE_SERVER:$SBBASE/VoiceDocStore/

echo "Press 'Enter' to finish."
read exit1
