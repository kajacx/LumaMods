using BepInEx;
using UnityEngine;

[BepInPlugin("com.yourname.lumaisland.firstmod", "First Luma Island Mod", "1.0.0")]
public class FirstMod : BaseUnityPlugin
{
    void Awake()
    {
        Logger.LogInfo("Hello, Luma Island! My mod is now loaded, yea!");
    }
}
