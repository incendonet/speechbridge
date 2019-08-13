# Base OS
FROM centos:7
MAINTAINER engineering@incendonet.com

# Env setup
ENV \
	BOOST_RELEASE_UNDER="1_63_0" \
	BOOST_RELEASE_DOT="1.63.0" \
	PJSIP_RELEASE="2.5.5"

# Build deps
RUN \
	yum -y update && \
	yum -y install epel-release && \
	yum -y install \
		alsa-lib-devel \
		autoconf \
		automake \
		bzip2 \
		gcc-c++ \
		jansson-devel \
		less \
		libtool \
		libuuid-devel \
		libxml2-devel \
		ncurses-devel \
		make \
		sqlite-devel \
		tar \
		unzip \
		uuid-devel \
		wget && \
	yum clean all

WORKDIR /usr/local/src/

# Copy in dependencies
COPY software/libpjsipplusplus ./libpjsipplusplus/
COPY software/audiortr2 ./audiortr2/

RUN \
# Build boost (CentOS 7 packages dont include 'log')
	wget https://sourceforge.net/projects/boost/files/boost/${BOOST_RELEASE_DOT}/boost_${BOOST_RELEASE_UNDER}.tar.bz2 && \
	tar -xjf boost_${BOOST_RELEASE_UNDER}.tar.bz2 && \
	cd boost_${BOOST_RELEASE_UNDER} && \
	./bootstrap.sh --prefix=/usr/local --with-libraries=filesystem,locale,log,system,thread && \
	./b2 install && \
	cd .. && \

# Build PJSIP
	wget http://www.pjsip.org/release/${PJSIP_RELEASE}/pjproject-${PJSIP_RELEASE}.tar.bz2 && \
	tar -xjf pjproject-${PJSIP_RELEASE}.tar.bz2 && \
	cd pjproject-${PJSIP_RELEASE} && \
	./configure --prefix=/usr/local --disable-video --disable-opencore-amr CFLAGS='-O2 -DNDEBUG -mtune=generic' && \
	make dep && \
	make && \
	make install && \
	ldconfig && \
	#ldconfig -p | grep pj && \
	cd .. && \
	export PKG_CONFIG_PATH=/usr/lib/pkgconfig && \

# Build AudioRtr-2
	cd libpjsipplusplus && \
	make -f Makefile.x86_64 && \
	cd .. && \

	cd audiortr2 && \
	make -f Makefile.x86_64 && \
	cd ..

CMD ["/bin/true"]
