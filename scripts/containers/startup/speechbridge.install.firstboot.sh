#!/bin/bash
set -e
trap 'echo "Hmmm, something went wrong.  Please copy the text above and /var/log/sbinstlog-YYYYMMDD.txt and email it to support@incendonet.com"; echo ""; exit' \
    SIGINT SIGQUIT SIGTERM EXIT

###################################################################################################
# Initializes the docker components of the system, specifically:  directories, DB
#
# Version 0.0.1.1
#
# Environment dependencies:
#   POSTGRES_USER, POSTGRES_PASSWORD, POSTGRES_SBUSER, POSTGRES_SBUSER, POSTGRES_SBPASSWORD
###################################################################################################

export TODAYSTR="$(/bin/date --date=today +%Y%m%d)"
export INSTALLOG=/var/log/sbinstlog-${TODAYSTR}.txt
export STARTDIR=${PWD}

PullIncendonetImages()
{
    docker pull quay.io/incendonet/centos6-sbsip_proxysrv       2>> ${INSTALLOG}
    docker pull quay.io/incendonet/centos7-sbsip_audiortr       2>> ${INSTALLOG}
    docker pull quay.io/incendonet/centos7-lv_license           2>> ${INSTALLOG}
    docker pull quay.io/incendonet/centos7-lv_sre               2>> ${INSTALLOG}
    docker pull quay.io/incendonet/centos7-sbbin_lv_cep         2>> ${INSTALLOG}
    docker pull quay.io/incendonet/centos7-sbweb                2>> ${INSTALLOG}
    docker pull quay.io/incendonet/sblicenseserver              2>> ${INSTALLOG}
}

ConfigureFirewall()
{
    # Remove any existing rules from all chains

    iptables --flush
    iptables -t nat --flush
    iptables -t mangle --flush

    # Remove any user-defined chains

    iptables --delete-chain
    iptables -t nat --delete-chain
    iptables -t mangle --delete-chain

    # Set default policies

    iptables --policy INPUT ACCEPT
    iptables --policy OUTPUT ACCEPT
    iptables --policy FORWARD ACCEPT
    iptables -t nat --policy PREROUTING ACCEPT
    iptables -t nat --policy INPUT ACCEPT
    iptables -t nat --policy OUTPUT ACCEPT
    iptables -t nat --policy POSTROUTING ACCEPT
    iptables -t mangle --policy PREROUTING ACCEPT
    iptables -t mangle --policy INPUT ACCEPT
    iptables -t mangle --policy FORWARD ACCEPT
    iptables -t mangle --policy OUTPUT ACCEPT
    iptables -t mangle --policy POSTROUTING ACCEPT


    # Create SPEECHBRIDGE chain

    iptables --new-chain SPEECHBRIDGE


    # Set up basic rules for when Speechbridge is not running.

    iptables --append INPUT --match state --state RELATED,ESTABLISHED --jump ACCEPT
    iptables --append INPUT --protocol icmp --jump ACCEPT
    iptables --append INPUT --in-interface lo --jump ACCEPT
    iptables --append INPUT --protocol tcp --match state --state NEW --destination-port 22 --jump ACCEPT       # SSH
    iptables --append INPUT --jump SPEECHBRIDGE
    iptables --append INPUT --jump REJECT --reject-with icmp-host-prohibited

    iptables --append FORWARD --jump REJECT --reject-with icmp-host-prohibited


    # Save above settings so they are reused next time iptables is started.

    service iptables save
}

echo "Firstboot init starting"
echo "Firstboot init starting" >> ${INSTALLOG}

echo ""
echo "Updating OS..."
echo "Updating OS..." >> ${INSTALLOG}
yum -y update                                                                           2>> ${INSTALLOG}
yum -y install bzip2 docker iptables-utils iptables-services less unzip wget            2>> ${INSTALLOG}

echo ""
echo "Configuring OS services..."
echo "Configuring OS services..." >> ${INSTALLOG}
setenforce 0
mv /etc/selinux/config /etc/selinux/config.BAK
sed 's|SELINUX=enforcing|SELINUX=permissive|' < /etc/selinux/config.BAK > /etc/selinux/config

systemctl stop firewalld
systemctl mask firewalld
systemctl enable iptables
systemctl stop iptables                            # This ensures that when iptables is started below it will load the configuration saved to /etc/sysconfig/iptables
ConfigureFirewall
systemctl start iptables


systemctl enable docker
systemctl stop docker                                           2>> ${INSTALLOG}      # This ensures that if this script is run multiple times that docker is able to update the iptables rules as necessary (since if it is already running then the start below will not do anything).
systemctl start docker                                          2>> ${INSTALLOG}

# Host locations
export BASEDIR=/usr/local/opt
export HOSTSBHOME=${BASEDIR}/incendonet
export SERV=${HOSTSBHOME}/services
export STARTUP=${HOSTSBHOME}/startup
export HOSTVOLUMES=${HOSTSBHOME}/volumes
export HOSTLICS=${HOSTVOLUMES}/opt-speechbridge/license
export DOCKERDIR=~/.docker
export HOSTSBCFG=${HOSTVOLUMES}/opt-speechbridge/config

