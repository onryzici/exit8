using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public static class StationLightingCorrection {
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");
  // Restore the approved V3 exposure, light energy and reflection pipeline.
  foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){
   if(l.name=="Baked rectangular light"){l.intensity=8;l.areaSize=new Vector2(1.28f,.24f);l.color=new Color(.97f,.98f,1);}
   if(l.name=="Realtime specular support"){l.intensity=.85f;l.range=6;l.color=new Color(.96f,.98f,1);}
   if(l.name.StartsWith("Diffuse floor return"))l.intensity=.12f;
  }
  var p=AssetDatabase.LoadAssetAtPath<PostProcessProfile>("Assets/ArtV2/StationPhotography.asset");
  p.GetSetting<ColorGrading>().postExposure.Override(-.35f);p.GetSetting<ColorGrading>().temperature.Override(-1);p.GetSetting<ColorGrading>().contrast.Override(12);p.GetSetting<ColorGrading>().saturation.Override(-5);
  p.GetSetting<Bloom>().intensity.Override(.20f);p.GetSetting<Bloom>().threshold.Override(1.4f);p.GetSetting<ScreenSpaceReflections>().enabled.Override(false);
  var wall=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Glazed ceramic 300mm.mat");wall.SetFloat("_Gloss",.88f);wall.SetFloat("_RoughnessWeight",.13f);wall.SetFloat("_MicroStrength",.42f);
  var floor=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Fine aggregate porcelain 450mm.mat");floor.SetFloat("_Gloss",.64f);floor.SetFloat("_RoughnessWeight",.28f);floor.SetFloat("_MicroStrength",.22f);
  foreach(var r in UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)){r.blendDistance=r.name.StartsWith("Local reflection")?1.3f:1;}
  var tube=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV4/T8 phosphor glass.mat");tube.SetColor("_EmissionColor",new Color(.88f,.94f,1)*7);
  // Keep the raised ceiling; use a restrained satin finish and shallower folded seams.
  var ceiling=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV3/Satin enamel aluminium.mat");ceiling.color=new Color(.70f,.705f,.69f);ceiling.SetFloat("_Metallic",0);ceiling.SetFloat("_Glossiness",.34f);ceiling.SetFloat("_BumpScale",.035f);
  var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/ArtV3/CeilingProfile.asset");var vs=mesh.vertices;
  for(int i=0;i<vs.Length;i++){if(vs[i].y<0)vs[i].y*=.3f;}mesh.vertices=vs;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
  Save();if(!Lightmapping.Bake())throw new Exception("Lighting correction bake failed");Save();
  StationV4.Capture();foreach(string name in new[]{"corridor","door","fixture"})File.Copy("Screenshots/"+name+"-v4.png","Screenshots/"+name+"-corrected.png",true);
  BuildStation.BuildPlayer();Debug.Log("STATION_LIGHTING_CORRECTION_COMPLETE");
 }
 static void Save(){foreach(var folder in new[]{"Assets/ArtV2","Assets/ArtV3","Assets/ArtV4"})foreach(var guid in AssetDatabase.FindAssets("",new[]{folder}))foreach(var a in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)))if(a)EditorUtility.SetDirty(a);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
}
