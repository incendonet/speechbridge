# Base OS
FROM centos:7
MAINTAINER info@incendonet.com

# Env setup
ENV \
	PROPER_NAME="SBLauncher"

ARG POCO_VER
ARG POCO_PATCH

# Build deps
RUN \
	yum -y update && \
	yum -y install epel-release &&\
	yum -y install \
		gcc-c++ \
		git \
		make \
		wget && \
	yum clean all

WORKDIR /usr/local/src/

# Copy in dependencies
COPY software/* ./

RUN \

# Get and build POCO Foundation
	wget https://pocoproject.org/releases/poco-${POCO_VER}/poco-${POCO_VER}${POCO_PATCH}.tar.gz && \
	tar -xzf poco-${POCO_VER}${POCO_PATCH}.tar.gz && \
	mkdir -p poco-${POCO_VER}${POCO_PATCH}/Foundation/samples/SBLauncher && \
	mv Make* poco-${POCO_VER}${POCO_PATCH}/Foundation/samples/SBLauncher/ && \
	mv src poco-${POCO_VER}${POCO_PATCH}/Foundation/samples/SBLauncher/ && \
	cd poco-${POCO_VER}${POCO_PATCH} && \
	./configure --no-tests --no-samples --minimal --shared --omit=CppUnit,CppUnit/WinTestRunner,XML,JSON,Util,Net && \
	make && \
	cd Foundation/samples/SBLauncher && \
	make

CMD ["/bin/true"]
