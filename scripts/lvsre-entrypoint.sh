#!/bin/bash
set -e

SBHOME=/opt/speechbridge

LVCONF=/etc/lumenvox


exec lv_sre_server -console
