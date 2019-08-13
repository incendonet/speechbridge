#!/bin/sh

NEWMONODIR=/opt/novell/mono/bin
BIN=/opt/speechbridge/bin

sCfgEmail=`grep EmailServer /opt/speechbridge/bin/DialogMgr.exe.config | cut -d\" -f4`

if [ "$sCfgEmail" = "Exchange2003" ]; then
    sDbEmail="Microsoft Exchange 2003"
elif [ "$sCfgEmail" = "Exchange2007" ]; then
    sDbEmail  "Microsoft Exchange 2007 - Normal"
else
    sDbEmail  "[none]"
fi

/bin/echo "UPDATE tblConfigParams SET sValue = '$sDbEmail' WHERE sComponent='Collaboration:0000';" > /home/speechbridge/collab0.sql
#$NEWMONODIR/mono --config $BIN/sbdbutils.exe.config $BIN/sbdbutils.exe --run-script /home/speechbridge/collab0.sql
