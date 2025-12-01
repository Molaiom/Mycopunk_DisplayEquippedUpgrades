using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace DisplayEquippedUpgrades;

[MycoMod(null, ModFlags.IsClientSide)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    static string pluginImageName = "equippedUpgradeImgObj";

    private void Awake()
    {
        ///Plugin setup
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Harmony.CreateAndPatchAll(typeof(DisplayEquippedUpgrades.Plugin));
    }

    ///<summary>Happens when the player opens the equipment menu of a weapon/grenade or character</summary>
    [HarmonyPatch(typeof(GearUpgradeUI), nameof(GearUpgradeUI.SetUpgrade))]
    [HarmonyPrefix]
    public static void GearUpgradeUISetUpgrade(ref bool __runOriginal, ref UpgradeInstance upgrade, GearUpgradeUI __instance)
    {
        __runOriginal = true;

        try
        {
            //If the upgrade is equipped
            if (PlayerData.GetGearData(__instance.transform.GetComponentInParent<GearDetailsWindow>().UpgradablePrefab).IsUpgradeEquipped(upgrade))
            {
                TryCreateImage(__instance.transform);
            }
            //If the upgrade is not equipped
            else
            {
                TryHideImage(__instance.transform);
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning(e.Message);
        }
    }

    ///<summary>Happens when an upgrade is equipped</summary>
    [HarmonyPatch(typeof(GearDetailsWindow), nameof(GearDetailsWindow.EquipUpgrade))]
    [HarmonyPostfix]
    public static void GearDetailsWindowEquipUpgrade(ref bool __runOriginal, ref UpgradeInstance upgrade, GearDetailsWindow __instance)
    {
        __runOriginal = true;

        try
        {
            foreach (var gearUpgradeUI in __instance.transform.GetComponentsInChildren<GearUpgradeUI>())
            {
                if (gearUpgradeUI.Upgrade == upgrade)
                {
                    TryCreateImage(gearUpgradeUI.transform);
                    return;
                }
            }
        }
        catch { }
    }

    ///<summary>Happens when an upgrade is unequipped</summary>
    [HarmonyPatch(typeof(GearDetailsWindow), nameof(GearDetailsWindow.UnequipUpgrade))]
    [HarmonyPostfix]
    public static void GearDetailsWindowUnequipUpgrade(ref bool __runOriginal, ref UpgradeInstance upgrade, GearDetailsWindow __instance)
    {
        __runOriginal = true;

        try
        {
            foreach (var gearUpgradeUI in __instance.transform.GetComponentsInChildren<GearUpgradeUI>())
            {
                if (gearUpgradeUI.Upgrade == upgrade)
                {
                    TryHideImage(gearUpgradeUI.transform);
                    return;
                }
            }
        }
        catch { }
    }

    ///<summary>Creates an image that makes the upgrade icon darker</summary>
    static void TryCreateImage(Transform transform)
    {
        //If there is already an image, simply re enable it
        if (GetModImageObj(transform).TryGetComponent(out Image image))
        {
            image.enabled = true;
            return;
        }

        // If there is no image, create one
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
        // If there is no image, do nothing
        if (!GetModImageObj(transform).TryGetComponent(out Image image))
            return;

        // If there is an image, disable it
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