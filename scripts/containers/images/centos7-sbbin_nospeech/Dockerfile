# Copyright 2017 Incendonet Inc., All Rights Reserved
# This Dockerfile builds an image with no 3rd party dependencies (ASR or TTS)

FROM incendonet/centos7-mono
MAINTAINER info@incendonet.com

# Env setup.  Note:  The usual env vars aren't expanded in the ENV section.
ENV	\
	HOME="/root" \
	LOCALETC="/usr/local/etc" \
	SBBASE="/opt/speechbridge" \
	SBBIN="/opt/speechbridge/bin" \
	SBCFG="/opt/speechbridge/config" \
	SBSBCONFIG="/opt/speechbridge/SBConfig" \
	SBTEMPL="/opt/speechbridge/templates" \
	ARCHIVE="drop"

# Get updates and build deps
RUN \
	yum -y update &&\
	yum -y install \
		bind-utils \
		less \
		unzip &&\
	yum clean all

# Copy in required files
COPY software/${ARCHIVE}.zip ${SBBASE}/

# Put files where we want them
WORKDIR ${SBBASE}
RUN \
	unzip ${ARCHIVE}.zip &&\
	rm -f ${ARCHIVE}.zip &&\

	mkdir -p ${SBBIN} &&\
	mkdir -p ${SBTEMPL} &&\
	mkdir -p ${SBSBCONFIG} &&\

	mv ${ARCHIVE}/dotnet/AppGenerator/bin/Release/AppGenerator.dll												${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/asrtest/bin/Release/asrtest.exe														${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/AsrFacadeLumenvox/bin/Release/AsrFacadeLumenvox.dll									${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/AsrFacadeLumenvox.2/bin/Release/AsrFacadeLumenvox.2.dll								${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/AsrFacadeLumenvox.2/bin/Release/AsrFacadeLumenvox.2.dll.config							${SBBIN}/AsrFacadeLumenvox.dll.config &&\
	mv ${ARCHIVE}/dotnet/AsrFacadeNull/bin/Release/AsrFacadeNull.dll											${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/AudioEngine_Console/bin/Release/AudioMgr.exe											${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/CleanNamesFolder/bin/Release/CleanNamesFolder.exe										${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/DialogEngine_Console/bin/Release/DialogMgr.exe											${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/Incendonet.Utilities.LogClient/bin/Release/Incendonet.Utilities.LogClient.dll			${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/Incendonet.Utilities.StringHelper/bin/Release/Incendonet.Utilities.StringHelper.dll	${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/Incendonet.Plugins.UserDirectory/bin/Release/Incendonet.Plugins.UserDirectory.dll		${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/ISMessaging/bin/Release/ISMessaging.dll												${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/passgen/bin/Release/passgen.exe														${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/passgen/bin/Release/MlkPwgen.dll														${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SBConfig/bin/Release/SBConfig.dll														${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SBConfigStor/bin/Release/Npgsql.dll													${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SBConfigStor/bin/Release/NSpring.dll													${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SBConfigStor/bin/Release/SBConfigStor.dll												${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/sbdbutils/bin/Release/sbdbutils.exe													${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SBEmail/bin/Release/SBEmail.dll														${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SBLdapConn/bin/Release/SBLdapConn.dll													${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SBLocalRM/bin/Release/SBLocalRM.exe													${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SBResourceMgr/bin/Release/SBResourceMgr.dll											${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SBSched/bin/Release/SBSched.exe														${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SBTTS/bin/Release/SBTTS.dll															${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/sbtest/bin/Release/sbtest.exe															${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/SimpleAES/bin/Release/SimpleAES.dll													${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/PromptSelector/bin/Release/PromptSelector.dll											${SBBIN} &&\
	mv ${ARCHIVE}/dotnet/XmlDocParser/bin/Release/XmlDocParser.dll												${SBBIN} &&\

	mkdir -p /etc/lumenvox &&\
	mv ${ARCHIVE}/containers/volumes/etc-lumenvox/*																${LOCALETC}/ &&\
	
	mv ${ARCHIVE}/containers/volumes/opt-speechbridge-bin/*														${SBBIN}/ &&\
	mv ${ARCHIVE}/containers/volumes/opt-speechbridge-templates/*.TEMPLATE										${SBTEMPL}/ &&\
	mv -f ${SBTEMPL}/AudioMgr.exe.config.NOSPEECH.TEMPLATE ${SBTEMPL}/AudioMgr.exe.config.TEMPLATE &&\

	ln -s ${SBBIN}/AsrFacadeLumenvox.dll.config ${SBBIN}/AsrFacadeLumenvox.2.dll.config &&\
	ln -s ${SBCFG}/AudioMgr.exe.config ${SBBIN}/AudioMgr.exe.config &&\
	ln -s ${SBCFG}/DialogMgr.exe.config ${SBBIN}/DialogMgr.exe.config &&\
	ln -s ${SBCFG}/sbdbutils.exe.config ${SBBIN}/sbdbutils.exe.config &&\
	ln -s ${SBCFG}/SBLocalRM.exe.config ${SBBIN}/SBLocalRM.exe.config &&\
	ln -s ${SBCFG}/SBSched.exe.config ${SBBIN}/SBSched.exe.config &&\
	
	rm -Rf ${ARCHIVE} &&\

	chmod +x ${SBBIN}/*.sh &&\

	ln -s /usr/lib64/libc-2.17.so /usr/lib64/libc.so

CMD ["/bin/true"]
