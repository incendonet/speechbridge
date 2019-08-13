#!/bin/sh

TESTIP=192.168.1.203
NUMPORTS=6

BINDIR="/opt/speechbridge/bin"
TESTDIR="$BINDIR/test/sipp"
LOGDIR="$TESTDIR/logs"
#TESTIP=`/bin/hostname -i`

COUNT=`mono --wapi=hps | wc -l`
NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"

echo "$NOWSTR: Mono thread count: $COUNT." >> $LOGDIR/testcall.log
echo "$NOWSTR: Starting test calls..." >> $LOGDIR/testcall.log

# -s : SIP username
# -l : Max number of simultaneous calls
# -r : Call rate (calls per second)
# -m : Max total calls to place

$TESTDIR/sipp -sf $TESTDIR/uac_pcap_MainMenu.xml $TESTIP -s TEST -l $NUMPORTS -r 1 -m 100000 --trace_err -watchdog_minor_maxtriggers 99999 -watchdog_major_maxtriggers 99999 2>> $LOGDIR/testcall.log

NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"
echo "$NOWSTR: Test calls completed." >> $LOGDIR/testcall.log

COUNT=`mono --wapi=hps | wc -l`
NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"

echo "$NOWSTR: Mono thread count: $COUNT." >> $LOGDIR/testcall.log
echo ""  >> $LOGDIR/testcall.log

