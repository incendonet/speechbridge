Summary: SpeechBridge 3.1.1 Update
Name: sbupdate
Version: 3.1.1
Release: 178
License: Incendonet
Group: Applications/Communications
Source: /opt/speechbridge/update/sbupdate_3-1-1-0.tar.gz

URL: http://www.incendonet.com/

# Build with:  rpmbuild -bb sbupdate_3-1-1-0.spec
#BuildRoot: %{_tmppath}/%{name}-%{version}-%{release}-buildroot
BuildRoot: /home/speechbridge/src/rpmbuild/%{name}-%{version}-%{release}-buildroot
BuildArch: i686

%define SBBASE     /opt/speechbridge
%define BAK        /opt/speechbridge/backup
%define BIN        /opt/speechbridge/bin
%define CONFIG     /opt/speechbridge/config
%define SBLOGS     /opt/speechbridge/logs
%define SBCONF     /opt/speechbridge/SBConfig
%define VDS        /opt/speechbridge/VoiceDocStore
%define INSTLOG    /opt/speechbridge/logs/InstallLog_%{name}-%{version}-%{release}.txt
%define SWDIR      /home/speechbridge/software
%define NEWMONODIR /opt/novell/mono/bin
%define HTTPDCONF  /etc/httpd/conf
%define HTTPDCONFD /etc/httpd/conf.d

%description
This package will update your installation to SpeechBridge 3.1.1

//Requires: apr, apr-devel, apr-util, apr-util-devel, cairo, cairo-devel, fontconfig-devel, freetype, freetype-devel, giflib, giflib-devel, httpd, httpd-devel, httpd-manual, libexif, libexif-devel, libjpeg-devel, libpng, libpng-devel, libtiff, libtiff-devel, libX11, libX11-devel, libXau-devel, libXdmcp-devel, libXext-devel, libXft-devel, libXrender-devel, mesa-libGL, mesa-libGL-devel, mod_ssl, pango, pango-devel, xorg-x11-proto-devel
Requires: sbupdate >= 3.0.1, mod_mono-addon >= 2.6.3, mono-addon-core >= 2.6.7, mono-addon-data-2.6.7-6.1.i386, mono-addon-data-postgresql >= 2.6.7, mono-addon-data-sqlite >=2.6.7, mono-addon-extras >= 2.6.7, mono-addon-libgdiplus0 >= 2.6.7, mono-addon-wcf >= , .6.7, mono-addon-web >= 2.6.7, mono-addon-winforms >= 2.6.7, mono-addon-xsp >= 2.6.5
Obsoletes: sbupdate-2.0.2-42_09042, sbupdate-3.0.1-301

%prep

%build

%install
#rm -rf %{buildroot}

%clean
#rm -rf %{buildroot}

#####################################################################
%post
#####################################################################
/bin/echo "Starting install of %{name}-%{version}-%{release}." >> %{INSTLOG}

/sbin/service speechbridged stop >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Creating backup directories...." >> %{INSTLOG}

mkdir -p %{BAK}/%{name}-%{version}.%{release}/bin                        >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{BAK}/%{name}-%{version}.%{release}/config                     >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images     >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/docs       >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore              >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Backing up files..." >> %{INSTLOG}

