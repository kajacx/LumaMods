#!/usr/bin/sh

declare -A MODS=(
    ["/e/Programming/CS/LumaMods/LumaMods/bin/Debug/LumaMods.dll"]="/e/SteamLibrary/steamapps/common/Luma Island/BepInEx/plugins/LumaMods.dll"
    ["/e/Programming/CS/LumaMods/OneHitWonder/bin/Debug/net8.0/OneHitWonder.dll"]="/e/SteamLibrary/steamapps/common/Luma Island/BepInEx/plugins/OneHitWonder.dll"
)

INTERVAL=1 # In seconds

echo "Waiting for changes..."

while true; do
    for SOURCE_DLL in "${!MODS[@]}"; do
        TARGET_DLL="${MODS[$SOURCE_DLL]}"
        
        # Check if the source DLL exists
        if [[ ! -f "$SOURCE_DLL" ]]; then
            echo "Warning: $SOURCE_DLL not found!"
            continue
        fi
        
        SRC_TIME=$(stat -c %Y "$SOURCE_DLL")
        DEST_TIME=$(stat -c %Y "$TARGET_DLL" 2>/dev/null || echo 0)
        
        if [[ "$SRC_TIME" -gt "$DEST_TIME" ]]; then
            cp "$SOURCE_DLL" "$TARGET_DLL"
            nircmd mediaplay 100 ./sounds/mixkit-long-pop-2358.wav
            echo "Updated" $(basename "$SOURCE_DLL") "copied at $(date)"
        fi
    done
    
    sleep "$INTERVAL"
done
