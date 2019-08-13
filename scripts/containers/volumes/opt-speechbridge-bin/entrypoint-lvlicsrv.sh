#!/bin/bash
set -e

LOCALBIN=/usr/local/bin
LOCALETC=/usr/local/etc
SBLIC=/opt/speechbridge/license

LVCONF=/etc/lumenvox

# Generate config files with environment variable replacement
${LOCALBIN}/envsub.sh < ${LOCALETC}/client_property.conf.TEMPLATE > ${LVCONF}/client_property.conf 

# Link to LV license file
rm -f ${LVCONF}/License.bts
ln -s ${SBLIC}/License.bts ${LVCONF}/License.bts

exec lv_license_server -console
