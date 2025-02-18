using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Security.Policy;
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

    public static bool GetPrivateField<T>(object obj, string fieldName, out T result)
    {
        LogInfo("Getting" + fieldName);
        LogInfo("Getting" + fieldName + " on " + (obj == null ? "NULL" : obj.ToString()));
        try
        {
            result = (T)AccessTools.Field(obj.GetType(), fieldName).GetValue(obj);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            LogInfo("Error getting" + fieldName + ": " + exception.Message);
            result = default;
            return false;
        }
    }

    public static void ForEach2<T>(T[,] items, Action<T, int, int> action)
    {
        int width = items.GetLength(0);
        int height = items.GetLength(1);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                action(items[i, j], i, j);
            }
        }
    }
}

[HarmonyPatch(typeof(Inventory), "Sort")]
class Patch_Inventory_Sort
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    static bool Prefix(Inventory __instance, IComparer<ItemStack> order, bool excludeHotbar = true)
    {
        if (Keyboard.current.shiftKey.isPressed)
        {
            FirstMod.LogInfo("Shift PRESSED yes");
        }
        else
        {
            FirstMod.LogInfo("Shift NOT PRESSED");
            return true; // run original method
        }

        var player = __instance.Player;
        FirstMod.LogInfo("Player: " + player);
        if (!FirstMod.GetPrivateField(__instance, "m_items", out ItemStack[] items))
        {
            return true; // run original method
        }

        //if (!FirstMod.GetPrivateField(player.Level, "m_data", out TileData[,] m_data))
        //{
        //    return true; // run original method
        //}

        //FirstMod.ForEach2(m_data, (obj, x, y) =>
        //{
        //    if (obj != null)
        //    {
        //        FirstMod.LogInfo(obj.ToString());
        //    }
        //    else
        //    {
        //        FirstMod.LogInfo("null m_data at" + x + " " + y);
        //    }
        //});

        if (!FirstMod.GetPrivateField(player.Level, "m_gameObjects", out TileOccupier[,] m_gameObjects))
        {
            return true; // run original method
        }

        FirstMod.LogInfo("Patch_Inventory_Sort ForEach2");
        FirstMod.ForEach2(m_gameObjects, (obj, x, y) =>
        {
            if (obj == null) return;

            var storage = obj.GetComponent<GenericStorageBox>();
            if (storage == null) return;

            var inventory = storage.Inventory;
            if (inventory == null) return;

            FirstMod.LogInfo("FOUND CHEST WITH INVENTORY!!");

            if (!FirstMod.GetPrivateField(inventory, "m_items", out ItemStack[] chestItems)) return;

            foreach (ItemStack item in chestItems)
            {
                if (item != null)
                {
                    FirstMod.LogInfo($"{item}: {item.item} - {item.amount}");
                }
            }
        });

        FirstMod.LogInfo("Patch_Inventory_Sort finished");
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




