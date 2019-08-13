Summary: SpeechBridge 4.2.1 Installer
Name: speechbridge
Version: 4.2.1
Release: 13007
License: Incendonet
Group: Applications/Communications

URL: http://www.incendonet.com/

#BuildRoot: %{_tmppath}/%{name}-%{version}-%{release}-buildroot
BuildRoot: /home/speechbridge/src/rpmbuild/%{name}-%{version}-%{release}-buildroot
BuildArch: i686

%define BUILDVERDASH   4-2-1

%define SBBASE         /opt/speechbridge
%define SBIBASE        /opt/speechbridge/installer

%define BAK            %{SBBASE}/backup
%define BIN            %{SBBASE}/bin
%define CONFIG         %{SBBASE}/config
%define SBLOGS         %{SBBASE}/logs
%define SBCONF         %{SBBASE}/SBConfig
%define VDS            %{SBBASE}/VoiceDocStore

%define IBIN            %{SBIBASE}/bin
%define ICONFIG         %{SBIBASE}/config
%define ISBLOGS         %{SBIBASE}/logs
%define ISBCONF         %{SBIBASE}/SBConfig
%define IVDS            %{SBIBASE}/VoiceDocStore

%define INSTLOG        %{SBBASE}/logs/InstallLog_%{name}-%{version}-%{release}.txt
%define SWDIR          /home/speechbridge/software
%define NEWMONODIR     /opt/novell/mono/bin
%define HTTPDCONF      /etc/httpd/conf
%define HTTPDCONFD     /etc/httpd/conf.d

%description
This package will update SpeechBridge from version 4.1.1 to 4.2.1

#Requires: apr, apr-devel, apr-util, apr-util-devel, cairo, cairo-devel, fontconfig-devel, freetype, freetype-devel, giflib, giflib-devel, httpd, httpd-devel, httpd-manual, libexif, libexif-devel, libjpeg-devel, libpng, libpng-devel, libtiff, libtiff-devel, libX11, libX11-devel, libXau-devel, libXdmcp-devel, libXext-devel, libXft-devel, libXrender-devel, mesa-libGL, mesa-libGL-devel, mod_ssl, pango, pango-devel, xorg-x11-proto-devel
Requires: speechbridge = 4.1.1
#Requires: mod_mono-addon >= 2.6.3, mono-addon-core >= 2.6.7, mono-addon-data-2.6.7-6.1.i386, mono-addon-data-postgresql >= 2.6.7, mono-addon-data-sqlite >=2.6.7, mono-addon-extras >= 2.6.7, mono-addon-libgdiplus0 >= 2.6.7, mono-addon-wcf >= , .6.7, mono-addon-web >= 2.6.7, mono-addon-winforms >= 2.6.7, mono-addon-xsp >= 2.6.5	# Some installations may be running later releases of Mono
Obsoletes: sbupdate-2.0.2-42_09042, sbupdate-3.0.1-301, sbupdate-3.1.1-178, speechbridge-4.0.1-157, speechbridge-4.1.1-243

#####################################################################
%pre
#####################################################################
#### NOTE:  We'd like to check the installed version here to know if we need to abort, but rpmbuild
#### appears to evaluate all macros at build time, and the rpm command does not when installing.
#### How do other installers (like postgresql) do upgrades in RPMs?

%prep

%build

#####################################################################
%install
#####################################################################
#rm -rf %{buildroot}

#####################################################################
%clean
#####################################################################
#rm -rf %{buildroot}

#####################################################################
%post
#####################################################################
#### NOTE:  We'd like to check the installed version here to know if we need to abort, but rpmbuild
#### appears to evaluate all macros at build time, and the rpm command does not when installing.
#### How do other installers (like postgresql) do upgrades in RPMs?

/bin/echo "Starting install of %{name}-%{version}-%{release}." >> %{INSTLOG}

/sbin/service speechbridged stop >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Creating backup directories...." >> %{INSTLOG}

mkdir -p %{BAK}/%{name}-%{version}.%{release}/bin                        >> %{INSTLOG} 2>> %{INSTLOG}
mkdir -p %{BAK}/%{name}-%{version}.%{release}/config                     >> %{INSTLOG} 2>> %{INSTLOG}
mkdir -p %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content    >> %{INSTLOG} 2>> %{INSTLOG}
mkdir -p %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/docs       >> %{INSTLOG} 2>> %{INSTLOG}
mkdir -p %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images     >> %{INSTLOG} 2>> %{INSTLOG}
mkdir -p %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/js         >> %{INSTLOG} 2>> %{INSTLOG}
mkdir -p %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore              >> %{INSTLOG} 2>> %{INSTLOG}

#/bin/echo "Removing old unused files..." >> %{INSTLOG}

#rm -f %{BIN}/NSpring.dll                         >> %{INSTLOG} 2>> %{INSTLOG}

#/bin/echo "Backing up files..." >> %{INSTLOG}

