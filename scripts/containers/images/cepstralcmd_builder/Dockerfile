# Copyright 2017 Incendonet Inc., All Rights Reserved
# This Dockerfile builds the CepstralCmd executable, following the builder-pattern
# (i.e., build the image and copy the artifact out)

FROM centos:7
MAINTAINER engineering@incendonet.com

# Env setup.  Note:  The usual env vars aren't expanded in the ENV section.
ENV	\
	HOME="/root" \
	BUILDBASE="/usr/local/src" \
	PROJNAME="cepstralcmd" \
	SBSW="/home/speechbridge/software"

ARG VERSION

# Get updates and build deps
RUN \
	yum -y update && \
	yum -y install \
		gcc \
		make \
	yum clean all

# Copy in required files and the source
COPY software/Cepstral_Callie-8kHz_x86-64-linux_${VERSION}.tar.gz $SBSW/
COPY software/CepstralCmd /usr/local/bin/

COPY software ${BUILDBASE}/${PROJNAME}
WORKDIR ${BUILDBASE}/${PROJNAME}
	
# Install Cepstral, then build cepstralcmd
RUN \
	cd $SBSW/ &&\
	tar -xzf Cepstral_Callie-8kHz_x86-64-linux_${VERSION}.tar.gz &&\
	cd Cepstral_Callie-8kHz_x86-64-linux_${VERSION} &&\
	./install.sh agree /opt/swift &&\
	echo "/opt/swift/lib" > /etc/ld.so.conf.d/swift.conf &&\
	ldconfig &&\
	cd .. &&\
	rm -Rf Cepstral* &&\
	ln -s /usr/local/bin/CepstralCmd /usr/local/bin/TtsCmd &&\

	cd ${BUILDBASE}/${PROJNAME} &&\
	make &&\
	rm -f *.o

CMD ["/bin/true"]
