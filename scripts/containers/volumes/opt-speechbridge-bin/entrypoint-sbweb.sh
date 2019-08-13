#!/bin/bash

set -e

MONOBIN=/usr/bin/mono
XSP=/usr/bin/xsp4
SBHOME=/opt/speechbridge
SBBIN=$SBHOME/bin
SBCONFIG=$SBHOME/SBConfig

# Generate config file with environment variable replacement
$SBBIN/envsub.sh < $SBCONFIG/Web.config.TEMPLATE > $SBCONFIG/Web.config

# Wait for Postgres to be ready
until PGPASSWORD=$POSTGRES_SBPASSWORD psql -h "postgres" -d "dspeechbridge" -U "$POSTGRES_SBUSER" -c '\l'; do
  >&2 echo "Postgres is unavailable - sleeping"
  sleep 1
done

# Start XSP
#   Params:  (See `xsp4 --help`)
#     --logfile=VALUE
#     --loglevels=VALUE
#     --nonstop
#     --https
#     --port=VALUE
#     --protocols=VALUE
cd $SBCONFIG
$XSP --port=80 --nonstop
#$XSP --port=443 --https --nonstop