#mv -f %{BIN}/libPocoFoundation.so.11             %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_C5_ShoretelSip_20080707    %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_C5_GenericSip_20080707     %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_C5_20110407                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_C5_20111115                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/ProxySrv_optnoshared_centos44       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/SBLauncher                          %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/CepstralCmd                         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/NeospeechCmd_L08                    %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{BIN}/sbfailover                          %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/sbbackup.cron                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/sbreportgen.cron                    %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/speechbridgemon.cron                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{BIN}/sbfailover.cron                     %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/sblogcleanup.cron                   %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/sbrmoldlogs.cron                    %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{BIN}/sbfailover.sh                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/sbreportyestsend.sh                 %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/asrtest.exe                         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AudioMgr.exe                        %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/DialogMgr.exe                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/MigrationTool_DIDPrompt.exe         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/sbdbutils.exe                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBLocalRM.exe                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBSched.exe                         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/nspring.dll                         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/NSpring.dll                         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AppGenerator.dll                    %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AsrFacadeLumenvox.dll               %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AsrFacadeNull.dll                   %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/ISMessaging.dll                     %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/PromptSelector.dll                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBConfig.dll                        %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBConfigStor.dll                    %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBEmail.dll                         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBLdapConn.dll                      %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBResourceMgr.dll                   %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBTTS.dll                           %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/SBUnitTests.dll                     %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SimpleAES.dll                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/XmlScriptParser.dll                 %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/speechbridged                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/neospeechd                          %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{BIN}/AudioMgr.exe.config                 %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{BIN}/DialogMgr.exe.config                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{BIN}/sbdbutils.exe.config                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{BIN}/SBLocalRM.exe.config                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
cp -f %{BIN}/SBSched.exe.config                  %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{BIN}/AsrFacadeLumenvox.dll.config        %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{BIN}/oss-application.conf.xml            %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{CONFIG}/SBPreCreate_pgsql.sql            %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{CONFIG}/SBCreate_pgsql.sql               %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{CONFIG}/SBUpdate_DD_pgsql.sql            %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{CONFIG}/English_pgsql.sql                %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{CONFIG}/Spanish_pgsql.sql                %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SWDIR}/neoinstall.sh                     %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{CONFIG}/setbranding.sh                   %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{CONFIG}/sbddinstall.sh                   %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{CONFIG}/cluster.notification             %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/index.html                       %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/index-rootdir.html               %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/AADirectory.aspx                 %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Default.aspx                     %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/EulaAccept.aspx                  %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/GenHash.aspx                     %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/GreetingPrompts.aspx             %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/GreetingPromptsForDid.aspx       %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Help.aspx                        %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login.aspx                       %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login_AlcatelLucent.aspx         %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login_AV.aspx                    %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login_Incendonet.aspx            %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login_NEC.aspx                   %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/LoginIncorrect.aspx              %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Logout.aspx                      %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/MenuEditor.aspx                  %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/MenuEditor.aspx_                 %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/MenuManager.aspx                 %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/MenuManager.aspx_                %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/ServerSettings.aspx              %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/ServerSettingsLdap.aspx          %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/UserEmailProps.aspx              %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
cp -f %{SBCONF}/Web.config                       %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/assets/style.css                 %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/assets/DD_style.css              %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_ActiveVoice.css     %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_AlcatelLucent.css   %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_AV.css              %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_Incendonet.css      %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_NEC.css             %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/BizHours.xml      %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/EULA.txt          %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/AlcatelLucent_EULA.txt                     %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/AV_EULA.txt                                %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/Incendonet_EULA.txt                        %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/Incendonet_EULA_ABBR.txt                   %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/favicon.ico                                        %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage.png                               %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_AlcatelLucent.png                 %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_AV.png                            %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_Incendonet.png                    %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_NEC.png                           %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
cp -f %{VDS}/AAMain.vxml.xml                     %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
cp -f %{VDS}/AAMain_Template.vxml.xml            %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{VDS}/CalendarMain_Template.vxml.xml      %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{VDS}/EmailMain_Template.vxml.xml         %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/SBRoot.vxml.xml                     %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{VDS}/SBRoot.vxml.xml.up                  %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/SBRoot.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/ABNFBoolean.gram                    %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/ABNFBoolean_es-MX.gram              %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/ABNFDate.gram                       %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/ABNFDate_es-MX.gram                 %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/ABNFDigits.gram                     %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/ABNFDigits_es-MX.gram               %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{VDS}/Prompts/IntermissionShort.wav       %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/Prompts >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{VDS}/Prompts/MainMenu.wav                %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/Prompts >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{VDS}/Prompts/ThankYouForCalling.wav      %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/Prompts >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{SWDIR}/sbpostinstall.sh                  %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f /etc/profile                               %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{HTTPDCONF}/httpd.conf                    %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{HTTPDCONFD}/mod_mono.conf                %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f /var/lib/pgsql/data/pg_hba.conf            %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}


#/bin/echo "Fixing old files..."
#mv -f %{VDS}/SBRoot.vxml.xml.up                       %{VDS}/SBRoot.vxml.xml                    >> %{INSTLOG} 2>> %{INSTLOG}


/bin/echo "Moving new files..." >> %{INSTLOG}

#mkdir -p %{BIN}                                    >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{CONFIG}                                 >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{SBLOGS}                                 >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{SBCONF}                                 >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{VDS}/Prompts/Names                      >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{SWDIR}                                  >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{SBCONF}/assets/content                  >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{SBCONF}/assets/docs                     >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{SBCONF}/assets/images                   >> %{INSTLOG} 2>> %{INSTLOG}
mkdir -p %{SBCONF}/assets/js                       >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{SBCONF}/bin                             >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{SBBASE}/SBReports                       >> %{INSTLOG} 2>> %{INSTLOG}

# Libraries, binaries, scripts, & cron
#mv -f %{IBIN}/libPocoFoundation.so.11               %{BIN}/libPocoFoundation.so.11           >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/sbfailover                            %{BIN}/sbfailover                        >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/speechbridged                         %{BIN}/speechbridged                     >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/neospeechd                            %{BIN}/neospeechd                        >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/sbbackup.cron                         %{BIN}/sbbackup.cron                     >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/sbfailover.cron                       %{BIN}/sbfailover.cron                   >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/sbreportgen.cron                      %{BIN}/sbreportgen.cron                  >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/sbrmoldlogs.cron                      %{BIN}/sbrmoldlogs.cron                  >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/speechbridgemon.cron                  %{BIN}/speechbridgemon.cron              >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/ProxySrv_optnoshared_centos44         %{BIN}/ProxySrv_optnoshared_centos44     >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/AudioRtr_C5_20110407                  %{BIN}/AudioRtr_C5_20110407              >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/AudioRtr_C5_20111115                  %{BIN}/AudioRtr_C5_20111115              >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/SBLauncher                            %{BIN}/SBLauncher                        >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/CepstralCmd                           %{BIN}/CepstralCmd                       >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/NeospeechCmd                          %{BIN}/NeospeechCmd                      >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/AudioMgr.sh                           %{BIN}/AudioMgr.sh                       >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/DialogMgr.sh                          %{BIN}/DialogMgr.sh                      >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/SBSched.sh                            %{BIN}/SBSched.sh                        >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/sbfailover.sh                         %{BIN}/sbfailover.sh                     >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/sbreportyestsend.sh                   %{BIN}/sbreportyestsend.cron             >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/importdialog.sh                       %{BIN}/importdialog.sh                   >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/exportdialog.sh                       %{BIN}/exportdialog.sh                   >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/AudioMgr.exe                          %{BIN}/AudioMgr.exe                      >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/asrtest.exe                           %{BIN}/asrtest.exe                       >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/DialogMgr.exe                         %{BIN}/DialogMgr.exe                     >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/MigrationTool_DIDPrompt.exe           %{BIN}/MigrationTool_DIDPrompt.exe       >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/sbdbutils.exe                         %{BIN}/sbdbutils.exe                     >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/SBLocalRM.exe                         %{BIN}/SBLocalRM.exe                     >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/SBSched.exe                           %{BIN}/SBSched.exe                       >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/sbtest.exe                            %{BIN}/sbtest.exe                        >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/nspring.dll                           %{BIN}/nspring.dll                       >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/AppGenerator.dll                      %{BIN}/AppGenerator.dll                  >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/AsrFacadeLumenvox.dll                 %{BIN}/AsrFacadeLumenvox.dll             >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/AsrFacadeNull.dll                     %{BIN}/AsrFacadeNull.dll                 >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/ISMessaging.dll                       %{BIN}/ISMessaging.dll                   >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/PromptSelector.dll                    %{BIN}/PromptSelector.dll                   >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/SBConfig.dll                          %{BIN}/SBConfig.dll                      >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/SBConfigStor.dll                      %{BIN}/SBConfigStor.dll                  >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/SBEmail.dll                           %{BIN}/SBEmail.dll                       >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/SBLdapConn.dll                        %{BIN}/SBLdapConn.dll                    >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/SBResourceMgr.dll                     %{BIN}/SBResourceMgr.dll                 >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/SBTTS.dll                             %{BIN}/SBTTS.dll                         >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/SBUnitTests.dll                       %{BIN}/SBUnitTests.dll                   >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/SimpleAES.dll                         %{BIN}/SimpleAES.dll                     >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IBIN}/XmlScriptParser.dll                   %{BIN}/XmlScriptParser.dll               >> %{INSTLOG} 2>> %{INSTLOG}

