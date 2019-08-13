#!/bin/bash

##############################################################
# On host
##############################################################

# Set and get IP address
hostname speechbridge1
export HOSTIPADDR=`hostname -I | awk '{ print $1; exit }'`

# General:
export HOSTSBHOME=/usr/local/opt/incendonet
export HOSTVOLUMES=${HOSTSBHOME}/volumes

# Postgres:
export SBPGDATA=${HOSTVOLUMES}/var-lib-postgresql-data-pgdata
export SBPGLOG=${HOSTVOLUMES}/var-log-postgresql

# LumenVox
export SBLVLICLOG=${HOSTVOLUMES}/var-log-lvlic
export SBLVSRELOG=${HOSTVOLUMES}/var-log-lvsre
export SBLVCLIENTLOG=${HOSTVOLUMES}/var-log-lvclient

##############################################################
# Passed into containers
##############################################################

# SpeechBridge:
export SBLICENSESERVER_IP=${HOSTIPADDR}     # SB license server IP
export SBLICENSESERVER_PORT=60000           # SB license server port

# Postgres:
export PGDATA=/var/lib/postgresql/data/pgdata
export PGLOG=/var/log/postgresql

export POSTGRES_DB=postgres   # optional
export POSTGRES_SBDB=dspeechbridge
#export POSTGRES_INITDB_ARGS="" # optional       # Args to `postgresql initdb`

# Asterisk
export SIPPROXYADDR=${HOSTIPADDR}
export LOCALNET=${HOSTLOCALNET}     # Something similar to:  192.168.1.0/24
export SIPPORT=5060
export SSIPPORT=5061
export RTPFIRSTPORT=10000
export RTPLASTPORT=10099

# LumenVox
export LVLICSRV=${HOSTIPADDR}
export LVLICSRVPORT=7569		# 7569 is the default
export LVSRESRV=${HOSTIPADDR}
