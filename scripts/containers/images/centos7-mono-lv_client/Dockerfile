# Copyright 2017, Incendonet Inc., All Rights Reserved
#
# This Dockerfile builds a base image that includes LumenVox client libraries

# Base OS
FROM incendonet/centos7-mono
MAINTAINER info@incendonet.com

# Env setup
ENV HOME="/root" \
	SBSW="/home/speechbridge/software"

ARG VERSION

# Get updates and build deps
RUN \
	yum -y update &&\
	yum -y install less &&\
	yum clean all

# Copy in required files
#Lumenvox
COPY software/LumenVoxCore-${VERSION}.el7.x86_64.rpm $SBSW/
COPY software/LumenVoxClient-${VERSION}.el7.x86_64.rpm $SBSW/

# Install dependencies
RUN \
	cd $SBSW/ &&\

	yum -y --nogpgcheck localinstall LumenVoxCore-${VERSION}.el7.x86_64.rpm LumenVoxClient-${VERSION}.el7.x86_64.rpm LumenVoxLicenseServer-${VERSION}.el7.x86_64.rpm &&\
	rm -f LumenVoxCore-${VERSION}.el7.x86_64.rpm LumenVoxClient-${VERSION}.el7.x86_64.rpm LumenVoxLicenseServer-${VERSION}.el7.x86_64.rpm

WORKDIR /

# Since this is a base image, there is no need for a CMD or ENTRYPOINT
CMD ["/bin/true"]
