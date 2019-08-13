#!/bin/bash
#
# Copyright 2012, All rights reserved, Incendonet Inc.
#

# Make sure that MASTER_IP and SLAVE_IP are set to the same value.
# This ensures that if we are running in a standalone system this script will
# quit as early as possible since there is nothing for it to do.

DEFAULT_IP="127.0.0.1"
MASTER_IP=$DEFAULT_IP                       # Actual values set in CONFIG_FILE
SLAVE_IP=$DEFAULT_IP                        # Actual values set in CONFIG_FILE
VIRTUAL_IP=$DEFAULT_IP                      # Actual values set in CONFIG_FILE

OTHER_MACHINE_IP=""                         # Set from command line options (if present)
MY_IP=""                                    # Set in init()

LOCK_DIR="/var/log"
LOCK_FILE=$LOCK_DIR/$(basename $0).lock

LOG_DIR="/opt/speechbridge/logs"
LOG_FILE=$LOG_DIR/$(basename $0)_$(date "+%Y%m%d").log

SBDBUTILS="/opt/novell/mono/bin/mono /opt/speechbridge/bin/sbdbutils.exe"
CONFIG_FILE="/opt/speechbridge/config/cluster.config"

DEVICE=""                                   # Set in init()
INTERFACE=""                                # Set in init()

NOTIFICATION_FILE="/opt/speechbridge/config/cluster.notification"
EMAIL_RECIPIENTS=""                         # Actual values set in NOTIFICATION_FILE

JOIN_CLUSTER=false                          # Overridden from command line options (if present)
SHOW_CONFIGURATION=false                    # Overridden from command line options (if present)
SHOW_USAGE=false                            # Overridden from command line options (if present)

PING_COUNT=5                                # The number of requests that ping should send when testing an address.  The higher the count, the longer it will take to return, but it will also be more forgiving of dropped packets.


# Exit constants

EXIT_SUCCESS=0
EXIT_FAILURE_UNKNOWN_STATE=1
EXIT_FAILURE_ALREADY_RUNNING=2


# Source function library.
. /etc/rc.d/init.d/functions                # Needed for killproc.  NOTE: This resets PATH.


# Read configuration files.

[ -r "$CONFIG_FILE" ] && . "$CONFIG_FILE"
[ -r "$NOTIFICATION_FILE" ] && . "$NOTIFICATION_FILE"


log ()
{
    echo -e $(date "+%F %T") "$1" >> "$LOG_FILE"

    if ( $SHOW_CONFIGURATION ) then         # If we are running with the "show configuration" (-c) command line option then make sure all errors get echoed to the console.
        echo -e "$1"
    fi
}

