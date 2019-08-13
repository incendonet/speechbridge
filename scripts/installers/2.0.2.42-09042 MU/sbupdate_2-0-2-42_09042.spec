Summary: SpeechBridge 2.0.2 Update
Name: sbupdate
Version: 2.0.2
Release: 42_09042
License: Incendonet
Group: Applications/Communications
Source: /opt/speechbridge/update/sbupdate_2-0-2-42_09042.tar.gz

URL: http://www.incendonet.com/

#BuildRoot: %{_tmppath}/%{name}-%{version}-%{release}-buildroot
BuildRoot: /home/speechbridge/rpmbuild/%{name}-%{version}-%{release}-buildroot
BuildArch: i686

%define SBHOME     /opt/speechbridge
%define BAK        /opt/speechbridge/backup
%define BIN        /opt/speechbridge/bin
%define SBCONF     /opt/speechbridge/SBConfig
%define VDS        /opt/speechbridge/VoiceDocStore
%define INSTLOG    /opt/speechbridge/logs/InstallLog_%{name}-%{version}-%{release}.txt
%define SWDIR      /home/speechbridge/software

%description
This package will update your installation to SpeechBridge 2.0.2.42-09042

%prep

%build

%install
#rm -rf %{buildroot}

%clean
#rm -rf %{buildroot}

#####################################################################
#####################################################################
%post
#####################################################################
#####################################################################
/bin/echo "Starting install of %{name}-%{version}-%{release}." >> %{INSTLOG}

/sbin/service speechbridged stop >> %{INSTLOG} 2>> %{INSTLOG}

#####################################################################
/bin/echo "Creating backup directories...." >> %{INSTLOG}

mkdir -p %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mkdir -p %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mkdir -p %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}

#####################################################################
/bin/echo "Backing up files..." >> %{INSTLOG}

cp %{SBHOME}/build.txt %{BAK}/%{name}-%{version}.%{release} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/ProxySrv_centos44_optnoshared %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/ProxySrv_optnoshared_centos44 %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_GenericSip_Centos4 %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_C5_GenericSip %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_C5_ShoretelSip %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/asrtest.exe %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AudioMgr.exe %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/DialogMgr.exe %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/sbdbutils.exe %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBLocalRM.exe %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBSched.exe %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AsrFacadeLumenvox.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/ISMessaging.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/MonoTimers.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBConfig.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBConfigStor.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBEmail.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBLdapConn.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBResourceMgr.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBTTS.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SimpleAES.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/XmlScriptParser.dll %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/speechbridged %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/speechbridgemon.cron %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f /etc/cron.hourly/refreshtpool.cron %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioMgr.exe.config %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/DialogMgr.exe.config %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/SBLocalRM.exe.config %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/SBSched.exe.config %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AsrFacadeLumenvox.dll.config %{BAK}/%{name}-%{version}.%{release}/bin >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Global.asax %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/AADirectory.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Default.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/EulaAccept.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/GenHash.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/GreetingPrompts.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Login.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/LoginIncorrect.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Logout.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/ServerSettings.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/ServerSettingsLdap.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/UserEmailProps.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Web.config %{BAK}/%{name}-%{version}.%{release}/SBConfig >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style.css %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/assets/images/LogoImage.png %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/assets/images/LogoImage_Incendonet.png %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/AAMain.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/AAMain_Template.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/CalendarMain.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/CalendarMain_Template.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/EmailMain.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/EmailMain_Template.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/SBRoot.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore >> %{INSTLOG} 2>> %{INSTLOG}

#####################################################################
/bin/echo "Moving new files..." >> %{INSTLOG}

