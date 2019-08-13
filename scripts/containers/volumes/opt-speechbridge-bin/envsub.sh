#!/bin/sh
while read LINE
do
    MODDED=$(echo "$LINE" | sed 's|"|{DQ}|g' | sed 's|<|{LT}|g' | sed 's|>|{GT}|g' | sed 's|(|{LP}|g' | sed 's|)|{RP}|g' | sed 's|;|{SC}|g')
    eval echo "$MODDED" | sed 's|{DQ}|"|g' | sed 's|{LT}|<|g' | sed 's|{GT}|>|g' | sed 's|{LP}|(|g' | sed 's|{RP}|)|g' | sed 's|{SC}|;|g'
done

