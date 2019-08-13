#!/bin/bash
set -e

LOCALBIN=/usr/local/bin
LOCALETC=/usr/local/etc

LVCONF=/etc/lumenvox

# Generate config files with environment variable replacement
$LOCALBIN/envsub.sh < $LOCALETC/client_property.conf.TEMPLATE > $LVCONF/client_property.conf 


localedef -i en_US -f ISO-8859-1 en_US

exec lv_sre_server -console
