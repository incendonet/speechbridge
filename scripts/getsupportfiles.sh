#!/bin/bash
#########################################################################################
# getsupportfiles.sh
#
# Copyright 2013, Incendonet Inc.
#
# Revision info:
# - 20131107  BDA - Initial version
#########################################################################################

NUMDAYS="7"

SB="/opt/speechbridge"
SBLOGS="$SB/logs"
SBCONTENT="$SB/SBConfig/assets/content"

TODAYSTR="$(/bin/date +%Y%m%d)"
TMPDIR="$SBLOGS/sbsupportfiles_$TODAYSTR"
ARCHIVE="$SBLOGS/sbsupportfiles_$TODAYSTR.tar.bz2"
ARCHIVELINK="$SBCONTENT/sbsupportfiles.tar.bz2"

LOGFILE="$SBLOGS/supportfiles_$TODAYSTR.log"
SQLFILE="$SBLOGS/sbdailylogs_$TODAYSTR.sql"

log ()
{
	echo -e $(date "+%F %T") "$1" >> "$LOGFILE"
}


# Delete older ones from today
rm -f $ARCHIVE

# Create directory for logfiles, copy a weeks worth of logs into it
mkdir $TMPDIR

# Copy the Apache httpd error_log(s)
cp -f /var/log/httpd/error_log* $TMPDIR

# Copy all AudioRtr logs, even though it may go back further than a week.  FIX:  This could be huge on a large busy system
cp -f $SBLOGS/AudioRtr* $TMPDIR

# Copy current ProxySrv log, the loop below should catch other relevant logs if it had restarted in the timeframe
cp -f $SBLOGS/ProxySrv.log $TMPDIR

# Copy all logs with the date in the filename for the timeframe.  FIX: Would we ever want to exclude recorded RawAudio's?  (If there were too many)
#  This will also grab the nightly backups, so we don't need to explicitly pull vxml
for (( II=0; II<$NUMDAYS; II++ ))
do
	DAYSTR=`/bin/date --date="$II days ago" +%Y%m%d`
	SUPPFNAME="$SBLOGS/sbsupportfiles_$DAYSTR.tar.bz2"
	#echo "Day $II is: $SUPPFNAME"
	
	cp -f $SBLOGS/*$DAYSTR* $TMPDIR
done

# Zip up the temp dir, set permissions, and create a symlink to the name SBConfig expects
tar -cjf $ARCHIVE $TMPDIR
rm -f $TMPDIR/*
rmdir $TMPDIR
chown speechbridge:speechbridge $ARCHIVE
chmod ug=r $ARCHIVE
rm -f $ARCHIVELINK
ln -s $ARCHIVE $ARCHIVELINK

echo ""
echo "Support files for the past $NUMDAYS days can be found in >>> $ARCHIVE <<<."
echo ""
echo "Press the Enter key to continue:"
read
echo ""