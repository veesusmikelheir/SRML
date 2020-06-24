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
using SRML.SR.SaveSystem.Data.LandPlot;
using UnityEngine;
using VanillaLandPlotData = MonomiPark.SlimeRancher.Persist.LandPlotV08;
namespace SRML.SR.SaveSystem.Patches
{
    [HarmonyPatch]
    internal static class PullLandPlotDataPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(SavedGame), "Pull", new Type[] { typeof(GameModel), typeof(RanchV07) });
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            var instrList = instr.ToList();
            foreach (var v in instrList)    
            {
                if (v.opcode == OpCodes.Newobj && v.operand is ConstructorInfo con &&
                    con.DeclaringType == typeof(VanillaLandPlotData))
                {
                    var prevInstr = instrList[instrList.IndexOf(v) - 1];
                    yield return new CodeInstruction(OpCodes.Ldloca_S, prevInstr.operand);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(KeyValuePair<string, LandPlotModel>), "get_Value"));
                    yield return new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(PullLandPlotDataPatch),"CreateLandPlotData"));
                }
                else
                {
                    yield return v;
                }
            }
        }

        public static VanillaLandPlotData CreateLandPlotData(LandPlotModel model)
        {
            var mod = SaveRegistry.ModForModelType(model.GetType());
            if (mod != null)
            {
                var info = SaveRegistry.GetSaveInfo(mod).GetRegistryFor<CustomLandPlotData>();
                var newmodel = info.GetDataForID(info.GetIDForModel(model.GetType()));
                newmodel.PullCustomModel(model);
                return newmodel;
            }
            return new VanillaLandPlotData();
        }
    }
}
