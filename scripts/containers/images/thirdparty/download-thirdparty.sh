#!/bin/bash

set +e

yum -y install wget

# Cepstral
CEP_VER="6.0.1"
wget http://www.cepstral.com/downloads/installers/linux64/Cepstral_Callie-8kHz_x86-64-linux_${CEP_VER}.tar.gz

# LumenVox
LV_VER="15.1.200-1"
wget http://www.lumenvox.com/packages/EL7/x86_64/LumenVoxClient-${LV_VER}.el7.x86_64.rpm
wget http://www.lumenvox.com/packages/EL7/x86_64/LumenVoxCore-${LV_VER}.el7.x86_64.rpm
wget http://www.lumenvox.com/packages/EL7/x86_64/LumenVoxSRE-${LV_VER}.el7.x86_64.rpm
wget http://www.lumenvox.com/packages/EL7/x86_64/LumenVoxLicenseServer-${LV_VER}.el7.x86_64.rpm
