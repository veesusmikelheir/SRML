using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SRML.SR.Patches
{
    [HarmonyPatch]
    internal static class SRInputConstructorPatch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Constructor(typeof(SRInput));
        }
        public static void Postfix(SRInput __instance)
        {
            foreach (var v in BindingRegistry.precreators) v(__instance.actions);
        }
    }
}
