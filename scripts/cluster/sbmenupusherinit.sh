#!/bin/bash
#
# Copyright 2013, All rights reserved, Incendonet Inc.
#
# sbmenupusherinit.sh - SpeechBridge Menu-Pusher initialization
#
IPADDR=`ifconfig eth0 | grep 'inet addr:' | cut -d: -f2 | awk '{ print $1}'`
CONFIG_FILE="/opt/speechbridge/config/sbmenupusher.conf"

[ -r "$CONFIG_FILE" ] && . "$CONFIG_FILE"

REMOTE_SERVER=$SERVER_B

if [ "$IPADDR" = "$REMOTE_SERVER" ]; then
    REMOTE_SERVER=$SERVER_A
fi

rm -f ~/Desktop/SBMenuPusher > /dev/null
ln -s /opt/speechbridge/bin/sbmenupusher.sh ~/Desktop/"SB Menu Pusher"
ln -s /opt/speechbridge/bin/sbimportlatestmenubackup.sh ~/Desktop/"SB Import Latest Menu Backup"

ssh-keygen -t rsa -b 2048 -N "" -f ~/.ssh/id_rsa
ssh-copy-id -i ~/.ssh/id_rsa.pub root@$REMOTE_SERVER		# type yes and the password when prompted
# You should now be able to `ssh root@SERVER` without a entering a password