# Website
#mv -f %{ISBCONF}/index.html                         %{SBCONF}/index.html >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/index-rootdir.html                 %{SBCONF}/index-rootdir.html >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/AADirectory.aspx                   %{SBCONF}/AADirectory.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/Default.aspx                       %{SBCONF}/Default.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/EulaAccept.aspx                    %{SBCONF}/EulaAccept.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/GenHash.aspx                       %{SBCONF}/GenHash.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/GreetingPrompts.aspx               %{SBCONF}/GreetingPrompts.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/GreetingPromptsForDid.aspx         %{SBCONF}/GreetingPromptsForDid.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/Help.aspx                          %{SBCONF}/Help.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/Login.aspx                         %{SBCONF}/Login.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/Login_AlcatelLucent.aspx           %{SBCONF}/Login_AlcatelLucent.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/Login_AV.aspx                      %{SBCONF}/Login_AV.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/Login_Incendonet.aspx              %{SBCONF}/Login_Incendonet.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/Login_NEC.aspx                     %{SBCONF}/Login_NEC.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/LoginIncorrect.aspx                %{SBCONF}/LoginIncorrect.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/Logout.aspx                        %{SBCONF}/Logout.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/MenuEditor.aspx                    %{SBCONF}/MenuEditor.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/MenuEditor.aspx_                   %{SBCONF}/MenuEditor.aspx_ >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/MenuManager.aspx                   %{SBCONF}/MenuManager.aspx >> %{INSTLOG} 2>> %{INSTLOG#}
#mv -f %{ISBCONF}/MenuManager.aspx_                  %{SBCONF}/MenuManager.aspx_ >> %{INSTLOG} 2>> %{INSTLOG#}
mv -f %{ISBCONF}/ServerSettings.aspx                %{SBCONF}/ServerSettings.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/ServerSettingsLdap.aspx            %{SBCONF}/ServerSettingsLdap.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/UserEmailProps.aspx                %{SBCONF}/UserEmailProps.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/Web.config                         %{SBCONF}/Web.config >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/assets/style.css                   %{SBCONF}/assets/style.css >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/assets/DD_style.css                %{SBCONF}/assets/DD_style.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/style_ActiveVoice.css       %{SBCONF}/assets/style_ActiveVoice.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/style_AlcatelLucent.css     %{SBCONF}/assets/style_AlcatelLucent.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/style_AV.css                %{SBCONF}/assets/style_AV.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/style_Incendonet.css        %{SBCONF}/assets/style_Incendonet.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/style_NEC.css               %{SBCONF}/assets/style_NEC.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/content/BizHours.xml        %{SBCONF}/assets/content/BizHours.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/content/EULA.txt            %{SBCONF}/assets/content/EULA.txt >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/content/AlcatelLucent_EULA.txt                      %{SBCONF}/assets/content/AlcatelLucent_EULA.txt >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/content/AV_EULA.txt                                 %{SBCONF}/assets/content/AV_EULA.txt >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/content/Incendonet_EULA.txt                         %{SBCONF}/assets/content/Incendonet_EULA.txt >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/content/NEC_EULA.txt                                %{SBCONF}/assets/content/NEC_EULA.txt >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/favicon.ico                                         %{SBCONF}/assets/favicon.ico >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/images/LogoImage.png                                %{SBCONF}/assets/images/LogoImage.png >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/images/LogoImage_AlcatelLucent.png                  %{SBCONF}/assets/images/LogoImage_AlcatelLucent.png >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/images/LogoImage_AV.png                             %{SBCONF}/assets/images/LogoImage_AV.png >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/images/LogoImage_Incendonet.png                     %{SBCONF}/assets/images/LogoImage_Incendonet.png >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/images/LogoImage_NEC.png                            %{SBCONF}/assets/images/LogoImage_NEC.png >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/assets/js/AADirectory.js                                   %{SBCONF}/assets/js/AADirectory.js >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/assets/js/CheckboxSupport.js                               %{SBCONF}/assets/js/CheckboxSupport.js >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/assets/js/MenuEditor.js                                    %{SBCONF}/assets/js/MenuEditor.js >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/assets/js/ServerSettings.js                                %{SBCONF}/assets/js/ServerSettings.js >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/assets/js/ServerSettingsLdap.js                            %{SBCONF}/assets/js/ServerSettingsLdap.js >> %{INSTLOG} 2>> %{INSTLOG}

# Documentation
#mv -f %{ISBCONF}/assets/docs/SpeechBridgeUserGuide_4-0.pdf                  %{SBCONF}/assets/docs/SpeechBridgeUserGuide_4-0.pdf >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side1_4-0.pdf       %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side1_4-0.pdf >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side2_4-0.pdf       %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side2_4-0.pdf >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISBCONF}/assets/docs/SpeechBridgeAdminGuide_4-0.pdf                 %{SBCONF}/assets/docs/SpeechBridgeAdminGuide_4-0.pdf >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ISBCONF}/assets/docs/SpeechBridgeAdminGuide_4-2.pdf                 %{SBCONF}/assets/docs/SpeechBridgeAdminGuide_4-2.pdf >> %{INSTLOG} 2>> %{INSTLOG}

# Configuration files
#mv -f %{IBIN}/AudioMgr.exe.config                   %{BIN}/AudioMgr.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/DialogMgr.exe.config                  %{BIN}/DialogMgr.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/sbdbutils.exe.config                  %{BIN}/sbdbutils.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/SBLocalRM.exe.config                  %{BIN}/SBLocalRM.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/SBSched.exe.config                    %{BIN}/SBSched.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/AsrFacadeLumenvox.dll.config          %{BIN}/AsrFacadeLumenvox.dll.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IBIN}/oss-application.conf.xml              %{BIN}/oss-application.conf.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/0.cfg                              %{CONFIG}/0.cfg >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/1.cfg                              %{CONFIG}/1.cfg >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/ProxySrv.config                    %{CONFIG}/ProxySrv.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/cluster.notification               %{CONFIG}/cluster.notification >> %{INSTLOG} 2>> %{INSTLOG}

# Configuration scripts
#mv -f %{ICONFIG}/neoinstall.sh                      %{SWDIR}/neoinstall.sh >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/setbranding.sh                     %{CONFIG}/setbranding.sh >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ISWDIR}/sbpostinstall.sh                    %{SWDIR}/sbpostinstall.sh >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/sbddinstall.sh                     %{CONFIG}/sbddinstall.sh >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/sb411upprotts.sh                   %{CONFIG}/sb411upprotts.sh >> %{INSTLOG} 2>> %{INSTLOG}

