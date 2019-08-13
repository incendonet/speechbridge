#!/bin/bash
#
# Copyright 2012, All rights reserved, Incendonet Inc.
#

# Generate the SQL script that can be used on another machine to 
# sync the dialog designer tables with the ones on this machine.

LOG_FILE="/opt/speechbridge/logs/dialogsync_"$(date "+%Y%m%d")".log"
OUTPUT_FILE="dialogs_"$(date "+%Y%m%d%H%M%S")".sql" # Generated in local folder and containing current date & time in filename.

DATABASE_NAME="dspeechbridge"
DATABASE_USER="sbuser1"
PGPASSWORD=""                                       # Database password provided by user when prompted.
export PGPASSWORD                                   # Make sure that this is visible in the environment of pg_dump

SUCCESS=true                                        # Indicates if there was a problem (start out assuming there isn't).


get_database_password ()
{
    read -p "Enter the database password: " -s PGPASSWORD
    echo
}

open_log ()
{
    echo "Export commenced $(date)" >> "$LOG_FILE"
}

close_log ()
{
    echo "Export completed $(date)" >> "$LOG_FILE"
}

write_output_file_header ()
{
    echo "-- Dump created $(date)" > "$OUTPUT_FILE"
    echo >> "$OUTPUT_FILE"


    # Make sure that psql, which is used to import the file we are generating here, returns a non 0 exit code if something went wrong with the import.
    # For more info see: http://www.postgresql.org/docs/8.1/static/app-psql.html#APP-PSQL-VARIABLES

    echo "\set ON_ERROR_STOP" >> "$OUTPUT_FILE"
}

begin_transaction ()
{
    echo "BEGIN;" >> "$OUTPUT_FILE"
}

end_transaction ()
{
    echo "COMMIT;" >> "$OUTPUT_FILE"
}

clear_table ()
{
    echo "DELETE FROM $1;" >> "$OUTPUT_FILE"
}

dump_data ()
{
    # Only bother dumping the table if there have been no problems so far.  
    # Otherwise the output file will be deleted anyway so a dump now would be a waste.

    if ( "$SUCCESS" ) then
        pg_dump --data-only --column-inserts --table="$1" -U "$DATABASE_USER" "$DATABASE_NAME" >> "$OUTPUT_FILE" 2>> "$LOG_FILE"

        if [ "$?" -ne 0 ]; then 
            SUCCESS=false
            echo "Problem occurred when trying to dump data for \"$1\"" >> "$LOG_FILE"
        fi
    fi
}

report_result ()
{
    # If there was a problem then delete the output file since it is in an inconsistent state and should never be used.

    if ( ! "$SUCCESS" ) then
        m -f "$OUTPUT_FILE"
        echo "There was a problem - no export file was created." | tee -a "$LOG_FILE"
    else
        echo "Dialog Designer tables exported to file $OUTPUT_FILE.  Copy this file to the other system and run importdialog.sh." | tee -a "$LOG_FILE"
    fi
}


# Start of script

open_log
get_database_password
write_output_file_header


# Wrap everything in a transaction to make sure that if there is a problem no changes will be made to the target database.

begin_transaction


# Remove existing data from Dialog Designer tables.

clear_table "tblmenucommandsmap"
clear_table "tblmenus"
clear_table "tblcommands"

dump_data "tblmenus"
dump_data "tblcommands"
dump_data "tblmenucommandsmap"

end_transaction
report_result
close_log
