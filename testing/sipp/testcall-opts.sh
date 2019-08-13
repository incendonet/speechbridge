#!/bin/sh

BINDIR="/opt/speechbridge/bin"
LOGDIR="/opt/speechbridge/logs"
TESTDIR="$BINDIR/test"
LOCALIP=`/bin/hostname -i`

#$TESTDIR/sipp_rhel5 -sf $TESTDIR/uac_pcap_MainMenu-Opts.xml $LOCALIP -s TEST -l 1 -r 1 -m 1 -trace_msg -trace_logs -trace_err
#$TESTDIR/sipp_rhel5 -sf $TESTDIR/uac_pcap_MainMenu-Opts.xml $LOCALIP -s TEST -l 1 -r 1 -m 1 > /dev/null 2> /dev/null
$TESTDIR/sipp_rhel5 -sf $TESTDIR/uac_pcap_MainMenu-Opts.xml 192.168.1.206 -s TEST -l 1 -r 1 -m 1 -trace_msg -trace_logs -trace_err
