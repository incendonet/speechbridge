#!/bin/sh

BINDIR="/opt/speechbridge/bin"
LOGDIR="/opt/speechbridge/logs"
TESTDIR="$BINDIR/test"
#TESTIP=`/bin/hostname -i`
TESTIP=192.168.1.209

COUNT=`mono --wapi=hps | wc -l`
NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"

echo "$NOWSTR: Mono thread count: $COUNT." >> $LOGDIR/testcall.log
echo "$NOWSTR: Starting test calls..." >> $LOGDIR/testcall.log

#$TESTDIR/sipp_rhel5 -sf $TESTDIR/uac_pcap_MainMenu.xml $TESTIP -s TEST -l 4 -r 1 -m 4 --trace_err > /dev/null 2> /dev/null
#$TESTDIR/sipp_rhel5 -sf $TESTDIR/uac_pcap_MainMenu.xml $TESTIP -s TEST -l 4 -r 1 -m 400 --trace_err
$TESTDIR/sipp_rhel5 -sf $TESTDIR/uac_pcap_MainMenu.xml $TESTIP -s TEST -l 12 -r 1 -m 1000000 --trace_err 2>> $LOGDIR/testcall.log

NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"
echo "$NOWSTR: Test calls completed." >> $LOGDIR/testcall.log

COUNT=`mono --wapi=hps | wc -l`
NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"

echo "$NOWSTR: Mono thread count: $COUNT." >> $LOGDIR/testcall.log
echo ""  >> $LOGDIR/testcall.log

