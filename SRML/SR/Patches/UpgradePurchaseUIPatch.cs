using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace SRML.SR.Patches
{
    [HarmonyPatch]
    internal static class UpgradePurchaseUIPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(PersonalUpgradeUI), "CreatePurchaseUI");
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            foreach (var v in instr)
            {
                if (v.opcode == OpCodes.Call && v.operand is MethodInfo info && info.Name == "get_Instance")
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S,(byte)0);
                    yield return new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(UpgradePurchaseUIPatch),"UpgradeFix"));
                    yield return v;
                }
                else
                {
                    yield return v;
                }
            }
        }

        static void UpgradeFix(PersonalUpgradeUI ui, ref PurchaseUI.Purchasable[] purchasables)
        {
            purchasables = purchasables.AddRangeToArray(PersonalUpgradeRegistry.moddedUpgrades.Select((x)=>ui.CreateUpgradePurchasable(x.Key)).ToArray());
        }
    }
}
