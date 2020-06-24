using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using SRML.SR.SaveSystem.Format;
using UnityEngine;

namespace SRML.SR.SaveSystem.Patches
{
    [HarmonyPatch]
    // Need to get rid of 4 instructions after savedgame load 
    internal static class LoadFallBackSavePatch
    {
        private static Type targetType = typeof(AutoSaveDirector).GetNestedTypes(BindingFlags.NonPublic)
            .First((x) => x.Name == "<LoadFallbackSave_Coroutine>d__70");

        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(targetType,"MoveNext");
        }

        public static void LoadModSave(AutoSaveDirector director, GameData.Summary summary)
        {
            SaveHandler.LoadModdedSave(director,summary.saveName);
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            using (var code = instructions.GetEnumerator())
            {
                while (code.MoveNext())
                {
                    var cur = code.Current;
                    if (cur.opcode == OpCodes.Callvirt && cur.operand is MethodInfo info && info.Name == "Load")
                    {
                        yield return cur;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);

                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(targetType,"<>4__this"));
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(targetType,"<summary>5__5"));
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(LoadFallBackSavePatch), "LoadModSave"));
                    }
                    else
                    {
                        yield return cur;
                    }
                }
            }
        }
    }
}
