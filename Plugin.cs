using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Pigeon.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DisplayEquippedUpgrades;

[MycoMod(null, ModFlags.IsClientSide)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;

    private void Awake()
    {
        ///Plugin setup
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Harmony.CreateAndPatchAll(typeof(DisplayEquippedUpgrades.Plugin));
    }

    ///<summary>Happens when the player opens the equipment menu of a gear or character</summary>
    [HarmonyPatch(typeof(GearUpgradeUI), nameof(GearUpgradeUI.SetUpgrade))]
    [HarmonyPostfix]
    public static void GearUpgradeUISetUpgrade(ref bool __runOriginal, GearUpgradeUI __instance, ref UpgradeInstance upgrade)
    {
        __runOriginal = true;

        try
        {
            ref RarityData rarity = ref Global.GetRarity(upgrade.Upgrade.Rarity);
            Color darkerRarityColor = Color.LerpUnclamped(rarity.color, rarity.backgroundColor, 0.25f);

            // If upgrade equipped
            if (PlayerData.GetGearData(__instance.transform.GetComponentInParent<GearDetailsWindow>().UpgradablePrefab).IsUpgradeEquipped(upgrade))
            {
                __instance.rarityText.color = rarity.backgroundColor;
                __instance.rarityText.outlineColor = rarity.backgroundColor;
                __instance.rarityText.outlineWidth = 0.15f;
                __instance.nameText.color = rarity.backgroundColor;
                __instance.nameText.outlineColor = rarity.backgroundColor;
                __instance.nameText.outlineWidth = 0.15f;
                __instance.button.SetHoverColor(Color.LerpUnclamped(rarity.color, rarity.backgroundColor, 0.3f));
                __instance.button.SetClickColor(darkerRarityColor, setGraphicComponentsOnThisObject: false);
                __instance.button.SetDefaultColor(rarity.color);
                __instance.button.mainGraphic.material = upgrade.IsTurbocharged ? Global.Instance.UITurbochargedMat : null;
                __instance.icon.color = !upgrade.Upgrade.IsSkin() ? rarity.backgroundColor : Global.Instance.WhiteUIColor;
                __instance.icon.material = null;
                __instance.outline.color = rarity.color;
            }
            else
            {
                __instance.button.SetDefaultColor(rarity.backgroundColor);
                __instance.icon.material = (upgrade.IsTurbocharged ? Global.Instance.UITurbochargedMat : null);
                __instance.rarityText.color = rarity.color;
                __instance.rarityText.outlineWidth = 0;
                __instance.nameText.color = Global.Instance.WhiteUIColor;
                __instance.nameText.outlineWidth = 0;
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning($"Method: SetUpgrade | {e.Message}");
        }
    }

    ///<summary>I have to overwrite this method because the icon color is changed here</summary>
    [HarmonyPatch(typeof(GearUpgradeUI), nameof(GearUpgradeUI.EnableGridView))]
    [HarmonyPostfix]
    public static void GearUpgradeUIEnableGridView(ref bool __runOriginal, GearUpgradeUI __instance)
    {
        __runOriginal = true;
        __instance.SetUpgrade(__instance.Upgrade);
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
                    gearUpgradeUI.SetUpgrade(upgrade);
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning($"Method: EquipUpgrade | {e.Message}");
        }
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
                    gearUpgradeUI.SetUpgrade(upgrade);
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning($"Method: UnequipUpgrade | {e.Message}");
        }
    }

}