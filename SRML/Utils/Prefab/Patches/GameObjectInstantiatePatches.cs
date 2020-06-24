using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SRML.Utils.Prefab.Patches
{

    internal static class GameObjectInstantiate
    {
        public static void OnInstantiate(GameObject obj)
        {
            foreach (var prefab in obj.GetComponentsInChildren<RuntimePrefab>())
            {
                
                if (!prefab) return;
                if (prefab.ShouldEnableOnInstantiate) prefab.gameObject.SetActive(true);
                MonoBehaviour.Destroy(prefab);
            }
        }
    }
    [HarmonyPatch]
    internal static class InstantiateGenericPatch
    {
        public static MethodInfo TargetMethod()
        {
            return typeof(UnityEngine.Object).GetMethods().First((x) =>x.Name == "Instantiate" && x.ContainsGenericParameters && x.GetParameters().Length == 1).MakeGenericMethod(typeof(UnityEngine.Object));
        }

        public static void Postfix(UnityEngine.Object original, UnityEngine.Object __result)
        {
            if (__result is GameObject obj) GameObjectInstantiate.OnInstantiate(obj);
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object))]
    [HarmonyPatch("Instantiate",new Type[] { typeof(UnityEngine.Object)})]
    internal static class GameObjectInstantiatePatch1
    {
        public static void Postfix(UnityEngine.Object original,UnityEngine.Object __result)
        {
            if (__result is GameObject obj) GameObjectInstantiate.OnInstantiate(obj);
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object))]
    [HarmonyPatch("Instantiate", new Type[] { typeof(UnityEngine.Object),typeof(Transform)})]
    internal static class GameObjectInstantiatePatch2
    {
        public static void Postfix(UnityEngine.Object original, UnityEngine.Object __result)
        {
            if (__result is GameObject obj) GameObjectInstantiate.OnInstantiate(obj);
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object))]
    [HarmonyPatch("Instantiate", new Type[] { typeof(UnityEngine.Object), typeof(Transform),typeof(bool)})]
    internal static class GameObjectInstantiatePatch3
    {
        public static void Postfix(UnityEngine.Object original, UnityEngine.Object __result)
        {
            if (__result is GameObject obj) GameObjectInstantiate.OnInstantiate(obj);
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object))]
    [HarmonyPatch("Instantiate", new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
    internal static class GameObjectInstantiatePatch4
    {
        public static void Postfix(UnityEngine.Object original, UnityEngine.Object __result)
        {
            if (__result is GameObject obj) GameObjectInstantiate.OnInstantiate(obj);
        }
    }
    
    [HarmonyPatch(typeof(UnityEngine.Object))]
    [HarmonyPatch("Instantiate", new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
    internal static class GameObjectInstantiatePatch5
    {
        public static void Postfix(UnityEngine.Object original, UnityEngine.Object __result)
        {
            if (__result is GameObject obj) GameObjectInstantiate.OnInstantiate(obj);
        }
    }
}
