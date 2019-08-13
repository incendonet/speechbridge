Summary: SpeechBridge Update
Name: sbupdate
Version: 1.3.258.1
Release: 1
License: Incendonet
Group: Applications/Communications
Source: /opt/speechbridge/update/sbupdate_1-3-259-1.tar.gz

URL: http://www.incendonet.com/

#BuildRoot: %{_tmppath}/%{name}-%{version}-%{release}-buildroot
BuildRoot: /opt/speechbridge/update/%{name}-%{version}-%{release}-buildroot
BuildArch: noarch

%description
This SpeechBridge update resolves the following issues:

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

mkdir -p /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mkdir -p /opt/speechbridge/backup/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null

mv -f /opt/speechbridge/bin/AudioMgr.exe /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/DialogMgr.exe /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBLocalRM.exe /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
#mv -f /opt/speechbridge/bin/SBSched.exe /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/AsrFacadeLumenvox.dll /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/ISMessaging.dll /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
#mv -f /opt/speechbridge/bin/MonoTimers.dll /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBConfig.dll /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBConfigStor.dll /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBEmail.dll /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBResourceMgr.dll /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBTTS.dll /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SimpleAES.dll /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/XmlScriptParser.dll /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/speechbridged /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBLocalRM.exe.config /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
#mv -f /opt/speechbridge/bin/SBSched.exe.config /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/SBConfig/UserEmailProps.aspx /opt/speechbridge/backup/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null
mv -f /opt/speechbridge/SBConfig/ServerSettings.aspx /opt/speechbridge/backup/%{name}-%{version}.%{release}/SBConfig > /dev/null 2> /dev/null

mv -f /opt/speechbridge/bin/AudioMgr.exe.up /opt/speechbridge/bin/AudioMgr.exe > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/DialogMgr.exe.up /opt/speechbridge/bin/DialogMgr.exe > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBLocalRM.exe.up /opt/speechbridge/bin/SBLocalRM.exe > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBSched.exe.up /opt/speechbridge/bin/SBSched.exe > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/AsrFacadeLumenvox.dll.up /opt/speechbridge/bin/AsrFacadeLumenvox.dll > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/ISMessaging.dll.up /opt/speechbridge/bin/ISMessaging.dll > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/MonoTimers.dll.up /opt/speechbridge/bin/MonoTimers.dll > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBConfig.dll.up /opt/speechbridge/bin/SBConfig.dll > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBConfigStor.dll.up /opt/speechbridge/bin/SBConfigStor.dll > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBEmail.dll.up /opt/speechbridge/bin/SBEmail.dll > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBResourceMgr.dll.up /opt/speechbridge/bin/SBResourceMgr.dll > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBTTS.dll.up /opt/speechbridge/bin/SBTTS.dll > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SimpleAES.dll.up /opt/speechbridge/bin/SimpleAES.dll > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/XmlScriptParser.dll.up /opt/speechbridge/bin/XmlScriptParser.dll > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/speechbridged.up /opt/speechbridge/bin/speechbridged > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBLocalRM.exe.config.up /opt/speechbridge/bin/SBLocalRM.exe.config > /dev/null 2> /dev/null
mv -f /opt/speechbridge/bin/SBSched.exe.config.up /opt/speechbridge/bin/SBSched.exe.config > /dev/null 2> /dev/null
mv -f /opt/speechbridge/SBConfig/UserEmailProps.aspx.up /opt/speechbridge/SBConfig/UserEmailProps.aspx > /dev/null 2> /dev/null
mv -f /opt/speechbridge/SBConfig/ServerSettings.aspx.up /opt/speechbridge/SBConfig/ServerSettings.aspx > /dev/null 2> /dev/null

/sbin/service speechbridged start > /dev/null 2> /dev/null
/sbin/service httpd reload > /dev/null 2> /dev/null

#####################################################################
%postun
#####################################################################
/sbin/service speechbridged stop > /dev/null 2> /dev/null

mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/AudioMgr.exe /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/DialogMgr.exe /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/SBLocalRM.exe /opt/speechbridge/bin > /dev/null 2> /dev/null
#mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/SBSched.exe /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/AsrFacadeLumenvox.dll /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/ISMessaging.dll /opt/speechbridge/bin > /dev/null 2> /dev/null
#mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/MonoTimers.dll /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/SBConfig.dll /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/SBConfigStor.dll /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/SBEmail.dll /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/SBResourceMgr.dll /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/SBTTS.dll /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/SimpleAES.dll /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/XmlScriptParser.dll /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/speechbridged /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/SBLocalRM.exe.config /opt/speechbridge/bin > /dev/null 2> /dev/null
#mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/bin/SBSched.exe.config /opt/speechbridge/bin > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/SBConfig/UserEmailProps.aspx /opt/speechbridge/SBConfig > /dev/null 2> /dev/null
mv -f /opt/speechbridge/backup/%{name}-%{version}.%{release}/SBConfig/ServerSettings.aspx /opt/speechbridge/SBConfig > /dev/null 2> /dev/null

rm -rf /opt/speechbridge/backup/%{name}-%{version}.%{release} > /dev/null 2> /dev/null

/sbin/service speechbridged start > /dev/null 2> /dev/null
/sbin/service httpd reload > /dev/null 2> /dev/null

#####################################################################
%files
#####################################################################
%defattr(-,speechbridge,speechbridge,-)
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/AudioMgr.exe.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/DialogMgr.exe.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/SBLocalRM.exe.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/SBSched.exe.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/AsrFacadeLumenvox.dll.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/ISMessaging.dll.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/MonoTimers.dll.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/SBConfig.dll.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/SBConfigStor.dll.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/SBEmail.dll.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/SBResourceMgr.dll.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/SBTTS.dll.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/SimpleAES.dll.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/bin/XmlScriptParser.dll.up
%attr(0550,speechbridge,speechbridge) /opt/speechbridge/bin/speechbridged.up
%attr(0640,speechbridge,speechbridge) /opt/speechbridge/bin/SBLocalRM.exe.config.up
%attr(0640,speechbridge,speechbridge) /opt/speechbridge/bin/SBSched.exe.config.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/SBConfig/UserEmailProps.aspx.up
%attr(0440,speechbridge,speechbridge) /opt/speechbridge/SBConfig/ServerSettings.aspx.up

#####################################################################
%changelog
#####################################################################
* Tue Sep 18 2007  Support <support@incendonet.com>
- Updated for 1.3.259.1
* Fri Feb 10 2006  B Ayers <bayers@incendonet.com>
- Updated for 0.8.1.22.3.
* Fri Jan 27 2006  B Ayers <bayers@incendonet.com>
- Created initial spec file
