using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Unity.Mathematics;
using UnityEngine;

[BepInPlugin("com.yourname.lumaisland.firstmod", "First Luma Island Mod", "1.0.0")]
public class FirstMod : BaseUnityPlugin
{
    public static ManualLogSource logger = null;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    void Awake()
    {
        Harmony harmony = new Harmony("com.yourname.lumaisland.mod");
        harmony.PatchAll();
        Logger.LogInfo("Hello, Luma Island! My mod is now loaded, yea!");
        logger = Logger;
    }
}

[HarmonyPatch(typeof(LumaAI), "Update")]
class Patch_LumaAiUpdate
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    static bool Prefix(LumaAI __instance)
    {
        // FirstMod.logger.LogInfo("Luma AI Update override");

        return true; // Allow the original method to run
        //return false; // Disable the method
    }
}




