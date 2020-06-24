using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using SRML.Utils;
using UnityEngine;

namespace SRML.SR.SaveSystem.Patches
{
    [HarmonyPatch]
    internal static class LoadSavePatch
    {
        private static Type targetType = typeof(AutoSaveDirector).GetNestedTypes(BindingFlags.NonPublic)
            .First((x) => x.Name == "<LoadSave_Coroutine>d__68");

        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(targetType,"MoveNext");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            foreach (var v in instr)
            {
                if (v.opcode == OpCodes.Callvirt&&v.operand is MethodInfo info&&info.Name == "Load")
                {
                    yield return v;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(targetType, "<>4__this"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(targetType, "gameName"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(targetType, "saveName"));
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(LoadSavePatch), "LoadModSave"));
                }   
                else
                {
                    yield return v;
                }
            }
        }

        public static void LoadModSave(AutoSaveDirector director, string gameName, string saveName)
        {
            SaveHandler.LoadModdedSave(director,saveName);
        }
    }
}
