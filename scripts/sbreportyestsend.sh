#!/bin/sh
#

logpath=/opt/speechbridge/logs

#sbgenreportcron
yesterdaystr="$(/bin/date --date=yesterday +%Y%m%d)"
yesterdayleg="$(/bin/date --date=yesterday +%m/%d/%Y)"
/opt/speechbridge/bin/sbreportgen.sh ${yesterdaystr} > $logpath/sbreport_${yesterdaystr}.txt
#mail -s "SpeechBridge report for ${yesterdayleg}" youradmin@yourcompany.com < $logpath/sbreport_${yesterdaystr}.txt
