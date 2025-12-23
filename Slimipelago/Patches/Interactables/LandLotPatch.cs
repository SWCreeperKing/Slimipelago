using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using Slimipelago.Patches.PlayerPatches;
using UnityEngine;

namespace Slimipelago.Patches.Interactables;

[PatchAll]
public static class CorralPatch
{
    [HarmonyPatch(typeof(SlimeFeeder), "ProcessFeedOperation"), HarmonyPrefix]
    public static bool Feeder(SlimeFeeder __instance, bool ejectFood)
    {
        try
        {
            var storage = __instance.GetPrivateField<SiloStorage>("storage");
            var relevantAmmo = storage.GetRelevantAmmo();
            relevantAmmo.SetAmmoSlot(0);
            if (relevantAmmo.HasSelectedAmmo())
            {
                if (ejectFood)
                {
                    __instance.CallPrivateMethod("EjectFood", relevantAmmo);
                }

                var slot = relevantAmmo.GetPrivateField<AmmoModel>("ammoModel")
                                       .slots[relevantAmmo.GetPrivateField<int>("selectedAmmoIdx")];
                slot.count = Math.Max(slot.count - 1, 0);
            }

            var model = __instance.GetPrivateField<LandPlotModel>("model");
            model.remainingFeedOperations = Math.Max(0, model.remainingFeedOperations - 1);
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }

        return false;
    }

    [HarmonyPatch(typeof(CorralUI), "AllowDemolish"), HarmonyPrefix]
    public static bool AllowDemolish(ref bool __result)
    {
        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(CorralUI), "Demolish"), HarmonyPrefix]
    public static bool Demolish(CorralUI __instance)
    {
        RefundBuilding(__instance, __instance.demolish.plotPrefab,
        [
            __instance.walls, __instance.musicBox, __instance.airNet, __instance.solarShield, __instance.plortCollector,
            __instance.feeder
        ], 250);
        return false;
    }

    [HarmonyPatch(typeof(AirNet), "OnCollisionEnter"), HarmonyPrefix]
    public static bool AirNet() => false;

    [HarmonyPatch(typeof(GardenUI), "Demolish"), HarmonyPrefix]
    public static bool Demolish(GardenUI __instance)
    {
        RefundBuilding(__instance, __instance.demolish.plotPrefab,
            [__instance.soil, __instance.sprinkler, __instance.scareslime, __instance.miracleMix, __instance.deluxe],
            250);
        return false;
    }

    [HarmonyPatch(typeof(CoopUI), "Demolish"), HarmonyPrefix]
    public static bool Demolish(CoopUI __instance)
    {
        RefundBuilding(__instance, __instance.demolish.plotPrefab,
            [__instance.walls, __instance.feeder, __instance.vitamizer, __instance.deluxe], 250);
        return false;
    }

    [HarmonyPatch(typeof(SiloUI), "Demolish"), HarmonyPrefix]
    public static bool Demolish(SiloUI __instance)
    {
        RefundBuilding(__instance, __instance.demolish.plotPrefab,
            [__instance.storage2, __instance.storage3, __instance.storage4], 450);
        return false;
    }

    [HarmonyPatch(typeof(IncineratorUI), "Demolish"), HarmonyPrefix]
    public static bool Demolish(IncineratorUI __instance)
    {
        RefundBuilding(__instance, __instance.demolish.plotPrefab, [__instance.ashTrough], 450);
        return false;
    }

    [HarmonyPatch(typeof(PondUI), "Demolish"), HarmonyPrefix]
    public static bool Demolish(PondUI __instance)
    {
        RefundBuilding(__instance, __instance.demolish.plotPrefab, [], 450);
        return false;
    }

    public static void RefundBuilding(LandPlotUI plotUI, GameObject prefab, LandPlotUI.UpgradePurchaseItem[] items,
        int basePrice)
    {
        plotUI.CallPrivateMethod("Replace", prefab);
        var upgrader = plotUI.GetPrivateField<LandPlot>("activator");
        foreach (var upgrade in items)
        {
            if (!upgrader.HasUpgrade(upgrade.upgrade)) continue;
            PlayerStatePatch.PlayerState.AddCurrency(upgrade.cost);
        }

        PlayerStatePatch.PlayerState.AddCurrency(basePrice); // coral price
    }
}