#!/bin/bash
set -e

SBHOME=/opt/speechbridge
SBBIN=$SBHOME/bin
ASTCONF=/etc/asterisk

echo "$SIPPROXYADDR speechbridge1" >> /etc/hosts
$SBBIN/envsub.sh < $ASTCONF/pjsip.conf.TEMPLATE > $ASTCONF/pjsip.conf
$SBBIN/envsub.sh < $ASTCONF/rtp.conf.TEMPLATE > $ASTCONF/rtp.conf

exec asterisk -fvvv
