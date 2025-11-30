using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static PlayerData;

namespace DisplayEquippedUpgrades;

[MycoMod(null, ModFlags.IsClientSide)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    static string pluginImageName = "equippedUpgradeImgObj";

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Harmony.CreateAndPatchAll(typeof(DisplayEquippedUpgrades.Plugin));
    }

    [HarmonyPatch(typeof(GearUpgradeUI), nameof(GearUpgradeUI.SetUpgrade))]
    [HarmonyPrefix]
    public static void GearUpgradeUISetUpgrade(ref bool __runOriginal, ref UpgradeInstance upgrade, GearUpgradeUI __instance)
    {
        __runOriginal = true;

        try
        {
            Logger.LogInfo($"Upgrade: [{upgrade.Upgrade.name}] Gear: [{upgrade.Gear}] Id: [{upgrade.upgradeID}]");

            if (PlayerData.GetGearData(__instance.transform.GetComponentInParent<GearDetailsWindow>().UpgradablePrefab).IsUpgradeEquipped(upgrade))
            {
                Logger.LogInfo(" is equipped");
                TryCreateImage(__instance.transform);
            }
            else
            {
                Logger.LogInfo(" is NOT equipped");
                TryHideImage(__instance.transform);
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning(e.Message);
        }
    }

    [HarmonyPatch(typeof(GearDetailsWindow), nameof(GearDetailsWindow.EquipUpgrade))]
    [HarmonyPostfix]
    public static void GearDetailsWindowEquipUpgrade(ref bool __runOriginal, ref UpgradeInstance upgrade, GearDetailsWindow __instance)
    {
        __runOriginal = true;

        try
        {
            __instance.transform.GetComponentInChildren<GearUpgradeUI>().SetUpgrade(upgrade);
        }
        catch { }
    }

    [HarmonyPatch(typeof(GearDetailsWindow), nameof(GearDetailsWindow.UnequipUpgrade))]
    [HarmonyPostfix]
    public static void GearDetailsWindowUnequipUpgrade(ref bool __runOriginal, ref UpgradeInstance upgrade, GearDetailsWindow __instance)
    {
        __runOriginal = true;

        try
        {
            __instance.transform.GetComponentInChildren<GearUpgradeUI>().SetUpgrade(upgrade);
        }
        catch { }
    }

    ///<summary>Creates an image that makes the upgrade icon darker</summary>
    static void TryCreateImage(Transform transform)
    {
        if (GetModImageObj(transform).TryGetComponent(out Image image))
        {
            image.enabled = true;
            return;
        }

        image = Instantiate(new GameObject().AddComponent<Image>(), transform);
        image.gameObject.name = pluginImageName;
        image.rectTransform.anchoredPosition = Vector2.zero;
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one;
        image.rectTransform.sizeDelta = Vector2.zero;
        image.raycastTarget = false;
        image.color = new Color(0, 0, 0, 0.95f);
        image.enabled = true;
    }

    ///<summary>Disables the image that makes the upgrade icon darker</summary>
    static void TryHideImage(Transform transform)
    {
        if (!GetModImageObj(transform).TryGetComponent(out Image image))
            return;

        image.enabled = false;
    }

    /// <summary>Finds the child object containing the image created by this plugin</summary>
    static GameObject GetModImageObj(Transform transform)
    {
        try
        {
            return transform.Find(pluginImageName).gameObject;
        }
        catch { return new GameObject(); }
    }
}