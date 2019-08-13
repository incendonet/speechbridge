#!/bin/bash
set -e

SBHOME=/opt/speechbridge
SBLIC=$SBHOME/license

LVCONF=/etc/lumenvox


ln -s $SBLIC/License.bts $LVCONF/License.bts

exec lv_license_server -console

