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
    internal static class PullGadgetDataPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(SavedGame), "Pull", new Type[] {typeof(GameModel), typeof(WorldV22)});
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            using (var code = instr.GetEnumerator())
            {
                while (code.MoveNext())
                {
                    var cur = code.Current;
                    if (cur.opcode == OpCodes.Callvirt && cur.operand is MethodInfo info && info.Name == "HasAttached")
                    {
                        yield return cur;
                        code.MoveNext();
                        yield return code.Current;
                        code.MoveNext();//skip newobj
                        yield return new CodeInstruction(OpCodes.Ldloca_S,(byte)22);
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(KeyValuePair<string, GadgetSiteModel>), "get_Value"));
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(PullGadgetDataPatch), "CreateGadgetData"));
                    }
                    else
                    {
                        yield return cur;
                    }
                }
            }
        }

        public static VanillaGadgetData CreateGadgetData(GadgetSiteModel model)
        {
            var mod = SaveRegistry.ModForModelType(model.attached.GetType());
            if (mod != null)
            {
                var info = SaveRegistry.GetSaveInfo(mod).GetRegistryFor<CustomGadgetData>();
                var newmodel = info.GetDataForID(info.GetIDForModel(model.attached.GetType()));
                newmodel.PullCustomModel(model.attached);
                return newmodel;
            }
            return new VanillaGadgetData();

        }
    }
}
