#!/bin/sh
#####################################################################################
# sbddinstall.sh
#
# Revision info:
# - 20120831  BDA - Inclusion in SpeechBridge 4.1.1
#####################################################################################

MONO=/opt/novell/mono/bin/mono
SB=/opt/speechbridge
SBHOME=/home/speechbridge
SW=$SBHOME/software
BIN=$SB/bin
CONFIG=$SB/config
SBCONF=$SB/SBConfig
LOG=$SB/logs/sbddinstall.log

echo "Starting sbddinstall.sh at `date`"                                                                         >> $LOG
echo ""                                                                                                          >> $LOG

#####################################################################################
# Check to see if DialogDesigner is already installed
DDVER=`$MONO --config $BIN/sbdbutils.exe.config $BIN/sbdbutils.exe --get-swver DialogDesigner`       2>> $LOG
DDVERLEN=`echo $DDVER | wc -c`                                                                                 2>> $LOG
echo "DDVER is '$DDVER'"                                                                                         >> $LOG

if [ $DDVERLEN -gt 1 ]; then
    echo "DialogDesigner is already installed."                                                                  >> $LOG
    echo "DialogDesigner is already installed."
else
    #####################################################################################
    # Configure website
    echo "Configuring website..." >> $LOG

    # Add MenuManager.aspx and MenuEditor.aspx to authorization-enforced pages
    NUMLINES=`grep MenuManager $SBCONF/Web.config | wc -l`

    if [ $NUMLINES -gt 0 ]; then
        echo "Not updating Web.config"
    else
        /bin/sed 's:<location path="Help.aspx">:<location path="MenuManager.aspx">\
\t\t<system.web>\
\t\t\t<authorization>\
\t\t\t\t<deny users="?"/>\
\t\t\t</authorization>\
\t\t</system.web>\
\t</location>\
\t<location path="MenuEditor.aspx">\
\t\t<system.web>\
\t\t\t<authorization>\
\t\t\t\t<deny users="?"/>\
\t\t\t</authorization>\
\t\t</system.web>\
\t</location>\
\t<location path="Help.aspx">:' < $SBCONF/Web.config > $SBCONF/Web2.config                                              2>> $LOG

        mv -f $SBCONF/Web2.config $SBCONF/Web.config                                                                   >> $LOG 2>> $LOG
        echo "Updated Web.config"
    fi

    /sbin/service httpd reload                                                                                       >> $LOG 2>> $LOG

    #####################################################################################
    # Initialize database
    echo "Initializing database..." >> $LOG

    $MONO --config $BIN/sbdbutils.exe.config $BIN/sbdbutils.exe --run-script $CONFIG/SBUpdate_DD_pgsql.sql        >> $LOG 2>> $LOG
    $MONO --config $BIN/sbdbutils.exe.config $BIN/sbdbutils.exe --run-script $CONFIG/sblp_en-AU_pgsql.sql            >> $LOG 2>> $LOG

    #####################################################################################
    # Add "menus" user with default password
    echo "Adding user(s)..." >> $LOG

    $MONO --config $BIN/sbdbutils.exe.config $BIN/sbdbutils.exe --add-user menus menus MenuManager.aspx            >> $LOG 2>> $LOG
fi

#####################################################################################
# Done
echo "Done with sbddinstall.sh at `date`." >> $LOG
echo "Done!"
