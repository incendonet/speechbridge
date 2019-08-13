#!/bin/sh

SYSCONF=/etc/sysconfig

# Enable and start xrdp
/sbin/chkconfig xrdp on
/sbin/service xrdp start

# Pinhole port 3389 in the firewall (iptables and redhat admin tool)
/bin/sed 's|-A RH-Firewall-1-INPUT -m state --state NEW -m udp -p udp --dport 5060 -j ACCEPT|-A RH-Firewall-1-INPUT -m state --state NEW -m tcp -p tcp --dport 3389 -j ACCEPT\
-A RH-Firewall-1-INPUT -m state --state NEW -m udp -p udp --dport 5060 -j ACCEPT|' < $SYSCONF/iptables > $SYSCONF/iptables2
mv -f $SYSCONF/iptables2 $SYSCONF/iptables

/bin/sed 's|--port=5060:udp|--port=3389:tcp\
--port=5060:udp|' < $SYSCONF/system-config-securitylevel > $SYSCONF/system-config-securitylevel2
mv -f $SYSCONF/system-config-securitylevel2 $SYSCONF/system-config-securitylevel

# Restart the firewall
/sbin/service iptables restart
