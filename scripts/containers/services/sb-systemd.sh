#!/bin/bash

ULBIN=/usr/local/bin
BASE=/usr/local/opt/incendonet
STARTUP=${BASE}/startup
SERVICES=${BASE}/services

DOCKERFILE_LIST="-f ${SERVICES}/sbcore.docker-compose.yml -f ${SERVICES}/sbcore-nethost-prod.docker-compose.yml -f ${SERVICES}/sbsip-proxysrv.docker-compose.yml"

MODE=$1

source ${STARTUP}/speechbridge.env.appliance.sh
source ${STARTUP}/speechbridge.env.secrets.sh

function set_speechbridge_firewall ()
{
    iptables --append SPEECHBRIDGE --protocol tcp --match state --state NEW --destination-port 80 --jump ACCEPT                                          # HTTP
    iptables --append SPEECHBRIDGE --protocol tcp --match state --state NEW --destination-port 443 --jump ACCEPT                                         # HTTPS
    iptables --append SPEECHBRIDGE --protocol tcp --match state --state NEW --destination-port 3389 --jump ACCEPT                                        # RDP
    iptables --append SPEECHBRIDGE --protocol udp --match state --state NEW --destination-port 5060:5125 --jump ACCEPT                                   # SIP
    iptables --append SPEECHBRIDGE --protocol udp --match state --state NEW --destination-port 10000:10127 --jump ACCEPT                                 # RTP
    
    
    # The following ports should only be accessible by other containers running on this system.
    # This is done by limiting access to originate only from the network interface used by the SpeechBridge containers.
    
    # IMPORTANT: This assumes that the network name specified in the docker-compose file is "sbcore".
    
    SPEECHBRIDGE_NETWORK_INTERFACE=$(docker network inspect --format='br-{{ printf "%.12s" .Id }}' services_sbcore)
    
    iptables --append SPEECHBRIDGE -i "$SPEECHBRIDGE_NETWORK_INTERFACE" --protocol tcp --match state --state NEW --destination-port 7569 --jump ACCEPT   # Lumenvox License Server - should only be accessible by other containers running on this system.
    iptables --append SPEECHBRIDGE -i "$SPEECHBRIDGE_NETWORK_INTERFACE" --protocol tcp --match state --state NEW --destination-port 60000 --jump ACCEPT  # SpeechBridge License Server - should only be accessible by other containers running on this system.
   
}

function clear_speechbridge_firewall ()
{
    iptables --flush SPEECHBRIDGE
}

function start_speechbridge ()
{
    ${ULBIN}/docker-compose ${DOCKERFILE_LIST} up -d
    set_speechbridge_firewall
}

function stop_speechbridge ()
{
    clear_speechbridge_firewall
    ${ULBIN}/docker-compose ${DOCKERFILE_LIST} down
}

if [ "${MODE}" == "start" ]; then
    start_speechbridge
elif [ "${MODE}" == "stop" ]; then
    stop_speechbridge
elif [ "${MODE}" == "restart" ]; then
    stop_speechbridge
    start_speechbridge
else
    echo "sb-systemd.sh got unknown argument: ${MODE}"
    exit 1
fi

