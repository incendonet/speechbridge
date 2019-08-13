#!/bin/bash

set -e

SBHOME=/opt/speechbridge
SBBIN=$SBHOME/bin
SBCFG=$SBHOME/config

LVCONF=/etc/lumenvox

# Generate config files with environment variable replacement
$SBBIN/envsub.sh < $SBCFG/client_property.conf.TEMPLATE > $LVCONF/client_property.conf 