#mv -f %{BIN}/ProxySrv_centos44_optnoshared       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/ProxySrv_optnoshared_centos44       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_C5_GenericSip              %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_C5_ShoretelSip             %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AudioRtr_C5_20101020                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/sbreportgen.cron                    %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/speechbridgemon.cron                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/CepstralCmd                         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/NeospeechCmd_L08                    %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/asrtest.exe                         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AudioMgr.exe                        %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/DialogMgr.exe                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/sbdbutils.exe                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBLocalRM.exe                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBSched.exe                         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AsrFacadeLumenvox.dll               %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/ISMessaging.dll                     %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/MonoTimers.dll                      %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBConfig.dll                        %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBConfigStor.dll                    %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBEmail.dll                         %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBLdapConn.dll                      %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBResourceMgr.dll                   %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBTTS.dll                           %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SimpleAES.dll                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/XmlScriptParser.dll                 %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/speechbridged                       %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{BIN}/AudioMgr.exe.config                 %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BIN}/DialogMgr.exe.config                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/SBLocalRM.exe.config                %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BIN}/SBSched.exe.config                  %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BIN}/AsrFacadeLumenvox.dll.config        %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{CONFIG}/SBCreate_pgsql.sql               %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{CONFIG}/sbupdate-collab0.sh              %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{CONFIG}/setbranding.sh                   %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Global.asax                      %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/AADirectory.aspx                 %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Default.aspx                     %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/EulaAccept.aspx                  %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/GenHash.aspx                     %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/GreetingPrompts.aspx             %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Help.aspx                       %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Login.aspx                       %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login_AlcatelLucent.aspx         %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login_AV.aspx                    %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Login_Incendonet.aspx            %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login_NEC.aspx                   %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/LoginIncorrect.aspx              %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Logout.aspx                      %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/ServerSettings.aspx              %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/ServerSettingsLdap.aspx          %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/UserEmailProps.aspx              %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{SBCONF}/Web.config                       %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style.css                 %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_ActiveVoice.css     %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_AlcatelLucent.css   %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_AV.css              %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_Incendonet.css      %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_NEC.css             %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{SBCONF}/assets/content/BizHours.xml      %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/EULA.txt          %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/AlcatelLucent_EULA.txt                     %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/AV_EULA.txt                                %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/Incendonet_EULA.txt                        %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/Incendonet_EULA_ABBR.txt                   %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage.png                               %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_AlcatelLucent.png                 %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_AV.png                            %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_Incendonet.png                    %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_NEC.png                           %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{VDS}/AAMain_Template.vxml.xml            %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{VDS}/CalendarMain_Template.vxml.xml      %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{VDS}/EmailMain_Template.vxml.xml         %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{VDS}/SBRoot.vxml.xml                     %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{VDS}/Prompts/IntermissionShort.wav       %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/Prompts >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{VDS}/Prompts/MainMenu.wav                %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/Prompts >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{VDS}/Prompts/ThankYouForCalling.wav      %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/Prompts >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SWDIR}/sbpostinstall.sh                  %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f /etc/profile                               %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#cp -f %{HTTPDCONF}/httpd.conf                    %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{HTTPDCONFD}/mod_mono.conf                %{BAK}/%{name}-%{version}.%{release}/config >> %{INSTLOG} 2>> %{INSTLOG}


/bin/echo "Moving new files..." >> %{INSTLOG}

#mkdir -p %{SBCONF}/assets/docs                                  >> %{INSTLOG} 2>> %{INSTLOG}

