using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[BepInPlugin("com.yourname.lumaisland.firstmod", "First Luma Island Mod", "1.0.0")]
public class FirstMod : BaseUnityPlugin
{
    public static ConfigEntry<bool> enable;
    public static ConfigEntry<string> quickStackWhitelist;

    public static readonly List<string> knownEntities = new List<string>()
    {
        "SimpleChest", "FeedingTrough"
    };

    public static ManualLogSource logger = null;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    void Awake()
    {
        Logger.LogInfo("Luma island first mod awake start.");

        logger = Logger;

        enable = Config.Bind("General", "Enable the mod", true, "Set to false to disable this mod.");
        quickStackWhitelist = Config.Bind("General", "Whitelist", "SimpleChest", "Comma-separated list of entities to quickstack into. Available values: " + knownEntities.Join());

        Harmony harmony = new Harmony("com.yourname.lumaisland.mod");
        harmony.PatchAll();

        Logger.LogInfo("Luma island first mod awake end.");
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
    static List<string> whitelistedNames = GetWhitelistedNames();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    static bool Prefix(Inventory __instance, IComparer<ItemStack> order, bool excludeHotbar = true)
    {
        if (!Keyboard.current.shiftKey.isPressed) return true; // run original method

        var player = __instance.Player;
        if (!FirstMod.GetPrivateField(__instance, "m_items", out ItemStack[] playerItems)) return true;
        if (!FirstMod.GetPrivateField(player.Level, "m_gameObjects", out TileOccupier[,] m_gameObjects)) return true;

        FirstMod.LogInfo("Patch_Inventory_Sort ForEach2");
        FirstMod.ForEach2(m_gameObjects, (obj, x, y) =>
        {
            if (!TryGetStorage(obj, out var storage)) return;

            var inventory = storage.Inventory;
            if (inventory == null) return;

            FirstMod.LogInfo("FOUND CHEST WITH INVENTORY!!");

            if (!FirstMod.GetPrivateField(inventory, "m_items", out ItemStack[] chestItems)) return;

            List<ItemStack> toRemove = new List<ItemStack>();
            foreach (ItemStack chestItem in chestItems.Where(item => item != null))
            {
                FirstMod.LogInfo($"{chestItem}: {chestItem.item} - {chestItem.amount}");

                foreach (var playerItem in playerItems.Where(item => item != null))
                {
                    FirstMod.LogInfo($"Comparing {playerItem.item.GetDescriptiveName()}");
                    if (playerItem.item == chestItem.item)
                    {
                        FirstMod.LogInfo($"Found MATCH!!!");
                        chestItem.amount += playerItem.amount;
                        playerItem.amount = 0;
                        toRemove.Add(playerItem);
                    }
                    else if (playerItem.item.GetDescriptiveName() == chestItem.item.GetDescriptiveName())
                    {
                        FirstMod.LogInfo($"Found GetDescriptiveName MATCH!!!");
                        chestItem.amount += playerItem.amount;
                        playerItem.amount = 0;
                        toRemove.Add(playerItem);
                    }
                }
            }

            foreach (var item in toRemove)
            {
                playerItems[Array.IndexOf(playerItems, item)] = null;
                //__instance.OnChange?.Invoke(item); // TODO:
            }
            __instance.OnInventoryUpdated?.Invoke();
            __instance.OnSorted?.Invoke();
        });

        FirstMod.LogInfo("Patch_Inventory_Sort finished");
        return true; // run original method
    }

    static bool TryGetStorage(TileOccupier occupier, out GenericStorageBox storage)
    {
        storage = null;
        if (occupier == null) return false;

        string name = GetOccupierName(occupier);

        if (!whitelistedNames.Contains(name)) return false;

        storage = occupier.GetComponent<GenericStorageBox>();
        if (storage == null && FirstMod.knownEntities.Contains(name))
        {
            FirstMod.LogInfo($"Known entity with name '{name}' doesn't have a GenericStorageBox. This shouldn't happen, please contact the mod author.");
            return false;
        }

        return true;
    }

    static HashSet<string> unknownWarningShown = new HashSet<string>();
    static List<string> GetWhitelistedNames()
    {
        List<string> names = FirstMod.quickStackWhitelist.Value.Split(',').Select(name => name.Trim()).Where(name => !name.IsNullOrEmpty()).ToList();

        foreach (string name in names)
        {
            if (!FirstMod.knownEntities.Contains(name) && !unknownWarningShown.Contains(name))
            {
                FirstMod.LogInfo($"Unknown entity: '{name}', known names are: " + FirstMod.knownEntities.Join());
                unknownWarningShown.Add(name);
            }
        }

        //FirstMod.LogInfo("NAMES: " + names.Join());

        return names;
    }

    static string GetOccupierName(TileOccupier occupier)
    {
        return occupier.name.Replace("(Clone)", "");
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
        ButtonWidget sortButton = (ButtonWidget)sortButtonField.GetValue(__instance);

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




