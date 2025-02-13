using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using Unity.Mathematics;
using UnityEngine;

[BepInPlugin("cz.kajacx.lumamods.onehitwonder", "One hit wonder", "0.1.0")]
public class OneHitWonder : BaseUnityPlugin
{
    public static ManualLogSource logger;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
    void Awake()
    {
        Harmony harmony = new Harmony("cz.kajacx.lumamods.onehitwonder");
        harmony.PatchAll();
        Logger.LogInfo("One hit wonder mod loaded.");
        logger = Logger;
    }
}

[HarmonyPatch(typeof(DamageUtil), "GetMineableDamage")]
class Patch_DamageUtil_GetMineableDamage
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
    static bool Prefix(WorldItemsData worldItemTarget, HealthData health, InventoryItem tool, ref float __result)
    {
        // OneHitWonder.logger.LogInfo("Patch_DamageUtil_GetMineableDamage");

        float num = ToolUpgradeConstants.GetUpgradedToolDamage(tool, worldItemTarget);

        // Disable the required 2 hits
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
    static bool Prefix(WorldItemsData worldItemTarget, HealthData health, PlayerController player, int2 targetTile, InventoryItem tool, ref float __result)
    {
        // OneHitWonder.logger.LogInfo("Patch_DamageUtil_GetTreeDamage");

        float num = ToolUpgradeConstants.GetUpgradedToolDamage(tool, worldItemTarget);
        num *= GetTreeAgeMultiplier(player.Level, targetTile);

        // Disable the required 2 hits
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
