using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using MonomiPark.SlimeRancher;
using MonomiPark.SlimeRancher.DataModel;
using SRML.SR.SaveSystem.Data.Actor;
using UnityEngine;
using VanillaActorData = MonomiPark.SlimeRancher.Persist.ActorDataV09;
namespace SRML.SR.SaveSystem.Patches
{
    [HarmonyPatch]
    internal static class PushActorDataPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(SavedGame), "PushActorData");
        }


        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            using (var code = instr.GetEnumerator())
            {
                while (code.MoveNext())
                {
                    var cur = code.Current;
                    if (cur.opcode == OpCodes.Callvirt && cur.operand is MethodInfo info &&
                        info.Name == "GetActorModel")
                    {
                        yield return cur;
                        code.MoveNext();
                        yield return code.Current;
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(PushActorDataPatch), "CheckModel"));
                    }
                    else
                    {
                        yield return cur;
                    }
                }
            }
        }

        public static void CheckModel(ActorModel model, VanillaActorData data)
        {
            
            if (data is CustomActorData customData)
            {
                customData.PushCustomModel(model);
            }

            foreach(var v in DataModelRegistry.actorLoaders.Where(x => x.Key(model)))
            {
                v.Value(model, data);
            }
        }

    }
}
