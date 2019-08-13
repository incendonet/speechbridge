#!/bin/sh

BINDIR="/opt/speechbridge/bin"
LOGDIR="/opt/speechbridge/logs"
TESTDIR="$BINDIR/test/sipp"
#TESTIP=`/bin/hostname -i`
TESTIP=192.168.1.209

COUNT=`mono --wapi=hps | wc -l`
NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"

echo "$NOWSTR: Mono thread count: $COUNT." >> $LOGDIR/testcall.log
echo "$NOWSTR: Starting test calls..." >> $LOGDIR/testcall.log

# -s : SIP username
# -l : Max number of simultaneous calls
# -r : Call rate (calls per second)
# -m : Max total calls to place

#$TESTDIR/sipp -sf $TESTDIR/uac_pcap_MainMenu.xml $TESTIP -s TEST -l 4 -r 1 -m 4 --trace_err -watchdog_minor_maxtriggers 99999 -watchdog_major_maxtriggers 99999 2>> $LOGDIR/testcall.log
$TESTDIR/sipp -sf $TESTDIR/uac_pcap_MainMenu.xml $TESTIP -s TEST -l 4 -r 1 -m 100000 --trace_err -watchdog_minor_maxtriggers 99999 -watchdog_major_maxtriggers 99999 2>> $LOGDIR/testcall.log

NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"
echo "$NOWSTR: Test calls completed." >> $LOGDIR/testcall.log

COUNT=`mono --wapi=hps | wc -l`
NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"

echo "$NOWSTR: Mono thread count: $COUNT." >> $LOGDIR/testcall.log
echo ""  >> $LOGDIR/testcall.log

