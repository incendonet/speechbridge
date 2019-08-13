#!/bin/sh
#

logpath=/opt/speechbridge/logs
yesterdaystr="$(/bin/date --date=yesterday +%Y%m%d)"

mv $logpath/SBLocalRM-stdout.log $logpath/SBLocalRM-stdout_${yesterdaystr}.log
mv $logpath/SBLocalRM-stderr.log $logpath/SBLocalRM-stderr_${yesterdaystr}.log
