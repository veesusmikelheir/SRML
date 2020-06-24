using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using MonomiPark.SlimeRancher;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Persist;
using SRML.SR.SaveSystem.Data.Gadget;
using UnityEngine;
using VanillaGadgetData = MonomiPark.SlimeRancher.Persist.PlacedGadgetV08;
namespace SRML.SR.SaveSystem.Patches
{
    [HarmonyPatch]
    internal static class PushGadgetPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(SavedGame), "Push",
                new Type[] {typeof(GameModel), typeof(VanillaGadgetData), typeof(GadgetSiteModel)});
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            using (var code = instr.GetEnumerator())
            {
                while (code.MoveNext())
                {
                    var cur = code.Current;
                    if (cur.opcode == OpCodes.Ldfld && cur.operand is FieldInfo info &&
                        info.Name=="attached")
                    {
                        yield return cur;
                        code.MoveNext();
                        yield return code.Current;
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(PushGadgetPatch),"CheckModel"));

                    }
                    else
                    {
                        yield return cur;
                    }
                }
            }
        }

        public static void CheckModel(GadgetModel model, VanillaGadgetData data)
        {
            if (data is CustomGadgetData custom)
            {
                custom.PushCustomModel(model);
            }
        }
    }
}
