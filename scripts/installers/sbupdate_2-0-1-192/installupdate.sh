#!/bin/sh

VER=speakupdate-3.0.1-192
INSTLOG=/opt/speechbridge/logs/$VER.log.txt

/sbin/service speechbridged stop >> $INSTLOG 2>> $INSTLOG
/sbin/service httpd stop >> $INSTLOG 2>> $INSTLOG

/bin/echo "Uninstalling Mono 1.2.4..."
/bin/echo "Uninstalling Mono 1.2.4..." >> $INSTLOG
/bin/rpm -ev mod_mono.i386 mono-core.i386 mono-data.i386 mono-data-postgresql.i386 mono-nunit.i386 mono-web.i386 mono-winforms.i386 xsp.i386 libgdiplus.i386 gtk-sharp2.i386 >> $INSTLOG 2>> $INSTLOG

/bin/echo "Installing Mono 1.2.6..."
/bin/echo "Installing Mono 1.2.6..." >> $INSTLOG
/usr/bin/yum -y install giflib >> $INSTLOG 2>> $INSTLOG
/bin/rpm -ivh gtk-sharp2-2.10.0-6.el5.centos.i386.rpm libgdiplus-1.2.5-1.el5.centos.i386.rpm mod_mono-1.2.1-1.el5.centos.i386.rpm mono-core-1.2.6-4.novell.i586.rpm mono-data-1.2.6-4.novell.i586.rpm mono-data-postgresql-1.2.6-4.novell.i586.rpm mono-nunit-1.2.6-4.novell.i586.rpm mono-data-sqlite-1.2.6-4.novell.i586.rpm mono-web-1.2.6-4.novell.i586.rpm mono-winforms-1.2.6-4.novell.i586.rpm xsp-1.2.1-1.el5.centos.i386.rpm >> $INSTLOG 2>> $INSTLOG
ln -s /usr/lib/libgdiplus.so.0.0.0 /usr/lib/libgdiplus.so >> $INSTLOG 2>> $INSTLOG

/bin/echo "Installing $VER.i686.rpm..."
/bin/echo "Installing $VER.i686.rpm..." >> $INSTLOG
/bin/rpm -ivh $VER.i686.rpm >> $INSTLOG 2>> $INSTLOG

/sbin/service httpd start >> $INSTLOG 2>> $INSTLOG
/sbin/service speechbridged start >> $INSTLOG 2>> $INSTLOG

cat $INSTLOG
