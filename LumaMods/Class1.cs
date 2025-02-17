using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public static void LogInfo(string message)
    {
        logger.LogInfo(message);
    }
}

[HarmonyPatch(typeof(Inventory), "Sort")]
class Patch_Inventory_Sort
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    static bool Prefix(PlayerInventoryPanel __instance, IComparer<ItemStack> order, bool excludeHotbar = true)
    {
        if (Keyboard.current.shiftKey.isPressed)
        {
            FirstMod.LogInfo("Shift PRESSED");
        }
        else
        {
            FirstMod.LogInfo("Shift NOT PRESSED");
        }

        return true; // run original method
    }
}

[HarmonyPatch(typeof(PlayerInventoryPanel), "OnCreate")]
class Patch_PlayerInventoryPanel_OnCreate
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    static void Postfix(PlayerInventoryPanel __instance)
    {
        FirstMod.LogInfo("Postfix start");

        var sortButtonField = AccessTools.Field(typeof(PlayerInventoryPanel), "m_sortButton");
        ButtonWidget sortButton = (ButtonWidget) sortButtonField.GetValue(__instance);

        // This runs *after* the original Start() method
        ButtonWidget quickStackButton = GameObject.Instantiate(sortButton);
        quickStackButton.transform.SetParent(__instance.transform);
        //quickStackButton.GetComponentInChildren<Text>().text = "Quick Stack";
        quickStackButton.OnPressed += () => FirstMod.LogInfo("QUICK STACKED CLICKED!!!");

        ButtonWidget qs = new ButtonWidget();
        qs.OnPressed += () => FirstMod.LogInfo("QS CLICK");
        //__instance.

        FirstMod.LogInfo("Postfix end");
    }
}