init () 
{
    local SLAVE=-1
    local DEVS=$(ip -o link | cut -f 2 --delimiter=' ')

    for i in $DEVS; do
        local DEV=${i%:}
        local DEV_IP=$(ifconfig $DEV) && DEV_IP=${DEV_IP#*inet addr:} && DEV_IP=${DEV_IP%% *}

        if [ "$DEV_IP" = "$MASTER_IP" ]; then
            SLAVE=0
            break
        fi

        if [ "$DEV_IP" = "$SLAVE_IP" ]; then
            SLAVE=1
            break
        fi
    done

    if [ "$SLAVE" -eq -1 ]; then
        send_script_failure_notification "ERROR: Unable to determine if I'm MASTER or SLAVE - failover script ABORTED."
        exit $EXIT_FAILURE_UNKNOWN_STATE
    fi

    DEVICE="$DEV"
    INTERFACE="$DEVICE:1"
    MY_IP="$DEV_IP"
}

is_master ()
{
    if [ "$MY_IP" = "$MASTER_IP" ]; then
        return $(true)
    fi

    return $(false)
}

is_slave ()
{
    return $(! is_master)
}

is_standalone_mode ()
{
	return $( [ "$MASTER_IP" = "$SLAVE_IP" ] )
}

is_clustering_enabled ()
{
    return $( [ "$MASTER_IP" != "$DEFAULT_IP" ] )
}

hold_off_to_give_master_enough_time_to_start ()
{
    local UP_SECONDS=$(cat /proc/uptime | cut -f 1 --delimiter='.')
    local UP_MINUTES=$((UP_SECONDS/60))

    if [ "$UP_MINUTES" -lt 5 ]; then
        return $(true)
    else
        return $(false)
    fi
}

release_virtual_ip () 
{
    ifconfig "$INTERFACE" down
    log "VirtualIP released."
}

acquire_virtual_ip () 
{
    ifconfig "$INTERFACE" "$VIRTUAL_IP"/24 up
    arping -U -c 5 -I "$DEVICE" "$VIRTUAL_IP" > /dev/null
    log "VirtualIP acquired."
}

is_network_reachable ()
{
    local ROUTER_IP=$(route -n | grep UG | cut -c 17-31)
    return $(is_ip_address_reachable "$ROUTER_IP")
}

is_slave_reachable ()
{
    return $(is_ip_address_reachable "$SLAVE_IP")
}

is_virtual_ip_active ()
{
    return $(is_ip_address_reachable "$VIRTUAL_IP")
}

is_ip_address_reachable ()
{
    return $(ping -c $PING_COUNT "$1" > /dev/null)
}

obtain_lock ()
{
    if [ -e "$LOCK_FILE" ]; then
        return $(false)
    fi


    # Create lockfile.

    echo "" > "$LOCK_FILE"
    return $(true)
}

release_lock ()
{
    rm "$LOCK_FILE"
}

send_script_failure_notification ()
{
    log "$1"
    send_notification "ALERT: Failover script error." "$1"
}

send_failover_notification ()
{
    local MESSAGE="There is a problem with $1.  Full handling of all calls has been taken over by $2."
    log "$MESSAGE"
    send_notification "ALERT: Failover occurred." "$MESSAGE"
}

send_notification ()
{
    if [ "$EMAIL_RECIPIENTS" ]; then
        echo "$2" | mail -s "$1" "$EMAIL_RECIPIENTS"
    fi
}

switch_to_standalone_mode ()
{
    reconfigure_SB "$1" "$1" "$2"
}

switch_to_cluster_mode ()
{
    reconfigure_SB "$1" "$2" "$3"
}

reconfigure_SB ()
{
    $SBDBUTILS --cluster "$3" "$1" "$2" >> "$LOG_FILE"
    restart_SB_components
}

restart_SB_components ()
{
    # Restart Proxy Server and Audio Routers
    # This relies on the fact that once these processes are killed SBLocalRM will restart them.

    killproc AudioRtr
    killproc ProxySrv
}

show_usage ()
{
    cat << EOF

Usage: $(basename $0) [-c | -m MasterIP -s SlaveIP]

       -c
            This will display the current configuration of the system.

       -m MasterIP -s SlaveIP 
            This will suspend failover testing on this machine and configure it to join
            a cluster using the specified Master and Slave IP.

            NOTE:
                 1) Both parameters have to be specified.
                 2) Either the 'MasterIP' or 'SlaveIP' parameter has to match this machines IP address.
                 3) It is assumed that the other machine in the cluster is configured correctly.

EOF
}

process_options ()
{
    local OPTION=
    local NO_OPTIONS=true
    local GOT_MASTER_IP=false
    local GOT_SLAVE_IP=false

    while getopts ":cm:s:" OPTION
    do
        case "$OPTION" in       
            c)
                SHOW_CONFIGURATION=true
                ;;
            m)
                MASTER_IP=$OPTARG
                GOT_MASTER_IP=true
                NO_OPTIONS=false
                ;;
            s)
                SLAVE_IP=$OPTARG
                GOT_SLAVE_IP=true
                NO_OPTIONS=false
                ;;
            *) 
                SHOW_USAGE=true
                ;;
        esac
    done

    if ( ! "$NO_OPTIONS" ) then
        if ( "$GOT_MASTER_IP" && "$GOT_SLAVE_IP" ) then
            JOIN_CLUSTER=true
        else
            SHOW_USAGE=true
        fi
    fi
}

