# Base OS
FROM microsoft/aspnetcore:1.1.2
MAINTAINER info@incendonet.com

# Env setup.  Note:  The usual env vars aren't expanded in the ENV section.
ENV	\
	SBBIN="/opt/speechbridge/bin"

# Copy in required files
COPY software/SBLicenseServer ${SBBIN}/

WORKDIR ${SBBIN}

CMD ["/bin/true"]
