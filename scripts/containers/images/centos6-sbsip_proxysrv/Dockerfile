# Base OS
FROM centos:6
MAINTAINER info@incendonet.com

# Env setup
ENV	HOME="/root" \
	SBBASE="/opt/speechbridge" \
	SBBIN="/opt/speechbridge/bin"

# Get updates and build deps
RUN \
	yum -y update &&\
	yum -y install \
		glibc.i686 compat-expat1.i686 libstdc++.i686 openssl-devel.i686 \
		less \
		unzip &&\
	yum clean all

# Copy in required files
COPY software/ProxySrv* ${SBBIN}/
COPY software/volumes/entrypoint-sbproxysrv.sh ${SBBIN}/
COPY software/volumes/oss-application.conf.xml ${SBBIN}/

# Put files where we want them
WORKDIR ${SBBASE}
RUN \
	chmod +x ${SBBIN}/ProxySrv* &&\
	ln -s ${SBBIN}/ProxySrv* ${SBBIN}/ProxySrv &&\
	chmod +x ${SBBIN}/*.sh

EXPOSE \
	5060

CMD ["/bin/true"]