#mv -f %{BIN}/ProxySrv_optnoshared_centos44.up      %{BIN}/ProxySrv_optnoshared_centos44 >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AudioRtr_C5_20110407.up               %{BIN}/AudioRtr_C5_20110407 >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/sbbackup.cron.up                     %{BIN}/sbbackup.cron >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/sbreportgen.cron.up                   %{BIN}/sbreportgen.cron >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/speechbridgemon.cron.up               %{BIN}/speechbridgemon.cron >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/sbrmoldlogs.cron.up                  %{BIN}/sbrmoldlogs.cron >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioMgr.sh.up                        %{BIN}/AudioMgr.sh >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/DialogMgr.sh.up                       %{BIN}/DialogMgr.sh >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/SBSched.sh.up                         %{BIN}/SBSched.sh >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/CepstralCmd.up                        %{BIN}/CepstralCmd >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/NeospeechCmd_L08.up                   %{BIN}/NeospeechCmd_L08 >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AudioMgr.exe.up                       %{BIN}/AudioMgr.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/asrtest.exe.up                        %{BIN}/asrtest.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/DialogMgr.exe.up                      %{BIN}/DialogMgr.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/sbdbutils.exe.up                      %{BIN}/sbdbutils.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBLocalRM.exe.up                      %{BIN}/SBLocalRM.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBSched.exe.up                        %{BIN}/SBSched.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/sbtest.exe.up                         %{BIN}/sbtest.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AsrFacadeLumenvox.dll.up              %{BIN}/AsrFacadeLumenvox.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/ISMessaging.dll.up                    %{BIN}/ISMessaging.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBConfig.dll.up                       %{BIN}/SBConfig.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBConfigStor.dll.up                   %{BIN}/SBConfigStor.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBEmail.dll.up                        %{BIN}/SBEmail.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBLdapConn.dll.up                     %{BIN}/SBLdapConn.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBResourceMgr.dll.up                  %{BIN}/SBResourceMgr.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBTTS.dll.up                          %{BIN}/SBTTS.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SimpleAES.dll.up                      %{BIN}/SimpleAES.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/XmlScriptParser.dll.up                %{BIN}/XmlScriptParser.dll >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/speechbridged.up                      %{BIN}/speechbridged >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BIN}/AudioMgr.exe.config.up                %{BIN}/AudioMgr.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BIN}/DialogMgr.exe.config.up               %{BIN}/DialogMgr.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/SBLocalRM.exe.config.up               %{BIN}/SBLocalRM.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BIN}/SBSched.exe.config.up                 %{BIN}/SBSched.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BIN}/AsrFacadeLumenvox.dll.config.up       %{BIN}/AsrFacadeLumenvox.dll.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{CONFIG}/sbupdate-collab0.sh.up             %{CONFIG}/sbupdate-collab0.sh >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{CONFIG}/setbranding.sh.up                  %{CONFIG}/setbranding.sh >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{CONFIG}/SBCreate_pgsql.sql.up              %{CONFIG}/SBCreate_pgsql.sql >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{CONFIG}/SBUpdate_3-1-1_pgsql.sql.up        %{CONFIG}/SBUpdate_3-1-1_pgsql.sql >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{CONFIG}/sbupdate-db_3-1-1.sh.up            %{CONFIG}/sbupdate-db_3-1-1.sh >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Global.asax.up                     %{SBCONF}/Global.asax >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/AADirectory.aspx.up                %{SBCONF}/AADirectory.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Default.aspx.up                    %{SBCONF}/Default.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/EulaAccept.aspx.up                 %{SBCONF}/EulaAccept.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/GenHash.aspx.up                    %{SBCONF}/GenHash.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/GreetingPrompts.aspx.up            %{SBCONF}/GreetingPrompts.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Help.aspx.up                       %{SBCONF}/Help.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Login.aspx.up                      %{SBCONF}/Login.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login_AlcatelLucent.aspx.up        %{SBCONF}/Login_AlcatelLucent.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login_AV.aspx.up                   %{SBCONF}/Login_AV.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Login_Incendonet.aspx.up           %{SBCONF}/Login_Incendonet.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Login_NEC.aspx.up                  %{SBCONF}/Login_NEC.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/LoginIncorrect.aspx.up             %{SBCONF}/LoginIncorrect.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Logout.aspx.up                     %{SBCONF}/Logout.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/ServerSettings.aspx.up             %{SBCONF}/ServerSettings.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/ServerSettingsLdap.aspx.up         %{SBCONF}/ServerSettingsLdap.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/UserEmailProps.aspx.up             %{SBCONF}/UserEmailProps.aspx >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{SBCONF}/Web.config.up                      %{SBCONF}/Web.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style.css.up                %{SBCONF}/assets/style.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_ActiveVoice.css.up    %{SBCONF}/assets/style_ActiveVoice.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_AlcatelLucent.css.up  %{SBCONF}/assets/style_AlcatelLucent.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_AV.css.up             %{SBCONF}/assets/style_AV.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_Incendonet.css.up     %{SBCONF}/assets/style_Incendonet.css >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style_NEC.css.up            %{SBCONF}/assets/style_NEC.css >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/assets/favicon.ico.up              %{SBCONF}/assets/favicon.ico >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/BizHours.xml.up     %{SBCONF}/assets/content/BizHours.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/EULA.txt.up         %{SBCONF}/assets/content/EULA.txt >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/AlcatelLucent_EULA.txt.up                   %{SBCONF}/assets/content/AlcatelLucent_EULA.txt >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/AV_EULA.txt.up                              %{SBCONF}/assets/content/AV_EULA.txt >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/Incendonet_EULA.txt.up                      %{SBCONF}/assets/content/Incendonet_EULA.txt >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/content/NEC_EULA.txt.up                             %{SBCONF}/assets/content/NEC_EULA.txt >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/docs/SpeechBridgeUserGuide_3-0.pdf.up               %{SBCONF}/assets/docs/SpeechBridgeUserGuide_3-0.pdf >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side1_3-0.pdf.up          %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side1_3-0.pdf >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side2_3-0.pdf.up          %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side2_3-0.pdf >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/docs/SpeechBridgeAdminGuide_3-0.pdf.up              %{SBCONF}/assets/docs/SpeechBridgeAdminGuide_3-0.pdf >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage.png.up                             %{SBCONF}/assets/images/LogoImage.png >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_AlcatelLucent.png.up               %{SBCONF}/assets/images/LogoImage_AlcatelLucent.png >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_AV.png.up                          %{SBCONF}/assets/images/LogoImage_AV.png >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_Incendonet.png.up                  %{SBCONF}/assets/images/LogoImage_Incendonet.png >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/images/LogoImage_NEC.png.up                         %{SBCONF}/assets/images/LogoImage_NEC.png >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/AAMain.vxml.xml.up                    %{VDS}/AAMain.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/AAMain_Template.vxml.xml.up           %{VDS}/AAMain_Template.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/CalendarMain.vxml.xml.up              %{VDS}/CalendarMain.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/CalendarMain_Template.vxml.xml.up     %{VDS}/CalendarMain_Template.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/EmailMain.vxml.xml.up                 %{VDS}/EmailMain.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/EmailMain_Template.vxml.xml.up        %{VDS}/EmailMain_Template.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/SBRoot.vxml.xml.up                    %{VDS}/SBRoot.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/Prompts/empty.wav.up                  %{VDS}/Prompts/empty.wav >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/Prompts/IntermissionShort.wav.up      %{VDS}/Prompts/IntermissionShort.wav >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/Prompts/MainMenu.wav.up               %{VDS}/Prompts/MainMenu.wav >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/Prompts/ThankYouForCalling.wav.up     %{VDS}/Prompts/ThankYouForCalling.wav >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SWDIR}/sbpostinstall.sh.up                 %{SWDIR}/sbpostinstall.sh >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f /etc/profile.up                              /etc/profile >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{HTTPDCONFD}/mod_mono.conf.up               %{HTTPDCONFD}/mod_mono.conf >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Creating symbolic links..." >> %{INSTLOG}

