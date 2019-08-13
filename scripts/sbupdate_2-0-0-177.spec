Summary: SpeechBridge Update
Name: sbupdate
Version: 2.0.0
Release: 177
License: Incendonet
Group: Applications/Communications
Source: /opt/speechbridge/update/sbupdate_2-0-0-177.tar.gz

URL: http://www.incendonet.com/

#BuildRoot: %{_tmppath}/%{name}-%{version}-%{release}-buildroot
BuildRoot: /home/speechbridge/rpmbuild/%{name}-%{version}-%{release}-buildroot
BuildArch: i686

%define BAK     /opt/speechbridge/backup
%define BIN     /opt/speechbridge/bin
%define SBCONF  /opt/speechbridge/SBConfig
%define VDS     /opt/speechbridge/VoiceDocStore

%description
This package will update your installation to SpeechBridge 2.0.0.177

%prep

%build

%install
#rm -rf %{buildroot}

%clean
#rm -rf %{buildroot}

#####################################################################
%post
#####################################################################
/sbin/service speechbridged stop > /dev/null 2> /dev/null
sleep 5

mkdir -p %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mkdir -p %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mkdir -p %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/assets > /dev/null 2> /dev/null

mv -f %{BIN}/ProxySrv_centos44 %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
#mv -f %{BIN}/ProxySrv_centos44_optnoshared %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/ProxySrv_optnoshared_centos44 %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/AudioRtr_GenericSip_Centos4 %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/AudioRtr_GenericSip_Centos5 %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/AudioRtr_ShoretelSip_Centos5 %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/AudioMgr.exe %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/DialogMgr.exe %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/SBLocalRM.exe %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/SBSched.exe %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/AsrFacadeLumenvox.dll %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/ISMessaging.dll %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/MonoTimers.dll %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/SBConfig.dll %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/SBConfigStor.dll %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/SBEmail.dll %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/SBResourceMgr.dll %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/SBTTS.dll %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/SimpleAES.dll %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/XmlScriptParser.dll %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/speechbridged %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/AudioMgr.exe.config %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/DialogMgr.exe.config %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/SBLocalRM.exe.config %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/SBSched.exe.config %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{BIN}/AsrFacadeLumenvox.dll.config %{BAK}/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f %{SBCONF}/Global.asax %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/AADirectory.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/Default.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/GenHash.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/Login.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/LoginIncorrect.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/Logout.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/ServerSettings.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/ServerSettingsLdap.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/UserEmailProps.aspx %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/Web.config %{BAK}/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f %{SBCONF}/assets/style.css %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets > /dev/null 2> /dev/null
#mv -f %{VDS}/AAMain.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore > /dev/null 2> /dev/null
mv -f %{VDS}/AAMain_Template.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore > /dev/null 2> /dev/null
#mv -f %{VDS}/CalendarMain.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore > /dev/null 2> /dev/null
mv -f %{VDS}/CalendarMain_Template.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore > /dev/null 2> /dev/null
#mv -f %{VDS}/EmailMain.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore > /dev/null 2> /dev/null
mv -f %{VDS}/EmailMain_Template.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore > /dev/null 2> /dev/null
mv -f %{VDS}/SBRoot.vxml.xml %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore > /dev/null 2> /dev/null

