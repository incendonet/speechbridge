Summary: SpeechBridge SIP UA Patch
Name: sbupdate
Version: 0.8.1.22
Release: 3
License: Incendonet
Group: Applications/Communications
Source: /home/speechbridge/update/sbupdate-0.8.1.22.3.tar.gz

URL: http://www.incendonet.com/

#BuildRoot: %{_tmppath}/%{name}-%{version}-%{release}-buildroot
BuildRoot: /home/speechbridge/update/%{name}-%{version}-%{release}-buildroot
BuildArch: noarch

%description
This SpeechBridge update resolves the following issues:

%prep

%build

%install
#rm -rf %{buildroot}

%clean
#rm -rf %{buildroot}

%post
/sbin/service speechbridgedaemon stop > /dev/null 2> /dev/null
mkdir -p /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mkdir -p /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/www/bin > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/bin/gua /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/bin/AudioEngine_Console.exe /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/bin/DialogMgr_Console.exe /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/bin/ISMessaging.dll /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/bin > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/www/AADirectory.aspx /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/www > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/www/ServerSettings.aspx /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/www > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/www/Web.config /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/www > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/www/bin/SBConfig.dll /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/www/bin > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/bin/gua.up /home/speechbridge/ISProto.root/bin/gua > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/bin/AudioEngine_Console.exe.up /home/speechbridge/ISProto.root/bin/AudioEngine_Console.exe > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/bin/DialogMgr_Console.exe.up /home/speechbridge/ISProto.root/bin/DialogMgr_Console.exe > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/bin/ISMessaging.dll.up /home/speechbridge/ISProto.root/bin/ISMessaging.dll > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/www/AADirectory.aspx.up /home/speechbridge/ISProto.root/www/AADirectory.aspx > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/www/ServerSettings.aspx.up /home/speechbridge/ISProto.root/www/ServerSettings.aspx > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/www/Web.config.up /home/speechbridge/ISProto.root/www/Web.config > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/www/bin/SBConfig.dll.up /home/speechbridge/ISProto.root/www/bin/SBConfig.dll > /dev/null 2> /dev/null
mv -f /sbin/service speechbridgedaemon start > /dev/null 2> /dev/null

%postun
/sbin/service speechbridgedaemon stop > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/bin/gua /home/speechbridge/ISProto.root/bin > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/bin/AudioEngine_Console.exe /home/speechbridge/ISProto.root/bin > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/bin/DialogMgr_Console.exe /home/speechbridge/ISProto.root/bin > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/bin/ISMessaging.dll /home/speechbridge/ISProto.root/bin > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/www/AADirectory.aspx /home/speechbridge/ISProto.root/www > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/www/ServerSettings.aspx /home/speechbridge/ISProto.root/www > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/www/Web.config /home/speechbridge/ISProto.root/www > /dev/null 2> /dev/null
mv -f /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release}/www/bin/SBConfig.dll /home/speechbridge/ISProto.root/www/bin > /dev/null 2> /dev/null
rm -rf /home/speechbridge/ISProto.root/backup/%{name}-%{version}.%{release} > /dev/null 2> /dev/null
/sbin/service speechbridgedaemon start > /dev/null 2> /dev/null

%files
%defattr(-,speechbridge,speechbridge,-)
%attr(0755,speechbridge,speechbridge) /home/speechbridge/ISProto.root/bin/gua.up
%attr(0755,speechbridge,speechbridge) /home/speechbridge/ISProto.root/bin/AudioEngine_Console.exe.up
%attr(0755,speechbridge,speechbridge) /home/speechbridge/ISProto.root/bin/DialogMgr_Console.exe.up
%attr(0755,speechbridge,speechbridge) /home/speechbridge/ISProto.root/bin/ISMessaging.dll.up
%attr(0644,speechbridge,speechbridge) /home/speechbridge/ISProto.root/www/AADirectory.aspx.up
%attr(0644,speechbridge,speechbridge) /home/speechbridge/ISProto.root/www/ServerSettings.aspx.up
%attr(0544,speechbridge,speechbridge) /home/speechbridge/ISProto.root/www/Web.config.up
%attr(0755,speechbridge,speechbridge) /home/speechbridge/ISProto.root/www/bin/SBConfig.dll.up

%changelog
* Fri Feb 10 2006  B Ayers <bayers@incendonet.com>
- Updated for 0.8.1.22.3.
* Fri Jan 27 2006  B Ayers <bayers@incendonet.com>
- Created initial spec file
