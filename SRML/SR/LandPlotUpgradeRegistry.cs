using SRML.SR.SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using static PurchaseUI;

namespace SRML.SR
{
    public static class LandPlotUpgradeRegistry
    {
        internal static IDRegistry<LandPlot.Upgrade> moddedUpgrades = new IDRegistry<LandPlot.Upgrade>();

        private static string DemolishKey = MessageUtil.Qualify("ui", "l.demolish_plot");
        private static string ClearCropKey = MessageUtil.Qualify("ui", "b.clear_crop");

        static LandPlotUpgradeRegistry()
        {
            ModdedIDRegistry.RegisterIDRegistry(moddedUpgrades);
            EnumPatcher.RegisterAlternate(typeof(LandPlot.Upgrade), (obj, name) => CreateLandPlotUpgrade(obj, name));
            PurchasableUIRegistry.RegisterManipulator((LandPlotUI ui, ref Purchasable[] purchasables) =>
            {
                List<Purchasable> purchasables1 = purchasables.ToList();
                Purchasable Demolish = purchasables1.FirstOrDefault(match => match.nameKey == DemolishKey);
                if (Demolish == null) return;
                purchasables1.Remove(Demolish);
                Purchasable ClearCrop = purchasables1.FirstOrDefault(match => match.nameKey == ClearCropKey);
                if (ClearCrop != null)
                {
                    purchasables1.Remove(ClearCrop);
                    purchasables1.Add(ClearCrop);
                }
                purchasables1.Add(Demolish);
                purchasables = purchasables1.ToArray();
            });
        }

        public static LandPlot.Upgrade CreateLandPlotUpgrade(object value, string name)
        {
            if (SRModLoader.CurrentLoadingStep > SRModLoader.LoadingStep.PRELOAD)
                throw new Exception("Can't register landplot upgrades outside of the PreLoad step");
            return moddedUpgrades.RegisterValueWithEnum((LandPlot.Upgrade)value, name);
        }

        public static void RegisterPurchasableUpgrade<T>(UpgradeShopEntry entry) where T : LandPlotUI
        {
            PurchasableUIRegistry.RegisterPurchasable<T>((x) => new Purchasable(entry.NameKey,entry.icon,entry.mainImg,entry.DescKey,entry.cost,entry.landplotPediaId,()=> {
                x.Upgrade(entry.upgrade, entry.cost);
            }, entry.isUnlocked != null ? (Func<bool>)(() => entry.isUnlocked(x.activator)) : (() => true), entry.isAvailable != null ? (Func<bool>)(() => entry.isAvailable(x.activator)) : (() => !x.activator.HasUpgrade(entry.upgrade)), null, entry.warning ?? null, null, null, entry.holdtopurchase));
        }

        public static void RegisterPlotUpgrader<T>(LandPlot.Id plot) where T : PlotUpgrader
        {
            switch (SRModLoader.CurrentLoadingStep)
            {
                case SRModLoader.LoadingStep.PRELOAD:
                    SRCallbacks.OnGameContextReady += () =>
                    {
                        GameContext.Instance.LookupDirector.GetPlotPrefab(plot).AddComponent<T>();
                    };
                    break;
                default:
                    GameContext.Instance.LookupDirector.GetPlotPrefab(plot).AddComponent<T>();
                    break;
            }
        }

        public struct UpgradeShopEntry
        {
            public LandPlot.Upgrade upgrade;
            public Sprite icon;
            public Sprite mainImg;
            public int cost;
            public PediaDirector.Id landplotPediaId;
            public Func<LandPlot, bool> isUnlocked;
            public Func<LandPlot, bool> isAvailable;
            public string warning;
            public bool holdtopurchase;

            string landplotName;

            public string LandPlotName
            {
                get => landplotName != null ? landplotName : this.landplotPediaId.ToString().ToLower();
                set => landplotName = value.ToLower();
            }

            public string DescKey => $"m.upgrade.desc.{landplotName}.{upgrade.ToString().ToLower()}";
            public string NameKey => $"m.upgrade.name.{landplotName}.{upgrade.ToString().ToLower()}";
        }
    }

    [HarmonyPatch(typeof(UITemplates))]
    [HarmonyPatch("CreatePurchaseUI")]
    internal static class Patch_LandPlot
    {

        private static List<PediaDirector.Id> ValidPedias = new List<PediaDirector.Id>
        {
            PediaDirector.Id.COOP,
            PediaDirector.Id.CORRAL,
            PediaDirector.Id.GARDEN,
            PediaDirector.Id.INCINERATOR,
            PediaDirector.Id.POND,
            PediaDirector.Id.SILO
        };

        private static bool IsPedia(string titleKey)
        {
            string titleNoT = titleKey.Replace("t.", "");
            foreach (PediaDirector.Id id in ValidPedias)
                if (id.ToString().ToLower() == titleNoT)
                    return true;
            return false;
        }

        private static void Prefix(UITemplates __instance, string titleKey, ref Purchasable[] purchasables)
        {
        }
    }
}