# Database initialization & update scripts
#mv -f %{ICONFIG}/SBPreCreate_pgsql.sql                                        %{CONFIG}/SBPreCreate_pgsql.sql >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ICONFIG}/SBCreate_pgsql.sql                                           %{CONFIG}/SBCreate_pgsql.sql >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ICONFIG}/SBUpdate_%{BUILDVERDASH}_pgsql.sql                           %{CONFIG}/SBUpdate_%{BUILDVERDASH}_pgsql.sql >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{ICONFIG}/SBUpdate_DD_pgsql.sql                                        %{CONFIG}/SBUpdate_DD_pgsql.sql >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/SBUpdate_DD_%{BUILDVERDASH}_pgsql.sql                        %{CONFIG}/SBUpdate_DD_%{BUILDVERDASH}_pgsql.sql >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/English_pgsql.sql                                            %{CONFIG}/English_pgsql.sql >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/Spanish_pgsql.sql                                            %{CONFIG}/Spanish_pgsql.sql >> %{INSTLOG} 2>> %{INSTLOG}

# VoiceXML and grammars
#mv -f %{IVDS}/AAMain.vxml.xml                       %{VDS}/AAMain.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IVDS}/AAMain_Template_4-2-1.vxml.xml        %{VDS}/AAMain_Template_4-2-1.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}	# The template will be updated (or not) by the calling shell script.
#mv -f %{IVDS}/CalendarMain.vxml.xml                 %{VDS}/CalendarMain.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/CalendarMain_Template.vxml.xml        %{VDS}/CalendarMain_Template.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/EmailMain.vxml.xml                    %{VDS}/EmailMain.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/EmailMain_Template.vxml.xml           %{VDS}/EmailMain_Template.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/SBRoot.vxml.xml                       %{VDS}/SBRoot.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/ABNFBoolean.gram                      %{VDS}/ABNFBoolean.gram >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/ABNFBoolean_es-MX.gram                %{VDS}/ABNFBoolean_es-MX.gram >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/ABNFDate.gram                         %{VDS}/ABNFDate.gram >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/ABNFDate_es-MX.gram                   %{VDS}/ABNFDate_es-MX.gram >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/ABNFDigits.gram                       %{VDS}/ABNFDigits.gram >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/ABNFDigits_es-MX.gram                 %{VDS}/ABNFDigits_es-MX.gram >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/prompts_20120323.tar.gz               %{VDS}/prompts_20120323.tar.gz >> %{INSTLOG} 2>> %{INSTLOG}

# Prompts
#tar -xzf %{IVDS}/prompts_20120323.tar.gz -C %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/Prompts/empty.wav                     %{VDS}/Prompts/empty.wav >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/Prompts/IntermissionShort.wav         %{VDS}/Prompts/IntermissionShort.wav >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/Prompts/MainMenu.wav                  %{VDS}/Prompts/MainMenu.wav >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/Prompts/ThankYouForCalling.wav        %{VDS}/Prompts/ThankYouForCalling.wav >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{IVDS}/Prompts/ThankYouForCallingPleaseSayTheName.wav        %{VDS}/Prompts/ThankYouForCallingPleaseSayTheName.wav >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{IVDS}/Prompts/Silence-7sec.wav              %{VDS}/Prompts/Silence-7sec.wav >> %{INSTLOG} 2>> %{INSTLOG}

# OS & middleware configuration files
#mv -f %{ICONFIG}/_etc_profile                        /etc/profile >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/httpd.conf                          %{HTTPDCONF}/httpd.conf >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/mod_mono.conf                       %{HTTPDCONFD}/mod_mono.conf >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{ICONFIG}/pg_hba.conf                         /var/lib/pgsql/data/pg_hba.conf >> %{INSTLOG} 2>> %{INSTLOG}

# Remove files that don't belong in the install

