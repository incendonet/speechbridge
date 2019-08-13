#!/bin/bash
#
# SpeechBridge report re-runner
# This version of the script takes YYYYMMDD as the first and only argument.
# Copyright 2016, All rights reserved, Incendonet Inc.
#

BASEPATH=/opt/speechbridge
BINPATH=$BASEPATH/bin
LOGPATH=$BASEPATH/logs
REPORTPATH=$BASEPATH/SBReports
CONFIGPATH=$BASEPATH/config
REPORTSCRIPT=$BINPATH/sbreportgen.cron
LOGBAK=$LOGPATH/BAK


function RunReportForDay {
	echo ""
	echo "Extracting logs for $2..."

	# Backup log archive so we can restore it later
	cd $LOGPATH
	cp -f sblogs_$1.tar.bz2 $LOGBAK

	# Extract the day's log files and move them into the directory
	# where the report generator expects them.
	cd $LOGPATH
	tar -xjf sblogs_$1.tar.bz2
	mv opt/speechbridge/logs/*$1* .

	# Fix filenames
	mv AudioMgr_$1-ALL.log.txt AudioMgr_$1.log.txt
	mv DialogMgr_$1-ALL.log.txt DialogMgr_$1.log.txt
	mv SBLocalRM_$1-ALL.log.txt SBLocalRM_$1.log.txt

	# Run the report
	echo "Running the report for $2..."
	source $REPORTSCRIPT $1 $2

	# Restore the original log archives
	cd $LOGPATH
	rm -f sblogs_$1.tar.bz2
	mv -f $LOGBAK/sblogs_$1.tar.bz2 .
	rmdir $LOGBAK
}

# Check args
if [ "$#" -ne 1 ]; then
    echo "  Please specify a date in the format:  YYYYMMDD"
	exit 1
fi

# Create backup directory to keep a copy of the original log archives
mkdir -p $LOGBAK

	DATEDASH="$(/bin/date --date=$1 +%Y-%m-%d)"
	DATENODASH="$(/bin/date --date=$1 +%Y%m%d)"

	RunReportForDay $DATENODASH $DATEDASH

echo "Done."
echo ""
