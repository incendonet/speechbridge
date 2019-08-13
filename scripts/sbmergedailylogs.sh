#!/bin/sh
#########################################################################################
# sbmergedailylogs.sh
#
# Copyright 2014, Incendonet Inc.
#
# Revision info:
# - 20131107  BDA - Initial version
# - 20140120  BDA - Added stderr redirection
#########################################################################################

SB=/opt/speechbridge
LOGS=$SB/logs
YESTFNAME=$1
DATEDASH=`date --date="$YESTFNAME" +%Y-%m-%d`
LOGFILE=$LOGS/sbmergedailylogs_$YESTFNAME.log
DBFILE=$LOGS/sbdailylogs_$YESTFNAME.sqlite
SQLFILE=$LOGS/sbdailylogs_$YESTFNAME.sql


if [ -z "$YESTFNAME" ]; then
	echo "Please specify the date string as an argument in the form:  YYYYMMDD."
else

	# Create combined logfiles for AudioMgr, DialogMgr, and SBLocalRM, skipping lines that don't begin with the date (and are most likely not in the format we can import)
	cat $LOGS/AudioMgr_$YESTFNAME*.log.txt | grep "^$DATEDASH" > $LOGS/AudioMgr_$YESTFNAME-ALL.log.txt		2>> "$LOGFILE"
	cat $LOGS/DialogMgr_$YESTFNAME*.log.txt | grep "^$DATEDASH" > $LOGS/DialogMgr_$YESTFNAME-ALL.log.txt	2>> "$LOGFILE"
	cat $LOGS/SBLocalRM_$YESTFNAME*.log.txt | grep "^$DATEDASH" > $LOGS/SBLocalRM_$YESTFNAME-ALL.log.txt	2>> "$LOGFILE"

	# Import logs into sqlite
	echo "DROP TABLE tblSBLogs;"										>  $SQLFILE
	echo "CREATE TABLE tblSBLogs "										>> $SQLFILE
	echo "( "															>> $SQLFILE
#	 echo "	   iKey                INTEGER PRIMARY KEY ASC, "			>> $SQLFILE
	echo "	  dtTime              TEXT, "								>> $SQLFILE
	echo "	  sSeverity           TEXT, "								>> $SQLFILE
	echo "	  sIpaddr             TEXT, "								>> $SQLFILE
	echo "	  sSessionId          TEXT, "								>> $SQLFILE
	echo "	  sAssemblyName       TEXT, "								>> $SQLFILE
	echo "	  sCallingObject      TEXT, "								>> $SQLFILE
	echo "	  sPortIndex          TEXT, "								>> $SQLFILE
	echo "	  sVmcIndex           TEXT, "								>> $SQLFILE
	echo "	  sMessage            TEXT "								>> $SQLFILE
	echo ");"															>> $SQLFILE
	echo ""																>> $SQLFILE
	echo ".mode tabs"													>> $SQLFILE
	echo ""																>> $SQLFILE
	echo ".import $LOGS/AudioMgr_$YESTFNAME-ALL.log.txt tblSBLogs"		 	>> $SQLFILE
	echo ".import $LOGS/DialogMgr_$YESTFNAME-ALL.log.txt tblSBLogs"		 	>> $SQLFILE
	echo ".import $LOGS/SBLocalRM_$YESTFNAME-ALL.log.txt tblSBLogs"		 	>> $SQLFILE
	echo ""																>> $SQLFILE

	sqlite3 $DBFILE < $SQLFILE															1>> "$LOGFILE"  2>&1
	
	# Cleanup:  leave behind the -ALL and sqlite files
	rm -f SQLFILE																		2>> "$LOGFILE"
	rm -f $LOGS/AudioMgr_$YESTFNAME.log.txt												2>> "$LOGFILE"
	rm -f $LOGS/AudioMgr_$YESTFNAME\_*.log.txt											2>> "$LOGFILE"
	rm -f $LOGS/DialogMgr_$YESTFNAME.log.txt											2>> "$LOGFILE"
	rm -f $LOGS/DialogMgr_$YESTFNAME\_*.log.txt											2>> "$LOGFILE"
	rm -f $LOGS/SBLocalRM_$YESTFNAME.log.txt											2>> "$LOGFILE"
	rm -f $LOGS/SBLocalRM_$YESTFNAME\_*.log.txt											2>> "$LOGFILE"
	
fi