ln -fs %{BIN}/AudioRtr_C5_20110407 %{BIN}/AudioRtr >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/speechbridgemon.cron /etc/cron.hourly/speechbridgemon.cron >> %{INSTLOG} 2>> %{INSTLOG}
ln -fs %{BIN}/sbbackup.cron /etc/cron.daily/sbbackup.cron >> %{INSTLOG} 2>> %{INSTLOG}
ln -fs %{BIN}/sbrmoldlogs.cron /etc/cron.daily/sbrmoldlogs.cron >> %{INSTLOG} 2>> %{INSTLOG}

# Remove files that don't belong in the install
#rm -rf %{VDS}/AAMain_Template_WithEmailCal.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#rm -rf %{VDS}/AAMain_Template_WithoutEmailCal.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Setting permissions..." >> %{INSTLOG}
# Note this is done to make sure that ALL files have the proper permissions.  The 'Files' section
# at the bottom only sets permissions on the files in the RPM.
#chmod ugo=rx   %{SBBASE}                                                   >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rx   %{SBBASE}/bin                                               >> %{INSTLOG} 2>> %{INSTLOG}
chmod  ug=r    %{SBBASE}/bin/*.exe                                         >> %{INSTLOG} 2>> %{INSTLOG}
chmod  ug=rw   %{SBBASE}/bin/*.dll                                         >> %{INSTLOG} 2>> %{INSTLOG}
chmod   o=r    %{SBBASE}/bin/*.dll                                         >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rwx  %{SBBASE}/config                                            >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/config/*.cfg                                      >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/config/ProxySrv.config                            >> %{INSTLOG} 2>> %{INSTLOG}
#chmod -R ugo=r %{SBBASE}/SBConfig                                          >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rx   %{SBBASE}/SBConfig                                          >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rx   %{SBBASE}/SBConfig/assets                                   >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rx   %{SBBASE}/SBConfig/assets/content                           >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rx   %{SBBASE}/SBConfig/assets/docs                              >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rx   %{SBBASE}/SBConfig/assets/images                            >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rx   %{SBBASE}/SBConfig/bin                                      >> %{INSTLOG} 2>> %{INSTLOG}
#chmod  ug=rx   %{SBLOGS}                                                   >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rx   %{SBBASE}/VoiceDocStore                                     >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=r    %{SBBASE}/VoiceDocStore/*_Template.vxml.xml                 >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/SBRoot.vxml.xml                     >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/AAMain.vxml.xml                     >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/EmailMain.vxml.xml                  >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/CalendarMain.vxml.xml               >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rwx  %{SBBASE}/VoiceDocStore/Prompts                             >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/Prompts/AHGreeting2.wav             >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/Prompts/PleaseSayTheName.wav        >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rw   %{SBBASE}/VoiceDocStore/Prompts/ThankYouForCalling.wav      >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo=rwx  %{SBBASE}/VoiceDocStore/Prompts/Names                       >> %{INSTLOG} 2>> %{INSTLOG}
chown -R speechbridge:speechbridge %{SBBASE}                               >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Updating DB..." >> %{INSTLOG}
/bin/sh < %{CONFIG}/sbupdate-db_3-1-1.sh >> %{INSTLOG} 2>> %{INSTLOG}

#/bin/echo Setting collaboration server in DB... >> %{INSTLOG} 2>> %{INSTLOG}

##%define sCfgEmail  %(grep EmailServer /opt/speechbridge/bin/DialogMgr.exe.config | cut -d\" -f4) >> %{INSTLOG} 2>> %{INSTLOG}
##%define bIs03      %(if [ "$sCfgEmail" = "Exchange2003" ]; then echo 1; else echo 0; fi)
##%define bIs07      %(if [ "$sCfgEmail" = "Exchange2007" ]; then echo 1; else echo 0; fi)
##%define sDbEmail  "[none]"
##%if bIs03
##    %define sDbEmail  "Microsoft Exchange 2003"
##%endif
##%if bIs07
##    %define sDbEmail  "Microsoft Exchange 2007 - Normal"
##%endif
##/bin/echo "UPDATE tblConfigParams SET sValue = '%{sDbEmail}' WHERE sComponent='Collaboration:0000';" > /home/speechbridge/collab0.sql 2>> %{INSTLOG}

#/bin/sh < %{CONFIG}/sbupdate-collab0.sh >> %{INSTLOG} 2>> %{INSTLOG}
#%{NEWMONODIR}/mono --config %{BIN}/sbdbutils.exe.config %{BIN}/sbdbutils.exe --run-script /home/speechbridge/collab0.sql >> %{INSTLOG} 2>> %{INSTLOG}
#rm -f /home/speechbridge/collab0.sql >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo Setting EDNoiseLevel in DB... >> %{INSTLOG} 2>> %{INSTLOG}

%define sEDNoiseLevel	%(grep EDNoiseLevel /opt/speechbridge/bin/AudioMgr.exe.config | cut -d\" -f4) >> %{INSTLOG} 2>> %{INSTLOG}
/bin/echo "UPDATE tblConfigParams SET sValue = '%{sEDNoiseLevel}' WHERE sComponent='Apps:global:0001';" > /home/speechbridge/sbdbdynamicupdate0.sql 2>> %{INSTLOG}
/bin/echo "UPDATE tblConfigParams SET sValue = '%{sEDNoiseLevel}' WHERE sComponent='Apps:global:0002';" >> /home/speechbridge/sbdbdynamicupdate0.sql 2>> %{INSTLOG}

#/bin/echo "Installing report CRON job and Apache settings..." >> %{INSTLOG}
#mkdir %{SBBASE}/SBReports >> %{INSTLOG} 2>> %{INSTLOG}
#chown speechbridge:speechbridge %{SBBASE}/SBReports >> %{INSTLOG} 2>> %{INSTLOG}
#chmod ugo+rx %{SBBASE}/SBReports >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{SBBASE}/SBReports /var/www/html/SBReports
##cp %{HTTPDCONF}/httpd.conf %{HTTPDCONF}/httpd.conf.sb_3-0-1.bak		# Copied above
#/bin/echo "" >> %{HTTPDCONF}/httpd.conf
#/bin/echo "" >> %{HTTPDCONF}/httpd.conf
#/bin/echo "#SpeechBridge reports" >> %{HTTPDCONF}/httpd.conf
#/bin/echo "<Directory \"/var/www/html/SBReports\">" >> %{HTTPDCONF}/httpd.conf
#/bin/echo "        AllowOverride None" >> %{HTTPDCONF}/httpd.conf
#/bin/echo "        Order deny,allow" >> %{HTTPDCONF}/httpd.conf
#/bin/echo "#        deny from all" >> %{HTTPDCONF}/httpd.conf
#/bin/echo "        Allow from all" >> %{HTTPDCONF}/httpd.conf
#/bin/echo "</Directory>" >> %{HTTPDCONF}/httpd.conf
#/bin/echo "" >> %{HTTPDCONF}/httpd.conf
#/bin/echo "" >> %{HTTPDCONF}/httpd.conf
#ln -fs %{BIN}/sbreportgen.cron /etc/cron.daily/sbreportgen.cron >> %{INSTLOG} 2>> %{INSTLOG}

#/bin/echo "Updating Web.config..." >> %{INSTLOG}
#/bin/sed 's:<location path="UserEmailProps.aspx">:<location path="Help.aspx">\
#\t\t<system.web>\
#\t\t\t<authorization>\
#\t\t\t\t<allow users="admin"/>\
#\t\t\t\t<deny users="*"/>\
#\t\t\t</authorization>\
#\t\t</system.web>\
#\t</location>\
#\t<location path="UserEmailProps.aspx">:' < %{SBCONF}/Web.config > %{SBCONF}/Web2.config 2>> %{INSTLOG}
#mv -f %{SBCONF}/Web2.config %{SBCONF}/Web.config >> %{INSTLOG}  2>> %{INSTLOG}

#/bin/echo "Updating AudioMgr.exe.config..." >> %{INSTLOG}
#/bin/sed 's:<add key="AsrStartupSleepMsec" value="15000"/>:<add key="AsrStartupSleepMsec" value="0"/>:' < %{BIN}/AudioMgr.exe.config > %{BIN}/AudioMgr2.exe.config 2>> %{INSTLOG}
#mv -f %{BIN}/AudioMgr2.exe.config %{BIN}/AudioMgr.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#/bin/chown speechbridge:speechbridge %{BIN}/AudioMgr.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#/bin/chmod ug+rw %{BIN}/AudioMgr.exe.config >> %{INSTLOG} 2>> %{INSTLOG}



/bin/echo "Restarting services..." >> %{INSTLOG}

/sbin/service httpd reload >> %{INSTLOG} 2>> %{INSTLOG}
/sbin/service speechbridged restart >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "%{name}-%{version}-%{release}" >> %{SBBASE}/build.txt
/bin/echo "Install of %{name}-%{version}-%{release} complete." >> %{INSTLOG}

#####################################################################
%postun
#####################################################################
/bin/echo "Starting rollback %{name}-%{version}-%{release}." >> %{INSTLOG}

/sbin/service speechbridged stop >> %{INSTLOG} 2>> %{INSTLOG}
sleep 2

/bin/echo "Restoring backed up files..." >> %{INSTLOG}

#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/ProxySrv*                                  %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioRtr*                                  %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbreportgen.cron                           %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/speechbridgemon.cron                       %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbrmoldlogs.cron                          %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/CepstralCmd                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/NeospeechCmd_L08                               %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioMgr.exe                               %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/asrtest.exe                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/DialogMgr.exe                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbdbutils.exe                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLocalRM.exe                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBSched.exe                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbtest.exe                                 %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AsrFacadeLumenvox.dll                      %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/ISMessaging.dll                            %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/MonoTimers.dll                             %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBConfig.dll                               %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBConfigStor.dll                           %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBEmail.dll                                %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLdapConn.dll                             %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBResourceMgr.dll                          %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBTTS.dll                                  %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SimpleAES.dll                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/XmlScriptParser.dll                        %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/speechbridged                              %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioMgr.exe.config                        %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/bin/DialogMgr.exe.config                       %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLocalRM.exe.config                       %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBSched.exe.config                         %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AsrFacadeLumenvox.dll.config               %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/config/SBCreate_pgsql.sql                      %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/config/sbupdate-collab0.sh                     %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/setbranding.sh                          %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Global.asax                           %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/AADirectory.aspx                      %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Default.aspx                          %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/EulaAccept.aspx                       %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/GenHash.aspx                          %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/GreetingPrompts.aspx                  %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Help.aspx                             %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login.aspx                            %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login_AlcatelLucent.aspx              %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login_AV.aspx                         %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login_Incendonet.aspx                 %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login_NEC.aspx                        %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/LoginIncorrect.aspx                   %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Logout.aspx                           %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/ServerSettings.aspx                   %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/ServerSettingsLdap.aspx               %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/UserEmailProps.aspx                   %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Web.config                            %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style.css                                       %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style_ActiveVoice.css                           %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style_AlcatelLucent.css                         %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style_AV.css                                    %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style_Incendonet.css                            %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style_NEC.css                                   %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/BizHours.txt                            %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/EULA.txt                                %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/AlcatelLucent_EULA.txt                  %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/AV_EULA.txt                             %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/Incendonet_EULA.txt                     %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/Incendonet_EULA_ABBR.txt                %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/content/NEC_EULA.txt                            %{SBCONF}/assets/content >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage.png                            %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage_AlcatelLucent.png              %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage_AV.png                         %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage_Incendonet.png                 %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage_NEC.png                        %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/AAMain.vxml.xml                  %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/AAMain_Template.vxml.xml         %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/CalendarMain.vxml.xml            %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/CalendarMain_Template.vxml.xml   %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/EmailMain.vxml.xml               %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/EmailMain_Template.vxml.xml      %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
##mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/SBRoot.vxml.xml                  %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/sbpostinstall.sh                         %{SWDIR} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/profile                                 /etc >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/httpd.conf                              %{HTTPDCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/config/mod_mono.conf                           %{HTTPDCONFD} >> %{INSTLOG} 2>> %{INSTLOG}

#ln -fs %{BIN}/ProxySrv_optnoshared_centos44 %{BIN}/ProxySrv                         >> %{INSTLOG} 2>> %{INSTLOG}
ln -fs %{BIN}/AudioRtr_C5_20101020 %{BIN}/AudioRtr                               >> %{INSTLOG} 2>> %{INSTLOG}

#/bin/echo "Removing leftover files..." >> %{INSTLOG}

#rm -rf %{SBCONF}/assets/docs/SpeechBridgeUserGuide_3-0.pdf                          >> %{INSTLOG} 2>> %{INSTLOG}
#rm -rf %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side1_3-0.pdf                     >> %{INSTLOG} 2>> %{INSTLOG}
#rm -rf %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side2_3-0.pdf                     >> %{INSTLOG} 2>> %{INSTLOG}
#rm -rf %{SBCONF}/assets/docs/SpeechBridgeAdminGuide_3-0.pdf                         >> %{INSTLOG} 2>> %{INSTLOG}

rm -rf %{BAK}/%{name}-%{version}.%{release}                                         >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Restarting components..." >> %{INSTLOG}

/sbin/service speechbridged restart >> %{INSTLOG} 2>> %{INSTLOG}
/sbin/service httpd reload >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Rollback of %{name}-%{version}-%{release} complete." >> %{INSTLOG}

#####################################################################
%files
#####################################################################
%defattr(-,speechbridge,speechbridge,-)
%attr(0550,speechbridge,speechbridge) %{BIN}/AudioRtr_C5_20110407.up
#%attr(0550,speechbridge,speechbridge) %{BIN}/ProxySrv_optnoshared_centos44.up
#%attr(0550,speechbridge,speechbridge) %{BIN}/speechbridged.up
%attr(0550,speechbridge,speechbridge) %{BIN}/sbbackup.cron.up
#%attr(0550,root,root)                 %{BIN}/sbreportgen.cron.up
#%attr(0550,speechbridge,speechbridge) %{BIN}/speechbridgemon.cron.up
%attr(0550,speechbridge,speechbridge) %{BIN}/sbrmoldlogs.cron.up
#%attr(0550,speechbridge,speechbridge) %{BIN}/AudioMgr.sh.up
#%attr(0550,speechbridge,speechbridge) %{BIN}/DialogMgr.sh.up
#%attr(0550,speechbridge,speechbridge) %{BIN}/SBSched.sh.up
%attr(0550,speechbridge,speechbridge) %{BIN}/CepstralCmd.up
#%attr(0550,speechbridge,speechbridge) %{BIN}/NeospeechCmd_L08.up
%attr(0440,speechbridge,speechbridge) %{BIN}/AudioMgr.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/asrtest.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/DialogMgr.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/sbdbutils.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBLocalRM.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBSched.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/sbtest.exe.up
#%attr(0440,speechbridge,speechbridge) %{BIN}/SBLocalRM.exe.config.up
%attr(0440,speechbridge,speechbridge) %{BIN}/AsrFacadeLumenvox.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/ISMessaging.dll.up
#%attr(0440,speechbridge,speechbridge) %{BIN}/NSpring.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBConfig.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBConfigStor.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBEmail.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBLdapConn.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBResourceMgr.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBTTS.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SimpleAES.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/XmlScriptParser.dll.up
%attr(0440,speechbridge,speechbridge) %{CONFIG}/SBCreate_pgsql.sql.up
%attr(0440,speechbridge,speechbridge) %{CONFIG}/SBUpdate_3-1-1_pgsql.sql.up
%attr(0550,speechbridge,speechbridge) %{CONFIG}/sbupdate-db_3-1-1.sh.up
#%attr(0550,speechbridge,speechbridge) %{CONFIG}/sbupdate-collab0.sh.up
#%attr(0550,speechbridge,speechbridge) %{CONFIG}/setbranding.sh.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/Global.asax.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/AADirectory.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Default.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/EulaAccept.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/GenHash.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/GreetingPrompts.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Help.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Login.aspx.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/Login_AlcatelLucent.aspx.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/Login_AV.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Login_Incendonet.aspx.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/Login_NEC.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/LoginIncorrect.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Logout.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/ServerSettings.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/ServerSettingsLdap.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/UserEmailProps.aspx.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/style.css.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/style_AlcatelLucent.css.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/style_ActiveVoice.css.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/style_AV.css.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/style_Incendonet.css.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/style_NEC.css.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/favicon.ico.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/content/BizHours.xml.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/content/AlcatelLucent_EULA.txt.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/content/AV_EULA.txt.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/content/EULA.txt.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/content/Incendonet_EULA.txt.up
##%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/content/Incendonet_EULA_ABBR.txt.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/content/NEC_EULA.txt.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/docs/SpeechBridgeUserGuide_3-0.pdf.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side1_3-0.pdf.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/docs/SpeechBridgeUserCheatSheet-Side2_3-0.pdf.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/docs/SpeechBridgeAdminGuide_3-0.pdf.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/images/LogoImage.png.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/images/LogoImage_AlcatelLucent.png.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/images/LogoImage_AV.png.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/images/LogoImage_Incendonet.png.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/images/LogoImage_NEC.png.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/AAMain_Template.vxml.xml.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/CalendarMain_Template.vxml.xml.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/EmailMain_Template.vxml.xml.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/SBRoot.vxml.xml.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/Prompts/empty.wav.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/Prompts/IntermissionShort.wav.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/Prompts/MainMenu.wav.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/Prompts/ThankYouForCalling.wav.up
#%attr(0550,speechbridge,speechbridge) %{SWDIR}/sbpostinstall.sh.up
#%attr(0644,root,root)                 /etc/profile.up
#%attr(0644,root,root)                 %{HTTPDCONFD}/mod_mono.conf.up

#####################################################################
%changelog
#####################################################################
* Tue May 24 2011  Support <support@incendonet.com>
- Updated for SpeechBridge 3.2.1
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
