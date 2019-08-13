# Base OS
FROM centos:7
MAINTAINER info@incendonet.com

# Env setup
ENV \
	LIB="/usr/local/lib" \
	SBBIN="/opt/speechbridge/bin"

# Build deps
RUN \
	yum -y update &&\
	yum -y install epel-release &&\
	yum -y install \
		alsa-lib \
		glibc \
		libgcc \
		libstdc++ \
		postgresql \
		tar \
		uuid && \
	yum clean all

WORKDIR /usr/local/

# Copy in required files
COPY software/libPoco* /usr/lib64/
COPY software/libboost* /usr/local/lib/
COPY software/SBLauncher ${SBBIN}/
COPY software/audiortr2 ${SBBIN}/AudioRtr
COPY software/*.sh ${SBBIN}/

RUN \
	chmod +x ${SBBIN}/*.sh

EXPOSE \
	5062-5099 \
	10000-10999

CMD ["/bin/true"]