/bin/echo "2.0.2.42-09042 MU" >> %{SBHOME}/build.txt
#mv -f %{BIN}/ProxySrv_optnoshared_centos44.up %{BIN}/ProxySrv_optnoshared_centos44 >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_C5_GenericSip.up %{BIN}/AudioRtr_C5_GenericSip >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioRtr_C5_ShoretelSip.up %{BIN}/AudioRtr_C5_ShoretelSip >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/asrtest.exe.up %{BIN}/asrtest.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AudioMgr.exe.up %{BIN}/AudioMgr.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/DialogMgr.exe.up %{BIN}/DialogMgr.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/sbdbutils.exe.up %{BIN}/sbdbutils.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBLocalRM.exe.up %{BIN}/SBLocalRM.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBSched.exe.up %{BIN}/SBSched.exe >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/AsrFacadeLumenvox.dll.up %{BIN}/AsrFacadeLumenvox.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/ISMessaging.dll.up %{BIN}/ISMessaging.dll >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/MonoTimers.dll.up %{BIN}/MonoTimers.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBConfig.dll.up %{BIN}/SBConfig.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBConfigStor.dll.up %{BIN}/SBConfigStor.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBEmail.dll.up %{BIN}/SBEmail.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBLdapConn.dll.up %{BIN}/SBLdapConn.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBResourceMgr.dll.up %{BIN}/SBResourceMgr.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SBTTS.dll.up %{BIN}/SBTTS.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/SimpleAES.dll.up %{BIN}/SimpleAES.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/XmlScriptParser.dll.up %{BIN}/XmlScriptParser.dll >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/speechbridged.up %{BIN}/speechbridged >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BIN}/speechbridgemon.cron.up %{BIN}/speechbridgemon.cron >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AudioMgr.exe.config.up %{BIN}/AudioMgr.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/DialogMgr.exe.config.up %{BIN}/DialogMgr.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/SBLocalRM.exe.config.up %{BIN}/SBLocalRM.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/SBSched.exe.config.up %{BIN}/SBSched.exe.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BIN}/AsrFacadeLumenvox.dll.config.up %{BIN}/AsrFacadeLumenvox.dll.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Global.asax.up %{SBCONF}/Global.asax >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/AADirectory.aspx.up %{SBCONF}/AADirectory.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Default.aspx.up %{SBCONF}/Default.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/EulaAccept.aspx.up %{SBCONF}/EulaAccept.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/GenHash.aspx.up %{SBCONF}/GenHash.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/GreetingPrompts.aspx.up %{SBCONF}/GreetingPrompts.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/Login.aspx.up %{SBCONF}/Login.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/LoginIncorrect.aspx.up %{SBCONF}/LoginIncorrect.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Logout.aspx.up %{SBCONF}/Logout.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/ServerSettings.aspx.up %{SBCONF}/ServerSettings.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/ServerSettingsLdap.aspx.up %{SBCONF}/ServerSettingsLdap.aspx >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/UserEmailProps.aspx.up %{SBCONF}/UserEmailProps.aspx >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/Web.config.up %{SBCONF}/Web.config >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{SBCONF}/assets/style.css.up %{SBCONF}/assets/style.css >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/assets/images/LogoImage.png.up %{SBCONF}/assets/images/LogoImage.png >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{SBCONF}/assets/images/LogoImage_Incendonet.png.up %{SBCONF}/assets/images/LogoImage_Incendonet.png >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/AAMain.vxml.xml.up %{VDS}/AAMain.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/AAMain_Template.vxml.xml.up %{VDS}/AAMain_Template.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/CalendarMain.vxml.xml.up %{VDS}/CalendarMain.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/CalendarMain_Template.vxml.xml.up %{VDS}/CalendarMain_Template.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/EmailMain.vxml.xml.up %{VDS}/EmailMain.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/EmailMain_Template.vxml.xml.up %{VDS}/EmailMain_Template.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{VDS}/SBRoot.vxml.xml.up %{VDS}/SBRoot.vxml.xml >> %{INSTLOG} 2>> %{INSTLOG}

#####################################################################
#/bin/echo "Creating symbolic links..." >> %{INSTLOG}

rm -f /etc/init.d/speechbridged >> %{INSTLOG} 2>> %{INSTLOG}
ln -fs %{BIN}/speechbridged /etc/init.d/speechbridged >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/ProxySrv_optnoshared_centos44 %{BIN}/ProxySrv >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/AudioRtr_C5_GenericSip %{BIN}/AudioRtr >> %{INSTLOG} 2>> %{INSTLOG}
#ln -fs %{BIN}/speechbridgemon.cron /etc/cron.hourly/speechbridgemon.cron >> %{INSTLOG} 2>> %{INSTLOG}

#####################################################################
/bin/echo "Restarting components..." >> %{INSTLOG}

/sbin/service httpd reload >> %{INSTLOG} 2>> %{INSTLOG}
/sbin/service speechbridged start >> %{INSTLOG} 2>> %{INSTLOG}

#/bin/echo "Updating DB..." >> %{INSTLOG}
#/usr/bin/mono --config %{BIN}/sbdbutils.exe.config %{BIN}/sbdbutils.exe --run-script %{BIN}/UpdateSipReg.sql >> %{INSTLOG} 2>> %{INSTLOG}
#rm -f ${BIN}/UpdateSipReg.sql >> %{INSTLOG} 2>> %{INSTLOG}

#####################################################################
/bin/echo "Install of %{name}-%{version}-%{release} complete." >> %{INSTLOG}

#####################################################################
#####################################################################
%postun
#####################################################################
#####################################################################
/bin/echo "Starting rollback %{name}-%{version}-%{release}." >> %{INSTLOG}

/sbin/service speechbridged stop >> %{INSTLOG} 2>> %{INSTLOG}
sleep 2

/bin/echo "Moving backup files..." >> %{INSTLOG}

