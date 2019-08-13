# Copyright 2017 Incendonet Inc., All Rights Reserved

FROM quay.io/incendonet/centos7-mono-lv_client-cep
MAINTAINER info@incendonet.com

# Env setup.  Note:  The usual env vars aren't expanded in the ENV section.
ENV	\
	HOME="/root" \
	LOCALETC="/usr/local/etc" \
	SBBASE="/opt/speechbridge" \
	SBBIN="/opt/speechbridge/bin" \
	SBCFG="/opt/speechbridge/config" \
	SBTEMPL="/opt/speechbridge/templates" \
	SBSW="/home/speechbridge/software" \
	ARTIFACTS="artifacts"

# Get updates and build deps
RUN \
	yum -y update && \
	yum -y install \
		bind-utils \
		less && \
	yum clean all

# Copy in required files
COPY software ${SBBASE}/${ARTIFACTS}

# Put files where we want them
WORKDIR ${SBBASE}
RUN \
	mkdir -p ${SBBIN} && \
	mkdir -p ${SBTEMPL} && \

	mv ${ARTIFACTS}/dotnet/AppGenerator/bin/Release/AppGenerator.dll											${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/asrtest/bin/Release/asrtest.exe														${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/AsrFacadeLumenvox/bin/Release/AsrFacadeLumenvox.dll									${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/AsrFacadeLumenvox.2/bin/Release/AsrFacadeLumenvox.2.dll								${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/AsrFacadeLumenvox.2/bin/Release/AsrFacadeLumenvox.2.dll.config						${SBBIN}/AsrFacadeLumenvox.dll.config && \
	mv ${ARTIFACTS}/dotnet/AsrFacadeNull/bin/Release/AsrFacadeNull.dll											${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/AudioEngine_Console/bin/Release/AudioMgr.exe											${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/CleanNamesFolder/bin/Release/CleanNamesFolder.exe									${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/DialogEngine_Console/bin/Release/DialogMgr.exe										${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/Incendonet.Utilities.LogClient/bin/Release/Incendonet.Utilities.LogClient.dll		${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/Incendonet.Utilities.StringHelper/bin/Release/Incendonet.Utilities.StringHelper.dll	${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/Incendonet.Plugins.UserDirectory/bin/Release/Incendonet.Plugins.UserDirectory.dll	${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/ISMessaging/bin/Release/ISMessaging.dll												${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/passgen/bin/Release/passgen.exe														${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/passgen/bin/Release/MlkPwgen.dll														${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBConfig/bin/Release/SBConfig.dll													${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBConfigStor/bin/Release/Npgsql.dll													${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBConfigStor/bin/Release/NSpring.dll													${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBConfigStor/bin/Release/RestSharp.dll												${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBConfigStor/bin/Release/SBConfigStor.dll											${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/sbdbutils/bin/Release/sbdbutils.exe													${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBEmail/bin/Release/SBEmail.dll														${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBLdapConn/bin/Release/SBLdapConn.dll												${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBLocalRM/bin/Release/SBLocalRM.exe													${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBResourceMgr/bin/Release/SBResourceMgr.dll											${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBSched/bin/Release/SBSched.exe														${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SBTTS/bin/Release/SBTTS.dll															${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/sbtest/bin/Release/sbtest.exe														${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/SimpleAES/bin/Release/SimpleAES.dll													${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/PromptSelector/bin/Release/PromptSelector.dll										${SBBIN} && \
	mv ${ARTIFACTS}/dotnet/XmlDocParser/bin/Release/XmlDocParser.dll											${SBBIN} && \

	mkdir -p /etc/lumenvox && \
	mv ${ARTIFACTS}/volumes/etc-lumenvox/*																		${LOCALETC}/ && \

	mv ${ARTIFACTS}/volumes/opt-speechbridge-bin/*																${SBBIN}/ && \
	mv ${ARTIFACTS}/volumes/opt-speechbridge-templates/*.TEMPLATE												${SBTEMPL}/ && \
	rm -f ${SBTEMPL}/*.NOSPEECH.TEMPLATE && \

	ln -s ${SBBIN}/AsrFacadeLumenvox.dll.config ${SBBIN}/AsrFacadeLumenvox.2.dll.config && \
	ln -s ${SBCFG}/AudioMgr.exe.config ${SBBIN}/AudioMgr.exe.config && \
	ln -s ${SBCFG}/DialogMgr.exe.config ${SBBIN}/DialogMgr.exe.config && \
	ln -s ${SBCFG}/sbdbutils.exe.config ${SBBIN}/sbdbutils.exe.config && \
	ln -s ${SBCFG}/SBLocalRM.exe.config ${SBBIN}/SBLocalRM.exe.config && \
	ln -s ${SBCFG}/SBSched.exe.config ${SBBIN}/SBSched.exe.config && \

	ln -s ${SBBASE}/license/opt_swift_etc_concurrency_license.txt /opt/swift/etc/concurrency_license.txt && \
	ln -s ${SBBASE}/license/opt_swift_voices_Callie8khz_license.txt /opt/swift/voices/Callie-8kHz/license.txt && \

	rm -Rf ${ARTIFACTS} && \

	chmod +x ${SBBIN}/*.sh && \

	ln -s /usr/lib64/libc-2.17.so /usr/lib64/libc.so

CMD ["/bin/true"]
