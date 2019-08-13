#!/bin/bash
##############################################################################################################
# Copyright (c) 2017 Incendonet Inc., All Rights Reserved
#
# Notes:
#  - Requires that docker engine be running before calling init function
#  - Env vars needed:  HOSTSBHOME, HOSTVOLUMES, AZURESASQUERYSTR
##############################################################################################################

export HOSTFILES_SOURCE=https://incendostor1.blob.core.windows.net/artifacts-forhost/SB_Zanzibar

##############################################################################################################
##############################################################################################################
GetDepsImages()
{
    docker pull incendonet/azcopy                           2>> ${INSTALLOG}
    docker pull centos:6                                    2>> ${INSTALLOG}
    docker pull centos:7                                    2>> ${INSTALLOG}
    docker pull postgres:9.2                                2>> ${INSTALLOG}
    docker pull docker.io/nginx:1.12.1-alpine               2>> ${INSTALLOG}
}

##############################################################################################################
##############################################################################################################
GetHostFiles()
{
    docker run \
        --rm -it \
        -v ${HOSTSBHOME}:/opt/speechbridge/tmp \
        incendonet/azcopy \
        azcopy \
            --recursive \
            --source ${HOSTFILES_SOURCE} \
            --destination /opt/speechbridge/tmp \
            --source-sas ${AZURESASQUERYSTR} \
            --quiet \
            2>> ${INSTALLOG}

    PROMPTS=${HOSTVOLUMES}/opt-speechbridge/VoiceDocStore/Prompts
    tar -xjf ${PROMPTS}/sbprompts_6-4-1.tar.bz2 --directory=${PROMPTS}/         2>> ${INSTALLOG}
}

##############################################################################################################
##############################################################################################################
InstallLicenses()
{
    set +e

    if [ -e ${STARTDIR/license.zip} ]; then
        TMPPWD=${PWD}
        cd ${STARTDIR}
        unzip license.zip
        cd ${TMPPWD}
    fi

    # Move license files
    mv -f ${STARTDIR}/Incendonet.lic                        ${HOSTLICS}/        2>> ${INSTALLOG}
    mv -f ${STARTDIR}/License.bts                           ${HOSTLICS}/        2>> ${INSTALLOG}
    mv -f ${STARTDIR}/verification.txt                      ${HOSTLICS}/        2>> ${INSTALLOG}
    mv -f ${STARTDIR}/opt_swift_voices_*_license.txt        ${HOSTLICS}/        2>> ${INSTALLOG}
    mv -f ${STARTDIR}/opt_swift_etc_concurrency_license.txt ${HOSTLICS}/        2>> ${INSTALLOG}

    # Move secrets used by installer
    # speechbridge.install.secrets.sh should already be in ${STARTDIR}

    # Install Quay token
    mkdir -p ${DOCKERDIR}
    mv -f ${STARTDIR}/incendonet-*-auth.json                ${DOCKERDIR}/config.json    2>> ${INSTALLOG}

    set -e
}