show_configuration ()
{
    echo

    if ( is_standalone_mode ) then
        echo "Running in standalone mode."
        echo
        echo "My IP     : $MY_IP"
    else
        if ( is_master ) then
            echo "Running as MASTER."
        else
            echo "Running as SLAVE."
        fi

        echo
        echo "Master IP : $MASTER_IP"
        echo "Slave IP  : $SLAVE_IP"
    fi

    if ( is_virtual_ip_active ) then
        echo "Virtual IP: $VIRTUAL_IP  Status: Up"
    else
        echo "Virtual IP: $VIRTUAL_IP  Status: Down"
    fi

    echo

    if [ -e "$LOCK_FILE" ]; then
        echo "Lockfile: $LOCK_FILE  Status: Locked"
    else
        echo "Lockfile: $LOCK_FILE  Status: Unlocked"
    fi

    echo

    if [ "$EMAIL_RECIPIENTS" ]; then
        echo "Notification e-mails sent to: $EMAIL_RECIPIENTS"
    else
        echo "No e-mail addresses are configure to receive notifications."
    fi

    echo
}



# Start of script

process_options "$@"

if ( "$SHOW_USAGE" ) then
    show_usage
    exit $EXIT_SUCCESS
fi

if ( ! is_clustering_enabled ) then
   echo "SpeechBridge clustering not enabled."
   exit $EXIT_SUCCESS
fi

if ( "$JOIN_CLUSTER" ) then
    until ( obtain_lock )
    do
        echo "Waiting for failover test scheduled by cron to complete.  If this takes longer than 30 seconds you might have to clear the lock file $LOCK_FILE yourself."
        sleep 10
    done

    log "Failover monitoring suspended"
    switch_to_cluster_mode "$MASTER_IP" "$SLAVE_IP" "$VIRTUAL_IP"

    echo -n "Failover monitoring suspended.  Press ENTER to resume."
    read

    echo "Failover monitoring resumed."
    log "Failover monitoring resumed"
    release_lock

    exit $EXIT_SUCCESS
fi


init

if ( "$SHOW_CONFIGURATION" ) then
    show_configuration
    exit $EXIT_SUCCESS
fi


# If I'm in standalone configuration then quit since there is no point in running this script any further.

if ( is_standalone_mode ) then

    # Is a Virtual IP supposed to be used?  If so make sure it is up and running before we quit.
    # Since the Virtual IP is configured dynamically it means that if the machine is rebooted it is lost.
    # Thus it is necessary to check for this in the standalone mode to make sure that is is recreated if required.

    if [ "$MASTER_IP" != "$VIRTUAL_IP" ]; then
        if ( ! is_virtual_ip_active ) then
          log "Running in standalone mode."
          acquire_virtual_ip
       fi
    fi

    exit $EXIT_SUCCESS
fi


# Am I the slave?

if ( is_slave ) then
    if ( hold_off_to_give_master_enough_time_to_start ) then
        exit $EXIT_SUCCESS
    fi
fi


# Am I already running?

if ( ! obtain_lock ) then
    log "Exit since an instance of me is already running."
    exit $EXIT_FAILURE_ALREADY_RUNNING
fi


# Can I see the network?

if ( is_network_reachable ) then
    log "Connected to Network"

    # Can I see the VirtualIP?

    if ( ! is_virtual_ip_active ) then
        acquire_virtual_ip

        if ( is_master ) then

            # If I can seee the slave then run in a cluster configuration, otherwise handle all the calls myself.

            if ( is_slave_reachable ) then
                switch_to_cluster_mode "$MASTER_IP" "$SLAVE_IP" "$VIRTUAL_IP"
            else
                switch_to_standalone_mode "$MASTER_IP" "$VIRTUAL_IP"
                send_failover_notification "$SLAVE_IP" "$MASTER_IP"
            fi
        else
            switch_to_standalone_mode "$SLAVE_IP" "$VIRTUAL_IP"
            send_failover_notification "$MASTER_IP" "$SLAVE_IP"
        fi
    else
        log "Virtual IP is active"

        # Can I still see the slave, or do I have to handle all the calls myself?

        if ( is_master ) then
            if ( ! is_slave_reachable ) then
                switch_to_standalone_mode "$MASTER_IP" "$VIRTUAL_IP"
                send_failover_notification "$SLAVE_IP" "$MASTER_IP"
            fi
        fi
    fi
    
else
    log "Disconnected from Network"
    release_virtual_ip
fi

release_lock



