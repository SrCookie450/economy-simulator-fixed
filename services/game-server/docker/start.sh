#!/bin/bash
# Create xvfb screen (virtual frame buffer) for rendering
DISPLAY_ID=$(( $RANDOM % 100 + 1 ))
echo "Creating XVFB on display=$DISPLAY_ID";

Xvfb :$DISPLAY_ID -screen 0 1600x1200x24+32 &

cd /usr/src/app/RCCService/ && 
chmod a+x ./rcc &&
echo "Start RCC args: $@" &&

if [ "$1" == "convertgame" ]; then
    echo "Starting placeconverter instead (game)";
    ls /app && ls /app/place-conversion-data/;
    ./RobloxPlaceConverter game /app/place-conversion-data/out.rbxl /app/place-conversion-data/in.rbxl;
    exit 0;
elif [ "$1" == "converthat" ]; then
    echo "Starting placeconverter instead (hat)";
    ls /app && ls /app/place-conversion-data/;
    ./RobloxPlaceConverter hat /app/place-conversion-data/out.rbxl /app/place-conversion-data/in.rbxl;
    exit 0;
fi;

# Apparently stdbuf segfaults if you print a lot.
# Uncomment the line below me and comment out the last line if you want to debug RCC.
DISPLAY=:$DISPLAY_ID stdbuf -i0 -o0 -e0 ./rcc $@
# DISPLAY=:1 ./rcc $@