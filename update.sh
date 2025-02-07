#!/usr/bin/sh

SOURCE_DLL="/e/Programming/CS/LumaMods/LumaMods/bin/Debug/LumaMods.dll"
TARGET_DLL="/e/SteamLibrary/steamapps/common/Luma Island/BepInEx/plugins/LumaMods.dll"
INTERVAL=1 # In seconds

echo "Waiting for changes..."

while true; do
    SRC_TIME=$(stat -c %Y "$SOURCE_DLL")
    DEST_TIME=$(stat -c %Y "$TARGET_DLL" 2>/dev/null || echo 0)

    if [[ "$SRC_TIME" -gt "$DEST_TIME" ]]; then
        cp "$SOURCE_DLL" "$TARGET_DLL"
        nircmd mediaplay 100 ./sounds/mixkit-long-pop-2358.wav
        echo "Updated mod copied at $(date)"
    fi
    sleep "$INTERVAL"
done