mv -f %{BAK}/%{name}-%{version}.%{release}/build.txt %{SBHOME} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/ProxySrv* %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioRtr* %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioMgr.exe %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/asrtest.exe %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/DialogMgr.exe %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/sbdbutils.exe %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLocalRM.exe %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBSched.exe %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AsrFacadeLumenvox.dll %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/ISMessaging.dll %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/MonoTimers.dll %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBConfig.dll %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBConfigStor.dll %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBEmail.dll %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBResourceMgr.dll %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBTTS.dll %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SimpleAES.dll %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/XmlScriptParser.dll %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/speechbridged %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/speechbridgemon.cron %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioMgr.exe.config %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/DialogMgr.exe.config %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLocalRM.exe.config %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBSched.exe.config %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AsrFacadeLumenvox.dll.config %{BIN} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Global.asax %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/AADirectory.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Default.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/EulaAccept.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/GenHash.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/GreetingPrompts.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/LoginIncorrect.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Logout.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/ServerSettings.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/ServerSettingsLdap.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/UserEmailProps.aspx %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Web.config %{SBCONF} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style.css %{SBCONF}/assets >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage.png %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/images/LogoImage_Incendonet.png %{SBCONF}/assets/images >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/AAMain.vxml.xml %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/AAMain_Template.vxml.xml %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/CalendarMain.vxml.xml %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/CalendarMain_Template.vxml.xml %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/EmailMain.vxml.xml %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/EmailMain_Template.vxml.xml %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/SBRoot.vxml.xml %{VDS} >> %{INSTLOG} 2>> %{INSTLOG}

#/bin/echo "Removing leftover files..." >> %{INSTLOG}

#rm -rf %{BAK}/%{name}-%{version}.%{release}/bin/UpdateSipReg.sql >> %{INSTLOG} 2>> %{INSTLOG}
rm -rf %{BAK}/%{name}-%{version}.%{release} >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Restarting components..." >> %{INSTLOG}

/sbin/service speechbridged start >> %{INSTLOG} 2>> %{INSTLOG}
/sbin/service httpd reload >> %{INSTLOG} 2>> %{INSTLOG}

/bin/echo "Rollback of %{name}-%{version}-%{release} complete." >> %{INSTLOG}

#####################################################################
#####################################################################
%files
#####################################################################
#####################################################################
%defattr(-,speechbridge,speechbridge,-)
#%attr(0550,speechbridge,speechbridge) %{BIN}/AudioRtr_C5_GenericSip.up
#%attr(0550,speechbridge,speechbridge) %{BIN}/AudioRtr_C5_ShoretelSip.up
%attr(0440,speechbridge,speechbridge) %{BIN}/AudioMgr.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/asrtest.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/DialogMgr.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/sbdbutils.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBLocalRM.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBSched.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/AsrFacadeLumenvox.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/ISMessaging.dll.up
#%attr(0440,speechbridge,speechbridge) %{BIN}/MonoTimers.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBConfig.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBConfigStor.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBEmail.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBLdapConn.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBResourceMgr.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBTTS.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SimpleAES.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/XmlScriptParser.dll.up
%attr(0550,speechbridge,speechbridge) %{BIN}/speechbridged.up
%attr(0550,speechbridge,speechbridge) %{BIN}/speechbridgemon.cron.up
#%attr(0640,speechbridge,speechbridge) %{BIN}/AudioMgr.exe.config.up
#%attr(0640,speechbridge,speechbridge) %{BIN}/DialogMgr.exe.config.up
#%attr(0640,speechbridge,speechbridge) %{BIN}/SBLocalRM.exe.config.up
#%attr(0640,speechbridge,speechbridge) %{BIN}/SBSched.exe.config.up
#%attr(0640,speechbridge,speechbridge) %{BIN}/AsrFacadeLumenvox.dll.config.up
#%attr(0444,speechbridge,speechbridge) %{BIN}/UpdateSipReg.sql
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/Global.asax.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/AADirectory.aspx.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/Default.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/EulaAccept.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/GenHash.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/GreetingPrompts.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Login.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/LoginIncorrect.aspx.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/Logout.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/ServerSettings.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/ServerSettingsLdap.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/UserEmailProps.aspx.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/Web.config.up
#%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/style.css.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/images/LogoImage.png.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/images/LogoImage_Incendonet.png.up
#%attr(0640,speechbridge,speechbridge) %{VDS}/AAMain.vxml.xml.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/AAMain_Template.vxml.xml.up
#%attr(0640,speechbridge,speechbridge) %{VDS}/CalendarMain.vxml.xml.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/CalendarMain_Template.vxml.xml.up
#%attr(0640,speechbridge,speechbridge) %{VDS}/EmailMain.vxml.xml.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/EmailMain_Template.vxml.xml.up
#%attr(0440,speechbridge,speechbridge) %{VDS}/SBRoot.vxml.xml.up

#####################################################################
#####################################################################
%changelog
#####################################################################
#####################################################################
* Tue Feb 3 2009  Support <support@incendonet.com>
- Updated for SpeechBridge 2.0.2.42-09042
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