# Container locations
export SBHOME=/opt/speechbridge
export SBBIN="${SBHOME}/bin"
export SBCFG="${SBHOME}/config"
export SBTEMPL="${SBHOME}/templates"

export RETRYSECONDS=2

source ${STARTDIR}/speechbridge.install.secrets.sh
source ${STARTDIR}/speechbridge.install.depfuncs.sh

echo "Creating directories..."
echo "Creating directories..." >> ${INSTALLOG}
mkdir -p ${HOSTSBCFG}
mkdir -p ${HOSTLICS}
mkdir -p ${HOSTVOLUMES}/opt-speechbridge/logs
mkdir -p ${HOSTVOLUMES}/opt-speechbridge/plugins
#mkdir -p ${HOSTVOLUMES}/opt-speechbridge/VoiceDocStore/Prompts/Names
mkdir -p ${HOSTVOLUMES}/etc-asterisk
mkdir -p ${HOSTVOLUMES}/etc-nginx
mkdir -p ${HOSTVOLUMES}/var-log-asterisk
mkdir -p ${HOSTVOLUMES}/var-lib-postgresql-data-pgdata
mkdir -p ${HOSTVOLUMES}/var-log-postgresql
mkdir -p ${HOSTVOLUMES}/var-log-lvlic
mkdir -p ${HOSTVOLUMES}/var-log-lvsre
mkdir -p ${HOSTVOLUMES}/var-log-lvclient

echo ""
echo "Installing licenses..."
echo "Installing licenses..." >> ${INSTALLOG}
InstallLicenses

echo ""
echo "Pulling images..."
echo "Pulling images..." >> ${INSTALLOG}
GetDepsImages
PullIncendonetImages

echo ""
echo "Getting base file system..."
echo "Getting base file system..." >> ${INSTALLOG}
GetHostFiles
source ${STARTUP}/speechbridge.env.appliance.sh

echo ""
echo "Generating secrets..."
echo "Generating secrets..." >> ${INSTALLOG}

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -subj "/C=''/ST=''/L=''/O=''/CN=''" -keyout ${HOSTVOLUMES}/etc-nginx/nginx-selfsigned.key -out ${HOSTVOLUMES}/etc-nginx/nginx-selfsigned.crt        2>> ${INSTALLOG}

PASSPSQL=`docker run --rm -it quay.io/incendonet/centos7-sbbin_lv_cep /usr/bin/mono /opt/speechbridge/bin/passgen.exe`
PASSDBUSER=`docker run --rm -it quay.io/incendonet/centos7-sbbin_lv_cep /usr/bin/mono /opt/speechbridge/bin/passgen.exe`
ENCKEY=`docker run --rm -it quay.io/incendonet/centos7-sbbin_lv_cep /usr/bin/mono /opt/speechbridge/bin/sbdbutils.exe --gen-key`

sed "s|POSTGRES_PASSWORD=|POSTGRES_PASSWORD=${PASSPSQL}|" < ${STARTUP}/speechbridge.env.secrets.sh.BLANK > ${STARTUP}/speechbridge.env.secrets.sh.1         2>> ${INSTALLOG}
sed "s|POSTGRES_SBPASSWORD=|POSTGRES_SBPASSWORD=${PASSDBUSER}|" < ${STARTUP}/speechbridge.env.secrets.sh.1 > ${STARTUP}/speechbridge.env.secrets.sh.2       2>> ${INSTALLOG}
sed "s|DB_CKEY=|DB_CKEY=${ENCKEY}|" < ${STARTUP}/speechbridge.env.secrets.sh.2 > ${STARTUP}/speechbridge.env.secrets.sh                                     2>> ${INSTALLOG}
chmod +x ${STARTUP}/speechbridge.env.secrets.sh
rm -f ${STARTUP}/speechbridge.env.secrets.sh.?

source ${STARTUP}/speechbridge.env.secrets.sh

echo ""
echo "Installing docker-compose..."
echo "Installing docker-compose..." >> ${INSTALLOG}
curl -L https://github.com/docker/compose/releases/download/1.12.0/docker-compose-`uname -s`-`uname -m` > /usr/local/bin/docker-compose                     2>> ${INSTALLOG}
chmod +x /usr/local/bin/docker-compose

echo ""
echo "Initializing DB scripts..."
echo "Initializing DB scripts..." >> ${INSTALLOG}
# Note: sbdotnet is dependent on postgres, so it will actually be started before the `psql --help` command
docker-compose \
    -f ${SERV}/sbcore.docker-compose.yml -f ${SERV}/sbcore-nethost-prod.docker-compose.yml \
    run --rm \
    -e POSTGRES_SBPASSWORD=${POSTGRES_SBPASSWORD} \
    sbdotnet \
    bash -c \
        "sed 's|\${POSTGRES_SBPASSWORD}|${POSTGRES_SBPASSWORD}|' < ${SBTEMPL}/SBPreCreate_pgsql.sql.TEMPLATE > ${SBCFG}/SBPreCreate_pgsql.sql &&\
        cp ${SBTEMPL}/SBCreate_pgsql.sql.TEMPLATE ${SBCFG}/SBCreate_pgsql.sql &&\
        cp ${SBTEMPL}/sblp_en-US_pgsql.sql.TEMPLATE ${SBCFG}/sblp_en-US_pgsql.sql"      2>> ${INSTALLOG}

