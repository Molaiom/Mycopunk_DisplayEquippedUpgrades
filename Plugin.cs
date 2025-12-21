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
		//Plugin setup
		Logger = base.Logger;
		Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
		Harmony.CreateAndPatchAll(typeof(DisplayEquippedUpgrades.Plugin));
	}

	[HarmonyPatch(typeof(GearUpgradeUI), nameof(GearUpgradeUI.UpdateBorder))]
	[HarmonyPostfix]
	public static void Main(ref bool __runOriginal, GearUpgradeUI __instance, ref bool isEquipped)
	{
		__runOriginal = true;

		try
		{
			UpgradeInstance upgrade = __instance?.Upgrade;
			ref RarityData rarity = ref Global.GetRarity(upgrade.Upgrade.Rarity);
			Color darkerRarityColor = Color.LerpUnclamped(rarity.color, rarity.backgroundColor, 0.25f);
			__instance?.border.color = new Color32(65, 65, 65, 255); // this color is hard coded in the original code

			if (upgrade == null)
				return;

			if (isEquipped)
			{
				__instance?.button.SetHoverColor(Color.LerpUnclamped(rarity.color, rarity.backgroundColor, 0.3f));
				__instance?.button.SetClickColor(darkerRarityColor, setGraphicComponentsOnThisObject: false);
				__instance?.button.SetDefaultColor(rarity.color);
				__instance?.button.mainGraphic.material = upgrade.IsTurbocharged ? Global.Instance.UITurbochargedMat : rarity.uiMat;
				__instance?.icon.color = !upgrade.Upgrade.IsSkin() ? rarity.backgroundColor : Global.Instance.WhiteUIColor;
				__instance?.icon.material = rarity.uiMat;
				__instance?.outline.color = rarity.color;
				__instance?.outline.material = upgrade.IsTurbocharged ? Global.Instance.UITurbochargedMat : rarity.uiMat;
				__instance?.rarityText.color = rarity.backgroundColor;
				__instance?.rarityText.outlineColor = rarity.backgroundColor;
				__instance?.rarityText.outlineWidth = 0.15f;
				__instance?.nameText.color = rarity.backgroundColor;
				__instance?.nameText.outlineColor = rarity.backgroundColor;
				__instance?.nameText.outlineWidth = 0.15f;
			}
			else
			{
				__instance?.button.SetDefaultColor(rarity.backgroundColor);
				__instance?.icon.material = (upgrade.IsTurbocharged ? Global.Instance.UITurbochargedMat : rarity.uiMat);
				__instance?.icon.color = rarity.color;
				__instance?.outline.material = rarity.uiMat;
				__instance?.rarityText.color = rarity.color;
				__instance?.rarityText.outlineWidth = 0;
				__instance?.nameText.color = Global.Instance.WhiteUIColor;
				__instance?.nameText.outlineWidth = 0;
			}
		}
		catch (Exception e)
		{
			Logger.LogWarning($"Method: SetUpgrade | {e.Message}");
		}
	}

	[HarmonyPatch(typeof(GearUpgradeUI), nameof(GearUpgradeUI.EnableGridView))]
	[HarmonyPostfix]
	public static void EnableGridView(ref bool __runOriginal, GearUpgradeUI __instance)
	{
		//I have to overwrite this method because the icon color is changed here directly
		__runOriginal = true;
		__instance?.SetUpgrade(__instance.Upgrade);

	}

	[HarmonyPatch(typeof(OuroUpgradeUI), nameof(OuroUpgradeUI.ExcludeFromPool))]
	[HarmonyPostfix]
	public static void ExcludeFromPool(ref bool __runOriginal, ref string source, OuroUpgradeUI __instance)
	{
		//I have to overwrite this method because a lot of visuals are changed here directly
		__runOriginal = false;
		__instance?.excludeFromPool = source;
	}
}