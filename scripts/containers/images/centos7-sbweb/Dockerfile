# Base OS
FROM incendonet/centos7-mono-apache
MAINTAINER info@incendonet.com

# Env setup.  Note:  The usual env vars aren't expanded in the ENV section.
ENV	\
	HOME="/root" \
	LOCALETC="/usr/local/etc" \
	SBBASE="/opt/speechbridge" \
	SBBIN="/opt/speechbridge/bin" \
	SBCFG="/opt/speechbridge/config" \
	SBTEMPL="/opt/speechbridge/templates" \
	SBSBCONFIG="/opt/speechbridge/SBConfig" \
	SBSW="/home/speechbridge/software" \
	ARTIFACTS="artifacts"

# Get updates and build deps
RUN \
	yum -y update && \
	yum -y install less unzip && \
	yum clean all

# Copy in required files
COPY software ${SBBASE}/${ARTIFACTS}

# Put files where we want them
WORKDIR ${SBBASE}
RUN \
	mkdir -p ${SBBIN} && \
	mkdir -p ${SBTEMPL} && \
	mkdir -p ${SBSBCONFIG}/bin && \

	mv ${ARTIFACTS}/dotnet/AppGenerator/bin/Release/AppGenerator.dll											${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/Incendonet.Utilities.LogClient/bin/Release/Incendonet.Utilities.LogClient.dll		${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/Incendonet.Utilities.StringHelper/bin/Release/Incendonet.Utilities.StringHelper.dll	${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/Incendonet.Plugins.UserDirectory/bin/Release/Incendonet.Plugins.UserDirectory.dll	${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/ISMessaging/bin/Release/ISMessaging.dll												${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/SBConfig/bin/Release/SBConfig.dll													${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/SBConfigStor/bin/Release/Npgsql.dll													${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/SBConfigStor/bin/Release/NSpring.dll													${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/SBConfigStor/bin/Release/RestSharp.dll												${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/SBConfigStor/bin/Release/SBConfigStor.dll											${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/SBEmail/bin/Release/SBEmail.dll														${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/SBLdapConn/bin/Release/SBLdapConn.dll												${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/SBResourceMgr/bin/Release/SBResourceMgr.dll											${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/SBTTS/bin/Release/SBTTS.dll															${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/SimpleAES/bin/Release/SimpleAES.dll													${SBBIN}/ && \
	mv ${ARTIFACTS}/dotnet/XmlDocParser/bin/Release/XmlDocParser.dll											${SBBIN}/ && \

	mv ${ARTIFACTS}/volumes/opt-speechbridge-bin/entrypoint-sbweb.sh											${SBBIN}/ && \
	mv ${ARTIFACTS}/volumes/opt-speechbridge-bin/envsub.sh														${SBBIN}/ && \

	mv ${ARTIFACTS}/volumes/opt-speechbridge-SBConfig/*															${SBSBCONFIG}/ && \
	mv ${ARTIFACTS}/volumes/opt-speechbridge-templates/sbdbutils.exe.config.TEMPLATE							${SBTEMPL}/ && \
	mv ${ARTIFACTS}/web/SBConfig/*.Master																		${SBSBCONFIG} && \
	mv ${ARTIFACTS}/web/SBConfig/*.aspx																			${SBSBCONFIG} && \
	mv ${ARTIFACTS}/web/SBConfig/*.html																			${SBSBCONFIG} && \
	mv ${ARTIFACTS}/web/SBConfig/assets																			${SBSBCONFIG} && \

	ln -s ${SBBIN}/AppGenerator.dll							${SBSBCONFIG}/bin/AppGenerator.dll && \
	ln -s ${SBBIN}/Incendonet.Utilities.LogClient.dll		${SBSBCONFIG}/bin/Incendonet.Utilities.LogClient.dll && \
	ln -s ${SBBIN}/ISMessaging.dll							${SBSBCONFIG}/bin/ISMessaging.dll && \
	ln -s ${SBBIN}/Npgsql.dll								${SBSBCONFIG}/bin/Npgsql.dll && \
	ln -s ${SBBIN}/NSpring.dll								${SBSBCONFIG}/bin/NSpring.dll && \
	ln -s ${SBBIN}/RestSharp.dll							${SBSBCONFIG}/bin/RestSharp.dll && \
	ln -s ${SBBIN}/SBConfig.dll								${SBSBCONFIG}/bin/SBConfig.dll && \
	ln -s ${SBBIN}/SBConfigStor.dll							${SBSBCONFIG}/bin/SBConfigStor.dll && \
	ln -s ${SBBIN}/SBEmail.dll								${SBSBCONFIG}/bin/SBEmail.dll && \
	ln -s ${SBBIN}/SBLdapConn.dll							${SBSBCONFIG}/bin/SBLdapConn.dll && \
	ln -s ${SBBIN}/SBResourceMgr.dll						${SBSBCONFIG}/bin/SBResourceMgr.dll && \
	ln -s ${SBBIN}/SBTTS.dll								${SBSBCONFIG}/bin/SBTTS.dll && \
	ln -s ${SBBIN}/SimpleAES.dll							${SBSBCONFIG}/bin/SimpleAES.dll && \
	ln -s ${SBBIN}/XmlDocParser.dll							${SBSBCONFIG}/bin/XmlDocParser.dll && \

	rm -Rf ${ARTIFACTS} && \

	chmod +x ${SBBIN}/*.sh

CMD ["/bin/true"]
