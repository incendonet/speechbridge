#!/bin/bash
#
# Copyright 2014, All rights reserved, Incendonet Inc.
#

SBSOFT=/home/speechbridge/software
SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBCONFIG=$SBBASE/config
SBSBCONFIG=$SBBASE/SBConfig
SBVDS=$SBBASE/VoiceDocStore
SBLOGS=$SBBASE/logs
SBLIC=$SBBASE/license

echo "Fixing permissions..."

/bin/chmod ug=rx	$SBSOFT/*.sh
/bin/chmod ugo=rx   $SBBASE
/bin/chmod ugo=rx   $SBBIN
/bin/chmod ug=r     $SBBIN/*.so.*
/bin/chmod ug=rx    $SBBIN/*.cron
/bin/chmod ug=rx    $SBBIN/*.sh
/bin/chmod ug=rx    $SBBIN/speechbridged
/bin/chmod ug=rx    $SBBIN/neospeechd
/bin/chmod ug=rx    $SBBIN/SBLauncher
/bin/chmod ug=rx    $SBBIN/AudioRtr_*
/bin/chmod ug=rx    $SBBIN/CepstralCmd
/bin/chmod ug=rx    $SBBIN/NeospeechCmd
/bin/chmod ug=rx    $SBBIN/ProxySrv_*
/bin/chmod ug=r     $SBBIN/*.exe
/bin/chmod ug=r     $SBBIN/*.dll
/bin/chmod ug=r     $SBBIN/*.config
/bin/chmod ug=r     $SBBIN/*.xml
/bin/chmod ugo=rwx  $SBCONFIG
/bin/chmod ugo=rw   $SBCONFIG/*.cfg
/bin/chmod ug=rwx   $SBCONFIG/*.sh
/bin/chmod ug=r		$SBBASE/config/*.sql
/bin/chmod ugo=rw   $SBCONFIG/ProxySrv.config
/bin/chmod ugo=rw   $SBCONFIG/cluster.notification
/bin/chmod -R ugo=r	$SBLIC
/bin/chmod ugo+rx	$SBLIC
/bin/chmod ug+w		$SBLIC
/bin/chmod -R ugo=r $SBSBCONFIG
/bin/chmod ugo=rx   $SBSBCONFIG
/bin/chmod ugo=rx   $SBSBCONFIG/assets
/bin/chmod ugo=rx   $SBSBCONFIG/assets/content
/bin/chmod ugo=rx   $SBSBCONFIG/assets/docs
/bin/chmod ugo=rx   $SBSBCONFIG/assets/images
/bin/chmod ugo=rx   $SBSBCONFIG/assets/js
/bin/chmod ugo=rx   $SBSBCONFIG/bin
/bin/chmod ug=rx    $SBLOGS
/bin/chmod ugo=rx   $SBVDS
/bin/chmod ugo+w    $SBVDS
/bin/chmod ugo=rw   $SBVDS/*.vxml.xml
/bin/chmod -R ugo=r $SBVDS/*_Template.vxml.xml
/bin/chmod ugo=r    $SBVDS/*.gram
/bin/chmod ugo=rwx  $SBVDS/Prompts
/bin/chmod -R ug=rwx   $SBVDS
/bin/chmod -R ugo=rw   $SBVDS/Prompts/*.wav
/bin/chmod -R ugo=rwx  $SBVDS/Prompts/Names
/bin/chmod ugo=rx   $SBBASE/SBReports
/bin/chmod ug+w     $SBBASE/SBReports

/bin/chown -R speechbridge:speechbridge $SBBASE
/bin/chown root:root $SBBIN/sbfailover*

echo "Done fixing permissions."
echo ""