mv -f %{BIN}/ProxySrv_optnoshared_centos44.up %{BIN}/ProxySrv_optnoshared_centos44 > /dev/null 2> /dev/null
mv -f %{BIN}/AudioRtr_GenericSip_Centos4.up %{BIN}/AudioRtr_GenericSip_Centos4 > /dev/null 2> /dev/null
mv -f %{BIN}/AudioMgr.exe.up %{BIN}/AudioMgr.exe > /dev/null 2> /dev/null
mv -f %{BIN}/DialogMgr.exe.up %{BIN}/DialogMgr.exe > /dev/null 2> /dev/null
mv -f %{BIN}/SBLocalRM.exe.up %{BIN}/SBLocalRM.exe > /dev/null 2> /dev/null
mv -f %{BIN}/SBSched.exe.up %{BIN}/SBSched.exe > /dev/null 2> /dev/null
mv -f %{BIN}/AsrFacadeLumenvox.dll.up %{BIN}/AsrFacadeLumenvox.dll > /dev/null 2> /dev/null
mv -f %{BIN}/ISMessaging.dll.up %{BIN}/ISMessaging.dll > /dev/null 2> /dev/null
mv -f %{BIN}/MonoTimers.dll.up %{BIN}/MonoTimers.dll > /dev/null 2> /dev/null
mv -f %{BIN}/SBConfig.dll.up %{BIN}/SBConfig.dll > /dev/null 2> /dev/null
mv -f %{BIN}/SBConfigStor.dll.up %{BIN}/SBConfigStor.dll > /dev/null 2> /dev/null
mv -f %{BIN}/SBEmail.dll.up %{BIN}/SBEmail.dll > /dev/null 2> /dev/null
mv -f %{BIN}/SBResourceMgr.dll.up %{BIN}/SBResourceMgr.dll > /dev/null 2> /dev/null
mv -f %{BIN}/SBTTS.dll.up %{BIN}/SBTTS.dll > /dev/null 2> /dev/null
mv -f %{BIN}/SimpleAES.dll.up %{BIN}/SimpleAES.dll > /dev/null 2> /dev/null
mv -f %{BIN}/XmlScriptParser.dll.up %{BIN}/XmlScriptParser.dll > /dev/null 2> /dev/null
mv -f %{BIN}/speechbridged.up %{BIN}/speechbridged > /dev/null 2> /dev/null
mv -f %{BIN}/AudioMgr.exe.config.up %{BIN}/AudioMgr.exe.config > /dev/null 2> /dev/null
mv -f %{BIN}/DialogMgr.exe.config.up %{BIN}/DialogMgr.exe.config > /dev/null 2> /dev/null
mv -f %{BIN}/SBLocalRM.exe.config.up %{BIN}/SBLocalRM.exe.config > /dev/null 2> /dev/null
mv -f %{BIN}/SBSched.exe.config.up %{BIN}/SBSched.exe.config > /dev/null 2> /dev/null
mv -f %{BIN}/AsrFacadeLumenvox.dll.config.up %{BIN}/AsrFacadeLumenvox.dll.config > /dev/null 2> /dev/null
mv -f %{SBCONF}/Global.asax.up %{SBCONF}/Global.asax > /dev/null 2> /dev/null
mv -f %{SBCONF}/AADirectory.aspx.up %{SBCONF}/AADirectory.aspx > /dev/null 2> /dev/null
mv -f %{SBCONF}/Default.aspx.up %{SBCONF}/Default.aspx > /dev/null 2> /dev/null
mv -f %{SBCONF}/GenHash.aspx.up %{SBCONF}/GenHash.aspx > /dev/null 2> /dev/null
mv -f %{SBCONF}/Login.aspx.up %{SBCONF}/Login.aspx > /dev/null 2> /dev/null
mv -f %{SBCONF}/LoginIncorrect.aspx.up %{SBCONF}/LoginIncorrect.aspx > /dev/null 2> /dev/null
mv -f %{SBCONF}/Logout.aspx.up %{SBCONF}/Logout.aspx > /dev/null 2> /dev/null
mv -f %{SBCONF}/ServerSettings.aspx.up %{SBCONF}/ServerSettings.aspx > /dev/null 2> /dev/null
mv -f %{SBCONF}/ServerSettingsLdap.aspx.up %{SBCONF}/ServerSettingsLdap.aspx > /dev/null 2> /dev/null
mv -f %{SBCONF}/UserEmailProps.aspx.up %{SBCONF}/UserEmailProps.aspx > /dev/null 2> /dev/null
mv -f %{SBCONF}/Web.config.up %{SBCONF}/Web.config > /dev/null 2> /dev/null
mv -f %{SBCONF}/assets/style.css.up %{SBCONF}/assets/style.css > /dev/null 2> /dev/null
#mv -f %{VDS}/AAMain.vxml.xml.up %{VDS}/AAMain.vxml.xml > /dev/null 2> /dev/null
mv -f %{VDS}/AAMain_Template.vxml.xml.up %{VDS}/AAMain_Template.vxml.xml > /dev/null 2> /dev/null
#mv -f %{VDS}/CalendarMain.vxml.xml.up %{VDS}/CalendarMain.vxml.xml > /dev/null 2> /dev/null
mv -f %{VDS}/CalendarMain_Template.vxml.xml.up %{VDS}/CalendarMain_Template.vxml.xml > /dev/null 2> /dev/null
#mv -f %{VDS}/EmailMain.vxml.xml.up %{VDS}/EmailMain.vxml.xml > /dev/null 2> /dev/null
mv -f %{VDS}/EmailMain_Template.vxml.xml.up %{VDS}/EmailMain_Template.vxml.xml > /dev/null 2> /dev/null
mv -f %{VDS}/SBRoot.vxml.xml.up %{VDS}/SBRoot.vxml.xml > /dev/null 2> /dev/null

ln -fs %{BIN}/ProxySrv_optnoshared_centos44 %{BIN}/ProxySrv > /dev/null 2> /dev/null
ln -fs %{BIN}/AudioRtr_GenericSip_Centos4 %{BIN}/AudioRtr > /dev/null 2> /dev/null

/sbin/service speechbridged start > /dev/null 2> /dev/null
/sbin/service httpd reload > /dev/null 2> /dev/null

