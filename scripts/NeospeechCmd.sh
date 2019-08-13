#!/bin/bash
#
# Copyright 2015, All rights reserved, Incendonet Inc.
#
# NeospeechCmd.sh
#
# This script reformats input strings starting or ending with the 'MARK' string
# (defined below) to SSML equivalents.
#

SBBASE=/opt/speechbridge
SBBIN=$SBBASE/bin
SBCONF=$SBBASE/config
SBLOGS=$SBBASE/logs
SBSBCONFIG=$SBBASE/SBConfig
SBVDS=$SBBASE/VoiceDocStore
CONFIG_FILE=$SBBIN/NeospeechCmd.config

NEOBASE=/usr/vt
NEOSAMPLE=$NEOBASE/sample/ttssample

OUTFILE=`echo "$1" | sed 's!.*/!!'`
TEXTSRC=$2

MARK='::'

# Default voice if config file not found:
VOICE="Kate"



if [ $# -lt 2 ]
then
	# Use default voice to print out the required arguments
	$SBBIN/NeospeechCmd_$VOICE
	exit 0
fi

# Read in the config file for the value of $VOICE
[ -r "$CONFIG_FILE" ] && . "$CONFIG_FILE"

# Get ID of voice name
if [ "$VOICE" == "Kate" ]; then
	VOICEID=100
elif [ "$VOICE" == "Paul" ]; then
	VOICEID=101
elif [ "$VOICE" == "Julie" ]; then
	VOICEID=103
elif [ "$VOICE" == "Ashley" ]; then
	VOICEID=105
elif [ "$VOICE" == "Violetta" ]; then
	VOICEID=400
elif [ "$VOICE" == "Francisco" ]; then
	VOICEID=401
elif [ "$VOICE" == "Gloria" ]; then
	VOICEID=402
else
	VOICEID=100
fi


MARKUP=none

# Create temp files for grep and sed input.
FILESRC=`mktemp -t ssmltrans.input.XXXXXXXXXX`
echo $TEXTSRC > $FILESRC

if [ `grep $MARK $FILESRC | wc -l` -ge 1 ]; then
	MARKUP=ssml
fi


if [ "$MARKUP" == "ssml" ] && [ -f $NEOSAMPLE ]; then

	# Create temp files for sed output.
	FILEDEST=`mktemp -t ssmltrans.output.XXXXXXXXXX`
	cp -f $FILESRC $FILEDEST

	# '<break time="___"/>' from 'break'
	sed -i 's|'$MARK'break|\<break time=\"|g' $FILEDEST
	sed -i 's|break'$MARK'|\"/\>|g' $FILEDEST

	# '<emphasis>' and '</emphasis>'
	sed -i 's|'$MARK'emphasis|\<emphasis\>|g' $FILEDEST
	sed -i 's|\emphasis'$MARK'|\</emphasis\>|g' $FILEDEST

	TEXTLEN=$((`wc -m < $FILEDEST`-1))
	TEXT=$(<$FILEDEST)

#	echo "This is the text: '"$TEXT"'."
#	echo "Text length == "$TEXTLEN 2> /dev/null
#	echo "The output file is: '"$OUTFILE"'."
#	echo "Voice ID is:" $VOICEID

	# The Neospeech API wants a path relative to /usr/vt/result, and automatically
	# adds ".wav" to the end of the filename specified.

	# PCM, 16-bit, 8KHz
	#$NEOSAMPLE 8 127.0.0.1 7000 "$TEXT" $TEXTLEN ../../../tmp $OUTFILE $VOICEID 1
	#mv /tmp/$OUTFILE.wav $1

	# mu-law, 8-bit, 8KHz
	$NEOSAMPLE 8 127.0.0.1 7000 "$TEXT" $TEXTLEN ../../../tmp $OUTFILE $VOICEID 3
	mv /tmp/$OUTFILE.pcm $1

	# Clean up
	rm -f $FILESRC
	rm -f $FILEDEST

else
	echo "Running NeospeechCmd_$VOICE"
	$SBBIN/NeospeechCmd_$VOICE $1 "$2" $3 $4 $5 $6
fi
