#!/bin/sh
MONOBIN=/usr/bin/mono
CONFIG_FILE=/opt/speechbridge/config/directories.sh.inc
[ -r "$CONFIG_FILE" ] && . "$CONFIG_FILE"

${MONOBIN} --config ${SBCFG}/AudioMgr.exe.config ${SBBIN}/AudioMgr.exe
