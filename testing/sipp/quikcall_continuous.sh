#!/bin/sh

BINDIR="/opt/speechbridge/bin"
LOGDIR="/opt/speechbridge/logs"
TESTDIR="$BINDIR/test/sipp"
#TESTIP=`/bin/hostname -i`
TESTIP=192.168.1.222

COUNT=`mono --wapi=hps | wc -l`
NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"

echo "$NOWSTR: Mono thread count: $COUNT." >> $LOGDIR/quickcall_once.log
echo "$NOWSTR: Starting test calls..." >> $LOGDIR/quickcall_once.log

# -s : SIP username
# -l : Max number of simultaneous calls
# -r : Call rate (calls per second)
# -m : Max total calls to place

$TESTDIR/sipp -sf $TESTDIR/uac_QuickCall.xml $TESTIP -s TEST -l 1 -r 1 -m 10000000 --trace_err -watchdog_minor_maxtriggers 99999 -watchdog_major_maxtriggers 99999 2>> $LOGDIR/quickcall_once.log

NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"
echo "$NOWSTR: Test calls completed." >> $LOGDIR/quickcall_once.log

COUNT=`mono --wapi=hps | wc -l`
NOWSTR="$(/bin/date +%Y%m%d-%H%M%S)"

echo "$NOWSTR: Mono thread count: $COUNT." >> $LOGDIR/quickcall_once.log
echo ""  >> $LOGDIR/quickcall_once.log