echo ""
echo "Starting postgres and creating system DB..."
echo "Starting postgres and creating system DB..." >> ${INSTALLOG}
# Start postgres and wait for it to be ready.  This will implicitly create the system DB.  Note: The command itself is irrelevant.
until docker-compose \
        -f ${SERV}/sbcore.docker-compose.yml -f ${SERV}/sbcore-nethost-prod.docker-compose.yml \
        run --rm \
        sbdotnet \
            bash -c "PGPASSWORD=${POSTGRES_SBPASSWORD} psql --help > /dev/null";    2>> ${INSTALLOG}
do
    >&2 echo "Postgres is unavailable - retry in ${RETRYSECONDS} sec"
    sleep "${RETRYSECONDS}"
done

echo ""
echo "Creating SpeechBridge DB..."
echo "Creating SpeechBridge DB..." >> ${INSTALLOG}
docker-compose \
    -f ${SERV}/sbcore.docker-compose.yml -f ${SERV}/sbcore-nethost-prod.docker-compose.yml \
    run --rm \
    -e POSTGRES_USER=${POSTGRES_USER} -e POSTGRES_PASSWORD=${POSTGRES_PASSWORD} \
    sbdotnet \
        bash -c "PGPASSWORD=${POSTGRES_PASSWORD} psql -h postgres -d postgres -U ${POSTGRES_USER} -f /opt/speechbridge/config/SBPreCreate_pgsql.sql"        2>> ${INSTALLOG}

echo ""
echo "Initializing the SpeechBridge DB..."
echo "Initializing the SpeechBridge DB..." >> ${INSTALLOG}
docker-compose \
    -f ${SERV}/sbcore.docker-compose.yml -f ${SERV}/sbcore-nethost-prod.docker-compose.yml \
    run --rm \
    sbdotnet \
        bash -c "PGPASSWORD=${POSTGRES_SBPASSWORD} psql -h postgres -d dspeechbridge -U ${POSTGRES_SBUSER} -f /opt/speechbridge/config/SBCreate_pgsql.sql"  2>> ${INSTALLOG}

echo ""
echo "Initializing the en_US language pack..."
echo "Initializing the en_US language pack..." >> ${INSTALLOG}
docker-compose \
    -f ${SERV}/sbcore.docker-compose.yml -f ${SERV}/sbcore-nethost-prod.docker-compose.yml \
    run --rm \
    sbdotnet \
        bash -c "PGPASSWORD=${POSTGRES_SBPASSWORD} psql -h postgres -d dspeechbridge -U ${POSTGRES_SBUSER} -f /opt/speechbridge/config/sblp_en-US_pgsql.sql"  2>> ${INSTALLOG}

# Create the SB admin user.
docker-compose \
    -f ${SERV}/sbcore.docker-compose.yml -f ${SERV}/sbcore-nethost-prod.docker-compose.yml \
    run --rm \
    -e SBLICENSESERVER_IP=${SBLICENSESERVER_IP} \
    -e SBLICENSESERVER_PORT=${SBLICENSESERVER_PORT} \
    -e POSTGRES_SBDB=${POSTGRES_SBDB} \
    -e POSTGRES_SBUSER=${POSTGRES_SBUSER} \
    -e POSTGRES_SBPASSWORD=${POSTGRES_SBPASSWORD} \
    -e DB_CKEY=${DB_CKEY} \
    sbdotnet \
        bash -c "
            /opt/speechbridge/bin/envsub.sh < /opt/speechbridge/templates/sbdbutils.exe.config.TEMPLATE > /opt/speechbridge/config/sbdbutils.exe.config &&
            /usr/bin/mono /opt/speechbridge/bin/sbdbutils.exe --reset-admin
            "       2>> ${INSTALLOG}
    
echo ""
echo "Initializing systemd..."
echo "Initializing systemd..." >> ${INSTALLOG}
chmod +x /usr/local/opt/incendonet/services/sb-systemd.sh
chmod 664 /usr/local/opt/incendonet/services/*.service
mv /usr/local/opt/incendonet/services/*.service /lib/systemd/system/ # Should see:  Created symlink from /etc/systemd/system/multi-user.target.wants/speechbridge.service to /etc/systemd/system/speechbridge.service.
systemctl daemon-reload                         2>> ${INSTALLOG}
systemctl enable speechbridge.service           2>> ${INSTALLOG}

echo ""
echo "Done with installation, press the <Enter> key to reboot now:"
read REBOOT
reboot
echo ""
echo "Done, rebooting." >> ${INSTALLOG}
