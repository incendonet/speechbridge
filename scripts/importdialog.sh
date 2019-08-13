#!/bin/bash
#
# Copyright 2012, All rights reserved, Incendonet Inc.
#

# Process the SQL script to sync Dialog Designer tables.

LOG_FILE="/opt/speechbridge/logs/dialogsync_"$(date "+%Y%m%d")".log"
INPUT_FILE=""                                       # Set on command line

DATABASE_NAME="dspeechbridge"
DATABASE_USER="sbuser1"
PGPASSWORD=""                                       # Database password provided by user when prompted.
export PGPASSWORD                                   # Make sure that this is visible in the environment of psql


get_database_password ()
{
    read -p "Enter the database password: " -s PGPASSWORD
    echo
}

open_log ()
{
    echo "Import commenced $(date)" >> "$LOG_FILE"
}

close_log ()
{
    echo "Import completed $(date)" >> "$LOG_FILE"
}

import_from_file ()
{
    echo "SQL script file: $1" >> "$LOG_FILE"
    psql "$DATABASE_NAME" "$DATABASE_USER" -f "$1" >> /dev/null 2>> "$LOG_FILE"
}

report_result ()
{
    if [ "$?" -ne 0 ]; then 
        echo -e "There was a problem during the import.  The database has not been changed.\nSee the log file $LOG_FILE for more information." | tee -a "$LOG_FILE"
    else
        echo "Dialog Designer tables successfully imported." | tee -a "$LOG_FILE"
    fi
}

usage ()
{
    echo "Usage: $(basename $0) SQLScriptFile"
}


# Start of script

open_log


# Verify that a SQL script has been specified as an argument (actually only checking that the file specified is readable).

if [ "$#" -eq 1 ]; then
    if [ -r "$1" ]; then
        INPUT_FILE="$1"
    else
        echo "Import ABORTED - File specified either does not exist or is not readable. ($1)" | tee -a "$LOG_FILE"
    fi
else
    echo "No SQL script file specified." >> "$LOG_FILE"
fi

if [ "" != "$INPUT_FILE" ]; then
    get_database_password
    import_from_file "$INPUT_FILE"
    report_result
else
    usage
fi

close_log
