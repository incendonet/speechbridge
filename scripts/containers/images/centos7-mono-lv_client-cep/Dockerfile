# Base OS
FROM quay.io/incendonet/centos7-mono-lv_client
MAINTAINER info@incendonet.com

# Env setup
ENV HOME="/root" \
	SBSW="/home/speechbridge/software" \
	OPTSB="/opt/speechbridge"

ARG VERSION

# Get updates and build deps
RUN \
	yum -y update &&\
	yum clean all

# Copy in required files
#Cepstral
COPY software/Cepstral_Callie-8kHz_x86-64-linux_${VERSION}.tar.gz $SBSW/
COPY software/CepstralCmd /usr/local/bin/

# Install dependencies
RUN \
	cd $SBSW/ &&\

	tar -xzf Cepstral_Callie-8kHz_x86-64-linux_${VERSION}.tar.gz &&\
	cd Cepstral_Callie-8kHz_x86-64-linux_${VERSION} &&\
	./install.sh agree /opt/swift &&\
	echo "/opt/swift/lib" > /etc/ld.so.conf.d/swift.conf &&\
	ldconfig &&\
	cd .. &&\
	rm -Rf Cepstral* &&\
	ln -s /usr/local/bin/CepstralCmd /usr/local/bin/TtsCmd

WORKDIR /

# Since this is a base image, there is no need for a CMD or ENTRYPOINT
CMD ["/bin/true"]
