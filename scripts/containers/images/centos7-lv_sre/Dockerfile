# Base OS
FROM centos:7
MAINTAINER info@incendonet.com

# Env setup
ENV \
	ENTRYPOINT="entrypoint-lvsre.sh" \
	ENTRYPOINT_DIR="/usr/local/bin/" \
	HOME="/root" \
	LOCALBIN="/usr/local/bin" \
	LOCALETC="/usr/local/etc" \
	SBSW="/home/speechbridge/software"

ARG VERSION

# Get updates and build deps
RUN \
	yum -y update &&\
	yum -y install less unzip &&\
	yum clean all

# Copy in required files
#Lumenvox
COPY software/LumenVoxCore-${VERSION}.el7.x86_64.rpm $SBSW/
COPY software/LumenVoxClient-${VERSION}.el7.x86_64.rpm $SBSW/
COPY software/LumenVoxSRE-${VERSION}.el7.x86_64.rpm $SBSW/
COPY software/client_property.conf.TEMPLATE $SBSW/
COPY software/envsub.sh $SBSW/
COPY software/${ENTRYPOINT} $SBSW/

# Install dependencies
WORKDIR ${SBSW}
RUN \
	# LumenVox
	yum -y --nogpgcheck localinstall LumenVoxCore-${VERSION}.el7.x86_64.rpm LumenVoxClient-${VERSION}.el7.x86_64.rpm LumenVoxSRE-${VERSION}.el7.x86_64.rpm &&\
	rm -f LumenVoxCore-${VERSION}.el7.x86_64.rpm LumenVoxClient-${VERSION}.el7.x86_64.rpm LumenVoxSRE-${VERSION}.el7.x86_64.rpm &&\

	# Get entrypoint script and config template
	mv envsub.sh ${LOCALBIN}/ &&\
	chmod +x ${LOCALBIN}/*.sh &&\
	mv ${ENTRYPOINT} ${ENTRYPOINT_DIR}/ &&\
	chmod +x ${ENTRYPOINT_DIR}/${ENTRYPOINT} &&\
	mv client_property.conf.TEMPLATE ${LOCALETC} &&\
	cd ~ &&\
	rm -Rf /home/speechbridge

WORKDIR /

EXPOSE 5730

CMD ["${ENTRYPOINT_DIR}/${ENTRYPOINT}"]