/bin/echo "Setting permissions..." >> %{INSTLOG}
# Note this is done to make sure that ALL files have the proper permissions.  The 'Files' section
# at the bottom only sets permissions on the files in the RPM.
chmod ugo=rx   %{SBBASE}                                                   >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rx   %{SBBASE}/bin                                               >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=r     %{SBBASE}/bin/*.so.*                                        >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=rx    %{SBBASE}/bin/*.cron                                        >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=rx    %{SBBASE}/bin/*.sh                                          >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=rx    %{SBBASE}/bin/speechbridged                                 >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=rx    %{SBBASE}/bin/neospeechd                                    >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=rx    %{SBBASE}/bin/SBLauncher                                    >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=rx    %{SBBASE}/bin/CepstralCmd                                   >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=rx    %{SBBASE}/bin/NeospeechCmd                                  >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=r     %{SBBASE}/bin/*.exe                                         >> %{INSTLOG} 2>> %{INSTLOG}
chmod og=r     %{SBBASE}/bin/*.dll                                         >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=r     %{SBBASE}/bin/*.config                                      >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=r     %{SBBASE}/bin/*.xml                                         >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rwx  %{SBBASE}/config                                            >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=rwx   %{SBBASE}/config/*.sh                                       >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rw   %{SBBASE}/config/*.cfg                                      >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rw   %{SBBASE}/config/ProxySrv.config                            >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rw   %{SBBASE}/config/cluster.notification                       >> %{INSTLOG} 2>> %{INSTLOG}
chmod -R ugo=r %{SBBASE}/SBConfig                                          >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rx   %{SBBASE}/SBConfig                                          >> %{INSTLOG} 2>> %{INSTLOG}
chmod u=r      %{SBBASE}/SBConfig/*.aspx_                                  >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rx   %{SBBASE}/SBConfig/assets                                   >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rx   %{SBBASE}/SBConfig/assets/content                           >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rx   %{SBBASE}/SBConfig/assets/docs                              >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rx   %{SBBASE}/SBConfig/assets/images                            >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rx   %{SBBASE}/SBConfig/assets/js                                >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rx   %{SBBASE}/SBConfig/bin                                      >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug=rx    %{SBLOGS}                                                   >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rx   %{SBBASE}/VoiceDocStore                                     >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo+w    %{SBBASE}/VoiceDocStore                                     >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=r    %{SBBASE}/VoiceDocStore/*_Template.vxml.xml                 >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/SBRoot.vxml.xml                     >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rw   %{SBBASE}/VoiceDocStore/AAMain.vxml.xml                     >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rw   %{SBBASE}/VoiceDocStore/EmailMain.vxml.xml                  >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rw   %{SBBASE}/VoiceDocStore/CalendarMain.vxml.xml               >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=r    %{SBBASE}/VoiceDocStore/*.gram                              >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rwx  %{SBBASE}/VoiceDocStore/Prompts                             >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/Prompts/*.wav                       >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/Prompts/AHGreeting2.wav             >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/Prompts/PleaseSayTheName.wav        >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/Prompts/ThankYouForCalling.wav      >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/Prompts/Silence-7sec.wav            >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rwx  %{SBBASE}/VoiceDocStore/Prompts/Names                       >> %{INSTLOG} 2>> %{INSTLOG}
chmod ugo=rx   %{SBBASE}/SBReports                                         >> %{INSTLOG} 2>> %{INSTLOG}
chmod ug+w     %{SBBASE}/SBReports                                         >> %{INSTLOG} 2>> %{INSTLOG}
chown -R speechbridge:speechbridge %{SBBASE}                               >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Creating symbolic links..." >> %{INSTLOG}

#ln -fs %{BIN}/libPocoFoundation.so.11              /usr/lib/libPocoFoundation.so.11      >> %{INSTLOG} 2>> %{INSTLOG}
#rm -f %{BIN}/AudioRtr                                                                    >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/AudioRtr_C5_20110407                 %{BIN}/AudioRtr                       >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/AudioRtr_C5_20111115                 %{BIN}/AudioRtr                       >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/ProxySrv_optnoshared_centos44        %{BIN}/ProxySrv                       >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/nspring.dll                          %{BIN}/NSpring.dll                    >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/sbbackup.cron                        /etc/cron.daily/sbbackup.cron         >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/sbreportgen.cron                     /etc/cron.daily/sbreportgen.cron      >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/sbrmoldlogs.cron                     /etc/cron.daily/sbrmoldlogs.cron      >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/speechbridgemon.cron                 /etc/cron.hourly/speechbridgemon.cron >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{SBBASE}/SBReports                         /var/www/html/SBReports               >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/AppGenerator.dll                     %{SBCONF}/bin/AppGenerator.dll      >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/ISMessaging.dll                      %{SBCONF}/bin/ISMessaging.dll       >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/nspring.dll                          %{SBCONF}/bin/nspring.dll           >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/nspring.dll                          %{SBCONF}/bin/NSpring.dll           >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/SBConfig.dll                         %{SBCONF}/bin/SBConfig.dll          >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/SBConfigStor.dll                     %{SBCONF}/bin/SBConfigStor.dll      >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/SBEmail.dll                          %{SBCONF}/bin/SBEmail.dll           >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/SBLdapConn.dll                       %{SBCONF}/bin/SBLdapConn.dll        >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/SBResourceMgr.dll                    %{SBCONF}/bin/SBResourceMgr.dll     >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/SBTTS.dll                            %{SBCONF}/bin/SBTTS.dll             >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/SimpleAES.dll                        %{SBCONF}/bin/SimpleAES.dll         >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/XmlScriptParser.dll                  %{SBCONF}/bin/XmlScriptParser.dll   >> %{INSTLOG} 2>> %{INSTLOG}
ln -fs %{SBCONF}/assets/docs/SpeechBridgeUserGuide_4-0.pdf         %{SBCONF}/assets/docs/SpeechBridgeUserGuide.pdf   >> %{INSTLOG} 2>> %{INSTLOG}
ln -fs %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side1_4-0.pdf         %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side1.pdf   >> %{INSTLOG} 2>> %{INSTLOG}
ln -fs %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side2_4-0.pdf         %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side2.pdf   >> %{INSTLOG} 2>> %{INSTLOG}
ln -fs %{SBCONF}/assets/docs/SpeechBridgeAdminGuide_4-2.pdf         %{SBCONF}/assets/docs/SpeechBridgeAdminGuide.pdf   >> %{INSTLOG} 2>> %{INSTLOG}


#echo "Installing SpeechBridge daemon..." >> %{INSTLOG}

#ln -s %{BIN}/speechbridged /etc/init.d                                     >> %{INSTLOG} 2>> %{INSTLOG}
#chkconfig speechbridged --add                                              >> %{INSTLOG} 2>> %{INSTLOG}
#chkconfig speechbridged on                                                 >> %{INSTLOG} 2>> %{INSTLOG}

echo "Configuring PostgreSql database..." >> %{INSTLOG}

chown postgres:postgres %{CONFIG}/*.sql                                    >> %{INSTLOG} 2>> %{INSTLOG}
#service postgresql stop                                                    >> %{INSTLOG} 2>> %{INSTLOG}
#service postgresql start                                                   >> %{INSTLOG} 2>> %{INSTLOG}

#cp -f /etc/sudoers /etc/sudoers.1 
#sed 's/Defaults    requiretty/#Defaults    requiretty/g' /etc/sudoers.1 > /etc/sudoers   2>> %{INSTLOG}
#sudo -u postgres psql -f %{CONFIG}/SBPreCreate_pgsql.sql                   >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f /etc/sudoers.1 /etc/sudoers                                          >> %{INSTLOG} 2>> %{INSTLOG}
#rm -f /etc/sudoers.1                                                       >> %{INSTLOG} 2>> %{INSTLOG}

#/bin/echo "Creating DB tables..." >> %{INSTLOG}
#%{NEWMONODIR}/mono --config %{BIN}/sbdbutils.exe.config %{BIN}/sbdbutils.exe --run-script %{CONFIG}/SBCreate_pgsql.sql          >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Updating DB..." >> %{INSTLOG}
%{NEWMONODIR}/mono --config %{BIN}/sbdbutils.exe.config %{BIN}/sbdbutils.exe --run-script %{CONFIG}/SBUpdate_%{BUILDVERDASH}_pgsql.sql    >> %{INSTLOG} 2>> %{INSTLOG}
#%{NEWMONODIR}/mono --config %{BIN}/sbdbutils.exe.config %{BIN}/sbdbutils.exe --run-script %{CONFIG}/SBUpdate_DD_%{BUILDVERDASH}_pgsql.sql       >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Running MigrationTool..." >> %{INSTLOG}
%{NEWMONODIR}/mono %{BIN}/MigrationTool_DIDPrompt.exe                       >> %{INSTLOG} 2>> %{INSTLOG}
# Set permissions and owner on directories and files created by the MigrationTool
chmod -R ugo=rwx  %{SBBASE}/VoiceDocStore/Prompts/DID                       >> %{INSTLOG} 2>> %{INSTLOG}   # Note: this will also set other files in the path (like WAVs) with execute permisions
chown -R speechbridge:speechbridge %{SBBASE}/VoiceDocStore/Prompts/DID      >> %{INSTLOG} 2>> %{INSTLOG}

#/bin/echo "Configuring Apache and website settings..."                     >> %{INSTLOG}
#ln -fs %{SBBASE}/SBReports /var/www/html/SBReports                         >> %{INSTLOG} 2>> %{INSTLOG}

#cp -f %{SBCONF}/index-rootdir.html /var/www/html/index.html                >> %{INSTLOG} 2>> %{INSTLOG}
#chown speechbridge:speechbridge /var/www/html/index.html                   >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ug+r /var/www/html/index.html                                        >> %{INSTLOG} 2>> %{INSTLOG}

#cp -f %{CONFIG}/httpd.conf /etc/httpd/conf                                 >> %{INSTLOG} 2>> %{INSTLOG}   # Already moved above

#ln -s %{SBCONF} /var/www/html/SBConfig                                     >> %{INSTLOG} 2>> %{INSTLOG}
#ln -s %{VDS} /var/www/html/VoiceDocStore                                   >> %{INSTLOG} 2>> %{INSTLOG}
#ln -s %{SBBASE}/SBReports /var/www/html/SBReports                          >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Updating Web.config and SBSched.exe.config..." >> %{INSTLOG}
/bin/sed 's:<location path="Help.aspx">:<location path="GreetingPromptsForDid.aspx">\
\t\t<system.web>\
\t\t\t<authorization>\
\t\t\t\t<allow users="admin"/>\
\t\t\t\t<deny users="*"/>\
\t\t\t</authorization>\
\t\t</system.web>\
\t</location>\
\t<location path="Help.aspx">:' < %{SBCONF}/Web.config > %{SBCONF}/Web2.config 2>> %{INSTLOG}
mv -f %{SBCONF}/Web2.config %{SBCONF}/Web.config >> %{INSTLOG}  2>> %{INSTLOG}
chown speechbridge:speechbridge %{SBCONF}/Web.config >> %{INSTLOG}  2>> %{INSTLOG}

/bin/sed 's:/var/www/html/VoiceDocStore/:/opt/speechbridge/VoiceDocStore/:' < %{SBCONF}/Web.config > %{SBCONF}/Web2.config 2>> %{INSTLOG}
mv -f %{SBCONF}/Web2.config %{SBCONF}/Web.config >> %{INSTLOG}  2>> %{INSTLOG}
chown speechbridge:speechbridge %{SBCONF}/Web.config >> %{INSTLOG}  2>> %{INSTLOG}

/bin/sed 's:/var/www/html/VoiceDocStore/:/opt/speechbridge/VoiceDocStore/:' < %{BIN}/SBSched.exe.config > %{BIN}/SBSched.exe.config2 2>> %{INSTLOG}
mv -f %{BIN}/SBSched.exe.config2 %{BIN}/SBSched.exe.config >> %{INSTLOG}  2>> %{INSTLOG}
chown speechbridge:speechbridge %{BIN}/SBSched.exe.config >> %{INSTLOG}  2>> %{INSTLOG}

/bin/echo "Running additional scripts..." >> %{INSTLOG}
#%{CONFIG}/sb411upprotts.sh %{INSTLOG}                                                   2>> %{INSTLOG}

/bin/echo "Restarting services..." >> %{INSTLOG}

/sbin/service httpd reload                                                 >> %{INSTLOG} 2>> %{INSTLOG}
/sbin/service speechbridged restart                                        >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Removing unused files..." >> %{INSTLOG}
rm -Rf %{SBIBASE}                                                          >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "%{name}-%{version}-%{release}" >> %{SBBASE}/build.txt
/bin/echo "Install of %{name}-%{version}-%{release} complete." >> %{INSTLOG}


#####################################################################
%postun
#####################################################################
/bin/echo "Starting rollback %{name}-%{version}-%{release}." >> %{INSTLOG}

/sbin/service speechbridged stop >> %{INSTLOG} 2>> %{INSTLOG}
sleep 2

/bin/echo "Restoring backed up files..." >> %{INSTLOG}

#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/libPocoFoundation.so.11                    %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/ProxySrv_optnoshared_centos44              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioRtr_C5_20110407                       %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioRtr_C5_20111115                       %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLauncher                                 %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/CepstralCmd                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/NeospeechCmd_L08                           %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbfailover                                 %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/speechbridged                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/neospeechd                                 %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbreportgen.cron                           %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/speechbridgemon.cron                       %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbfailover.cron                            %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioMgr.sh                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/DialogMgr.sh                               %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBSched.sh                                 %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbfailover.sh                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbreportyestsend.sh                        %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/importdialog.sh                            %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/exportdialog.sh                            %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioMgr.exe                               %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/asrtest.exe                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/DialogMgr.exe                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/MigrationTool_DIDPrompt.exe                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbdbutils.exe                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLocalRM.exe                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBSched.exe                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbtest.exe                                 %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/nspring.dll                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/NSpring.dll                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AppGenerator.dll                           %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AsrFacadeLumenvox.dll                      %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AsrFacadeNull.dll                          %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/ISMessaging.dll                            %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/PromptSelector.dll                         %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBConfig.dll                               %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBConfigStor.dll                           %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBEmail.dll                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLdapConn.dll                             %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBResourceMgr.dll                          %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBTTS.dll                                  %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBUnitTests.dll                            %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SimpleAES.dll                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/XmlScriptParser.dll                        %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioMgr.exe.config                        %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/DialogMgr.exe.config                       %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbdbutils.exe.config                       %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLocalRM.exe.config                       %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBSched.exe.config                         %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AsrFacadeLumenvox.dll.config               %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/oss-application.conf.xml                   %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/SBPreCreate_pgsql.sql                   %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/config/SBCreate_pgsql.sql                      %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/SBUpdate_4-2-1_pgsql.sql                   %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/config/SBUpdate_DD_pgsql.sql                   %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/English_pgsql.sql                        %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/Spanish_pgsql.sql                        %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/neoinstall.sh                           %{SWDIR} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/setbranding.sh                          %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/sbddinstall.sh                          %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/cluster.notification                    %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/index.html                            %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/index-rootdir.html                    %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/AADirectory.aspx                      %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Default.aspx                          %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/EulaAccept.aspx                       %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/GenHash.aspx                          %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/GreetingPrompts.aspx                  %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/GreetingPromptsForDid.aspx            %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Help.aspx                             %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login.aspx                            %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login_AlcatelLucent.aspx              %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login_AV.aspx                         %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login_Incendonet.aspx                 %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login_NEC.aspx                        %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/LoginIncorrect.aspx                   %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Logout.aspx                           %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/MenuEditor.aspx                       %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/MenuEditor.aspx_                      %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/MenuManager.aspx                      %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/MenuManager.aspx_                     %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/ServerSettings.aspx                   %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/ServerSettingsLdap.aspx               %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/UserEmailProps.aspx                   %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Web.config                            %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style.css                                       %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/DD_style.css                                    %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style_ActiveVoice.css                           %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style_AlcatelLucent.css                         %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style_AV.css                                    %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style_Incendonet.css                            %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style_NEC.css                                   %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/BizHours.xml                            %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/EULA.txt                                %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/AlcatelLucent_EULA.txt                  %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/AV_EULA.txt                             %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/Incendonet_EULA.txt                     %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/Incendonet_EULA_ABBR.txt                %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/NEC_EULA.txt                            %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/favicon.ico                                     %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage.png                            %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage_AlcatelLucent.png              %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage_AV.png                         %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage_Incendonet.png                 %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage_NEC.png                        %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/AAMain.vxml.xml                  %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/AAMain_Template.vxml.xml         %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/CalendarMain.vxml.xml            %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/CalendarMain_Template.vxml.xml   %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/EmailMain.vxml.xml               %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/EmailMain_Template.vxml.xml      %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/SBRoot.vxml.xml                  %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/ABNFBoolean.gram                 %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/ABNFBoolean_es-MX.gram           %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/ABNFDate.gram                    %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/ABNFDate_es-MX.gram              %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/ABNFDigits.gram                  %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/ABNFDigits_es-MX.gram            %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/sbpostinstall.sh                        %{SWDIR} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/*.cfg                                   %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/ProxySrv.config                         %{CONFIG} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/profile                                 /etc >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/httpd.conf                              %{HTTPDCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/mod_mono.conf                           %{HTTPDCONFD} >> %{INSTLOG} 2>> %{INSTLOG}
#mf -f %{BAK}/%{name}-%{version}.%{release}/config/pg_hba.conf                             /var/lib/pgsql/data >> %{INSTLOG} 2>> %{INSTLOG}


/bin/echo "Removing leftover files..." >> %{INSTLOG}

rm -rf %{SBCONF}/assets/docs/SpeechBridgeUserGuide.pdf                              >> %{INSTLOG} 2>> %{INSTLOG}
rm -rf %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side1.pdf                   >> %{INSTLOG} 2>> %{INSTLOG}
rm -rf %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side2.pdf                   >> %{INSTLOG} 2>> %{INSTLOG}
rm -rf %{SBCONF}/assets/docs/SpeechBridgeAdminGuide.pdf                             >> %{INSTLOG} 2>> %{INSTLOG}
rm -rf %{SBCONF}/assets/docs/SpeechBridgeAdminGuide_4-2.pdf                         >> %{INSTLOG} 2>> %{INSTLOG}

rm -rf %{SBBASE}/config/SBUpdate_%{BUILDVERDASH}_pgsql.sql                          >> %{INSTLOG} 2>> %{INSTLOG}
#rm -rf %{SBBASE}/config/SBUpdate_DD_%{BUILDVERDASH}_pgsql.sql                       >> %{INSTLOG} 2>> %{INSTLOG}
rm -rf %{BAK}/%{name}-%{version}.%{release}                                         >> %{INSTLOG} 2>> %{INSTLOG}

#/bin/echo "Rolling back database changes..." >> %{INSTLOG}


/bin/echo "Restarting components..." >> %{INSTLOG}

/sbin/service speechbridged restart >> %{INSTLOG} 2>> %{INSTLOG}
/sbin/service httpd reload >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Rollback of %{name}-%{version}-%{release} complete." >> %{INSTLOG}

#####################################################################
%files
#####################################################################
%defattr(-,speechbridge,speechbridge,-)
#%attr(0440,speechbridge,speechbridge) %{IBIN}/libPocoFoundation.so.11
#%attr(0550,speechbridge,speechbridge) %{IBIN}/AudioRtr_C5_20110407
#%attr(0550,speechbridge,speechbridge) %{IBIN}/AudioRtr_C5_20111115
#%attr(0550,speechbridge,speechbridge) %{IBIN}/ProxySrv_optnoshared_centos44
#%attr(0550,speechbridge,speechbridge) %{IBIN}/SBLauncher
#%attr(0550,speechbridge,speechbridge) %{IBIN}/CepstralCmd
#%attr(0550,speechbridge,speechbridge) %{IBIN}/NeospeechCmd
#%attr(0550,speechbridge,speechbridge) %{IBIN}/sbfailover
#%attr(0550,speechbridge,speechbridge) %{IBIN}/speechbridged
#%attr(0550,speechbridge,speechbridge) %{IBIN}/neospeechd
#%attr(0550,speechbridge,speechbridge) %{IBIN}/sbreportgen.cron
%attr(0550,speechbridge,speechbridge) %{IBIN}/sbbackup.cron
#%attr(0550,speechbridge,speechbridge) %{IBIN}/sbfailover.cron
#%attr(0550,speechbridge,speechbridge) %{IBIN}/sbrmoldlogs.cron
#%attr(0550,speechbridge,speechbridge) %{IBIN}/speechbridgemon.cron
#%attr(0550,speechbridge,speechbridge) %{IBIN}/sbfailover.sh
#%attr(0550,speechbridge,speechbridge) %{IBIN}/sbreportyestsend.sh
#%attr(0550,speechbridge,speechbridge) %{IBIN}/AudioMgr.sh
#%attr(0550,speechbridge,speechbridge) %{IBIN}/DialogMgr.sh
#%attr(0550,speechbridge,speechbridge) %{IBIN}/SBSched.sh
#%attr(0550,speechbridge,speechbridge) %{IBIN}/exportdialog.sh
#%attr(0550,speechbridge,speechbridge) %{IBIN}/importdialog.sh
%attr(0440,speechbridge,speechbridge) %{IBIN}/AudioMgr.exe
%attr(0440,speechbridge,speechbridge) %{IBIN}/asrtest.exe
%attr(0440,speechbridge,speechbridge) %{IBIN}/DialogMgr.exe
%attr(0440,speechbridge,speechbridge) %{IBIN}/MigrationTool_DIDPrompt.exe
%attr(0440,speechbridge,speechbridge) %{IBIN}/sbdbutils.exe
%attr(0440,speechbridge,speechbridge) %{IBIN}/SBLocalRM.exe
%attr(0440,speechbridge,speechbridge) %{IBIN}/SBSched.exe
%attr(0440,speechbridge,speechbridge) %{IBIN}/sbtest.exe
%attr(0440,speechbridge,speechbridge) %{IBIN}/AppGenerator.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/AsrFacadeLumenvox.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/AsrFacadeNull.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/ISMessaging.dll
#%attr(0440,speechbridge,speechbridge) %{IBIN}/nspring.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/PromptSelector.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/SBConfig.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/SBConfigStor.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/SBEmail.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/SBLdapConn.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/SBResourceMgr.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/SBTTS.dll
#%attr(0440,speechbridge,speechbridge) %{IBIN}/SBUnitTests.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/SimpleAES.dll
%attr(0440,speechbridge,speechbridge) %{IBIN}/XmlScriptParser.dll
#%attr(0440,speechbridge,speechbridge) %{IBIN}/AudioMgr.exe.config
#%attr(0440,speechbridge,speechbridge) %{IBIN}/DialogMgr.exe.config
#%attr(0440,speechbridge,speechbridge) %{IBIN}/sbdbutils.exe.config
#%attr(0440,speechbridge,speechbridge) %{IBIN}/SBLocalRM.exe.config
#%attr(0440,speechbridge,speechbridge) %{IBIN}/SBSched.exe.config
#%attr(0440,speechbridge,speechbridge) %{IBIN}/AsrFacadeLumenvox.dll.config
#%attr(0440,speechbridge,speechbridge) %{IBIN}/oss-application.conf.xml
#%attr(0440,speechbridge,speechbridge) %{ICONFIG}/0.cfg
#%attr(0440,speechbridge,speechbridge) %{ICONFIG}/1.cfg
#%attr(0440,speechbridge,speechbridge) %{ICONFIG}/ProxySrv.config
#%attr(0440,speechbridge,speechbridge) %{ICONFIG}/SBPreCreate_pgsql.sql
%attr(0440,speechbridge,speechbridge) %{ICONFIG}/SBCreate_pgsql.sql
%attr(0440,speechbridge,speechbridge) %{ICONFIG}/SBUpdate_%{BUILDVERDASH}_pgsql.sql
%attr(0440,speechbridge,speechbridge) %{ICONFIG}/SBUpdate_DD_pgsql.sql
#%attr(0440,speechbridge,speechbridge) %{ICONFIG}/SBUpdate_DD_%{BUILDVERDASH}_pgsql.sql
#%attr(0440,speechbridge,speechbridge) %{ICONFIG}/English_pgsql.sql
#%attr(0440,speechbridge,speechbridge) %{ICONFIG}/Spanish_pgsql.sql
#%attr(0550,speechbridge,speechbridge) %{ICONFIG}/neoinstall.sh
#%attr(0550,speechbridge,speechbridge) %{ICONFIG}/sb411upprotts.sh
#%attr(0550,speechbridge,speechbridge) %{ICONFIG}/setbranding.sh
#%attr(0550,speechbridge,speechbridge) %{ICONFIG}/sbddinstall.sh
#%attr(0550,speechbridge,speechbridge) %{ICONFIG}/_etc_profile
#%attr(0550,speechbridge,speechbridge) %{ICONFIG}/httpd.conf
#%attr(0644,speechbridge,speechbridge) %{ICONFIG}/mod_mono.conf
#%attr(0440,speechbridge,speechbridge) %{ICONFIG}/pg_hba.conf
#%attr(0440,speechbridge,speechbridge) %{ICONFIG}/cluster.notification
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/index.html
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/index-rootdir.html
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/AADirectory.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/Default.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/EulaAccept.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/GenHash.aspx
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/GreetingPrompts.aspx
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/GreetingPromptsForDid.aspx
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/Help.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/Login.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/Login_AlcatelLucent.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/Login_AV.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/Login_Incendonet.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/Login_NEC.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/LoginIncorrect.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/Logout.aspx
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/MenuEditor.aspx
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/MenuEditor.aspx_
#%attr(0400,speechbridge,speechbridge) %{ISBCONF}/MenuManager.aspx
#%attr(0400,speechbridge,speechbridge) %{ISBCONF}/MenuManager.aspx_
%attr(0400,speechbridge,speechbridge) %{ISBCONF}/ServerSettings.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/ServerSettingsLdap.aspx
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/UserEmailProps.aspx
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/Web.config
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/style.css
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/DD_style.css
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/style_AlcatelLucent.css
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/style_ActiveVoice.css
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/style_AV.css
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/style_Incendonet.css
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/style_NEC.css
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/content/BizHours.xml
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/content/AlcatelLucent_EULA.txt
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/content/AV_EULA.txt
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/content/EULA.txt
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/content/Incendonet_EULA.txt
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/content/NEC_EULA.txt
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/docs/SpeechBridgeUserGuide_4-0.pdf
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side1_4-0.pdf
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side2_4-0.pdf
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/docs/SpeechBridgeAdminGuide_4-0.pdf
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/docs/SpeechBridgeAdminGuide_4-1.pdf
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/docs/SpeechBridgeAdminGuide_4-2.pdf
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/favicon.ico
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/images/LogoImage.png
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/images/LogoImage_AlcatelLucent.png
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/images/LogoImage_AV.png
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/images/LogoImage_Incendonet.png
#%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/images/LogoImage_NEC.png
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/js/AADirectory.js
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/js/CheckboxSupport.js
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/js/MenuEditor.js
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/js/ServerSettings.js
%attr(0440,speechbridge,speechbridge) %{ISBCONF}/assets/js/ServerSettingsLdap.js
#%attr(0440,speechbridge,speechbridge) %{IVDS}/AAMain.vxml.xml
%attr(0440,speechbridge,speechbridge) %{IVDS}/AAMain_Template_4-2-1.vxml.xml
#%attr(0440,speechbridge,speechbridge) %{IVDS}/CalendarMain.vxml.xml
#%attr(0440,speechbridge,speechbridge) %{IVDS}/CalendarMain_Template.vxml.xml
#%attr(0440,speechbridge,speechbridge) %{IVDS}/EmailMain.vxml.xml
#%attr(0440,speechbridge,speechbridge) %{IVDS}/EmailMain_Template.vxml.xml
#%attr(0440,speechbridge,speechbridge) %{IVDS}/SBRoot.vxml.xml
#%attr(0440,speechbridge,speechbridge) %{IVDS}/ABNFBoolean.gram
#%attr(0440,speechbridge,speechbridge) %{IVDS}/ABNFBoolean_es-MX.gram
#%attr(0440,speechbridge,speechbridge) %{IVDS}/ABNFDate.gram
#%attr(0440,speechbridge,speechbridge) %{IVDS}/ABNFDate_es-MX.gram
#%attr(0440,speechbridge,speechbridge) %{IVDS}/ABNFDigits.gram
#%attr(0440,speechbridge,speechbridge) %{IVDS}/ABNFDigits_es-MX.gram
#%attr(0440,speechbridge,speechbridge) %{IVDS}/prompts_20120323.tar.gz
#%attr(0440,speechbridge,speechbridge) %{IVDS}/Prompts/empty.wav
#%attr(0440,speechbridge,speechbridge) %{IVDS}/Prompts/IntermissionShort.wav
#%attr(0440,speechbridge,speechbridge) %{IVDS}/Prompts/MainMenu.wav
#%attr(0440,speechbridge,speechbridge) %{IVDS}/Prompts/ThankYouForCalling.wav
%attr(0440,speechbridge,speechbridge) %{IVDS}/Prompts/ThankYouForCallingPleaseSayTheName.wav
#%attr(0440,speechbridge,speechbridge) %{IVDS}/Prompts/Silence-7sec.wav
#%attr(0550,speechbridge,speechbridge) %{ISWDIR}/sbpostinstall.sh

#####################################################################
%changelog
#####################################################################
* Wed Dec  6 2012  Support <support@incendonet.com>
- Updated to upgrade SpeechBridge 4.1.1 to 4.2.1 components
* Wed Aug  8 2012  Support <support@incendonet.com>
- Updated to upgrade SpeechBridge 4.0.1 to 4.1.1 components
* Thu Jul  2 2012  Support <support@incendonet.com>
- Updated to upgrade SpeechBridge 3.1.1 to 4.0.1 components
* Thu Mar 15 2012  Support <support@incendonet.com>
- Updated to install SpeechBridge 4.0.1 components
* Mon Jun 14 2010  Support <support@incendonet.com>
- Updated for SpeechBridge 3.0.1
* Mon Aug 18 2008  Support <support@incendonet.com>
- Included updated speakd
* Mon Jun 30 2008  Support <support@incendonet.com>
- Modified for SPEAK 3.0.1
* Thu Jan 31 2008  Support <support@incendonet.com>
- Updated for 1.4.0.32
* Fri Jan 11 2008  Support <support@incendonet.com>
- Updated for 1.3.376.1
* Tue Sep 18 2007  Support <support@incendonet.com>
- Updated for 1.3.259.1
* Fri Feb 10 2006  Support <support@incendonet.com>
- Updated for 0.8.1.22.3.
* Fri Jan 27 2006  Support <support@incendonet.com>
- Created initial spec file
