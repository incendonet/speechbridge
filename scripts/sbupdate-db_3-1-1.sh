#!/bin/sh

NEWMONODIR=/opt/novell/mono/bin
BIN=/opt/speechbridge/bin
CFG=/opt/speechbridge/config
INSTLOG=/opt/speechbridge/logs/sbupdate-db_3-1-1.log

SQLFILEUPDATE=$CFG/SBUpdate_3-1-1_pgsql.sql
SQLFILEDYN=$CFG/sbupdate-db_3-1-1.sql

sEDNoiseLevel=`grep EDNoiseLevel /opt/speechbridge/bin/AudioMgr.exe.config | cut -d\" -f4`

/bin/echo "UPDATE tblConfigParams SET sValue = '$sEDNoiseLevel' WHERE sComponent='Apps:global:0001';" > $SQLFILEDYN 2>> $INSTLOG
/bin/echo "UPDATE tblConfigParams SET sValue = '$sEDNoiseLevel' WHERE sComponent='Apps:global:0002';" >> $SQLFILEDYN 2>> $INSTLOG

$NEWMONODIR/mono --config $BIN/sbdbutils.exe.config $BIN/sbdbutils.exe --run-script $SQLFILEUPDATE >> $INSTLOG 2>> $INSTLOG
$NEWMONODIR/mono --config $BIN/sbdbutils.exe.config $BIN/sbdbutils.exe --run-script $SQLFILEDYN >> $INSTLOG 2>> $INSTLOG
rm -f $SQLFILEDYN >> $INSTLOG 2>> $INSTLOG
