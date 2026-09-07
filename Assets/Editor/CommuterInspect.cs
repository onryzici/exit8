using System.Linq;
using UnityEngine;
using UnityEditor;
public static class CommuterInspect {
 public static void Run(){foreach(string p in new[]{"Assets/Commuter/Male_Adult_08.fbx","Assets/Commuter/m_walk_neutral.max.fbx"}){var g=AssetDatabase.LoadAssetAtPath<GameObject>(p);Debug.Log("MODEL "+p);foreach(var t in g.GetComponentsInChildren<Transform>(true))if(t.parent==g.transform||t.name.Contains("Hand")||t.name.Contains("Foot"))Debug.Log("NODE "+t.name+" parent="+t.parent.name+" p="+t.position+" scale="+t.lossyScale);foreach(var r in g.GetComponentsInChildren<SkinnedMeshRenderer>(true))Debug.Log("SKIN "+r.name+" bounds="+r.bounds+" materials="+string.Join(",",r.sharedMaterials.Select(m=>m?m.name:"null")));foreach(var c in AssetDatabase.LoadAllAssetsAtPath(p).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__"))){Debug.Log("CLIP "+c.name+" length="+c.length+" legacy="+c.legacy);foreach(var b in AnimationUtility.GetCurveBindings(c).Take(5))Debug.Log("CURVE "+b.path+" "+b.propertyName);}}}
}
