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
        FirstMod.logger.LogInfo("Luma AI Update override");

        return true; // Allow the original method to run
        //return false; // Disable the method
    }
}

[HarmonyPatch(typeof(DamageUtil), "GetMineableDamage")]
class Patch_DamageUtil_GetMineableDamage
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    static bool Prefix(WorldItemsData worldItemTarget, HealthData health, InventoryItem tool, ref float __result)
    {
        FirstMod.logger.LogInfo("Patch_DamageUtil_GetMineableDamage");

        float num = ToolUpgradeConstants.GetUpgradedToolDamage(tool, worldItemTarget);

        // Disanble the required 2 hits
        // if (!tool.IsMax && num >= health.HealthOrPredictedHealth && health.IsAtMaxHealth)
        // {
        //     num *= 0.5f;
        // }

        __result = num;
        return false; // Do not run the original method
    }
}

[HarmonyPatch(typeof(DamageUtil), "GetTreeDamage")]
class Patch_DamageUtil_GetTreeDamage
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    static bool Prefix(WorldItemsData worldItemTarget, HealthData health, PlayerController player, int2 targetTile, InventoryItem tool, ref float __result)
    {
        FirstMod.logger.LogInfo("Patch_DamageUtil_GetTreeDamage");

        float num = ToolUpgradeConstants.GetUpgradedToolDamage(tool, worldItemTarget);
        num *= GetTreeAgeMultiplier(player.Level, targetTile);

        // Disanble the required 2 hits
        // if (!tool.IsMax && num >= health.HealthOrPredictedHealth && health.IsAtMaxHealth)
        // {
        //     num *= 0.5f;
        // }

        __result = num;
        return false; // Do not run the original method
    }

    // private method, just copying it works, I guess
    private static float GetTreeAgeMultiplier(Level level, int2 targetTile)
    {
        TileData data = level.GetData(targetTile);
        if (data == null)
        {
            return 1f;
        }
        TreeData customData = data.GetCustomData<TreeData>();
        if (customData == null)
        {
            return 1f;
        }
        float t = (float)customData.GrowthDays / 8f;
        return Mathf.Lerp(5f, 1f, t);
    }
}