#####################################################################
%postun
#####################################################################
/sbin/service speechbridged stop > /dev/null 2> /dev/null
sleep 5

mv -f %{BAK}/%{name}-%{version}.%{release}/bin/ProxySrv* %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioRtr* %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioMgr.exe %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/DialogMgr.exe %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLocalRM.exe %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBSched.exe %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AsrFacadeLumenvox.dll %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/ISMessaging.dll %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/MonoTimers.dll %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBConfig.dll %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBConfigStor.dll %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBEmail.dll %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBResourceMgr.dll %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBTTS.dll %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SimpleAES.dll %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/XmlScriptParser.dll %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/speechbridged %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AudioMgr.exe.config %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/DialogMgr.exe.config %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBLocalRM.exe.config %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/SBSched.exe.config %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/bin/AsrFacadeLumenvox.dll.config %{BIN} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Global.asax %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/AADirectory.aspx %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Default.aspx %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/GenHash.aspx %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Login.aspx %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/LoginIncorrect.aspx %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Logout.aspx %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/ServerSettings.aspx %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/ServerSettingsLdap.aspx %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/UserEmailProps.aspx %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/Web.config %{SBCONF} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/SBConfig/assets/style.css %{SBCONF}/assets > /dev/null 2> /dev/null
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/AAMain.vxml.xml %{VDS} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/AAMain_Template.vxml.xml %{VDS} > /dev/null 2> /dev/null
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/CalendarMain.vxml.xml %{VDS} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/CalendarMain_Template.vxml.xml %{VDS} > /dev/null 2> /dev/null
#mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/EmailMain.vxml.xml %{VDS} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/EmailMain_Template.vxml.xml %{VDS} > /dev/null 2> /dev/null
mv -f %{BAK}/%{name}-%{version}.%{release}/VoiceDocStore/SBRoot.vxml.xml %{VDS} > /dev/null 2> /dev/null

rm -rf %{BAK}/%{name}-%{version}.%{release} > /dev/null 2> /dev/null

/sbin/service speechbridged start > /dev/null 2> /dev/null
/sbin/service httpd reload > /dev/null 2> /dev/null

#####################################################################
%files
#####################################################################
%defattr(-,speechbridge,speechbridge,-)
%attr(0550,speechbridge,speechbridge) %{BIN}/ProxySrv_optnoshared_centos44.up
%attr(0550,speechbridge,speechbridge) %{BIN}/AudioRtr_GenericSip_Centos4.up
%attr(0440,speechbridge,speechbridge) %{BIN}/AudioMgr.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/DialogMgr.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBLocalRM.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBSched.exe.up
%attr(0440,speechbridge,speechbridge) %{BIN}/AsrFacadeLumenvox.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/ISMessaging.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/MonoTimers.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBConfig.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBConfigStor.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBEmail.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBResourceMgr.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SBTTS.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/SimpleAES.dll.up
%attr(0440,speechbridge,speechbridge) %{BIN}/XmlScriptParser.dll.up
%attr(0550,speechbridge,speechbridge) %{BIN}/speechbridged.up
%attr(0640,speechbridge,speechbridge) %{BIN}/AudioMgr.exe.config.up
%attr(0640,speechbridge,speechbridge) %{BIN}/DialogMgr.exe.config.up
%attr(0640,speechbridge,speechbridge) %{BIN}/SBLocalRM.exe.config.up
%attr(0640,speechbridge,speechbridge) %{BIN}/SBSched.exe.config.up
%attr(0640,speechbridge,speechbridge) %{BIN}/AsrFacadeLumenvox.dll.config.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Global.asax.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/AADirectory.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Default.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/GenHash.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Login.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/LoginIncorrect.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Logout.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/ServerSettings.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/ServerSettingsLdap.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/UserEmailProps.aspx.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/Web.config.up
%attr(0440,speechbridge,speechbridge) %{SBCONF}/assets/style.css.up
#%attr(0640,speechbridge,speechbridge) %{VDS}/AAMain.vxml.xml.up
%attr(0440,speechbridge,speechbridge) %{VDS}/AAMain_Template.vxml.xml.up
#%attr(0640,speechbridge,speechbridge) %{VDS}/CalendarMain.vxml.xml.up
%attr(0440,speechbridge,speechbridge) %{VDS}/CalendarMain_Template.vxml.xml.up
#%attr(0640,speechbridge,speechbridge) %{VDS}/EmailMain.vxml.xml.up
%attr(0440,speechbridge,speechbridge) %{VDS}/EmailMain_Template.vxml.xml.up
%attr(0440,speechbridge,speechbridge) %{VDS}/SBRoot.vxml.xml.up

#####################################################################
%changelog
#####################################################################
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
