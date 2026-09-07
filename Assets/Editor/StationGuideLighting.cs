using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
public static class StationGuideLighting {
 public static void AnchorFix(){
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");
  foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.name=="Tactile route tile")){
   var anchor=new GameObject("Guide light sample").transform;anchor.SetParent(r.transform,false);anchor.localPosition=new Vector3(0,1.18f,0);r.probeAnchor=anchor;
  }
  LightProbes.Tetrahedralize();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();BuildCorridorLoop.Export();Debug.Log("GUIDE_ANCHOR_FIXED");
 }
 public static void Run(){
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");var loop=Object.FindFirstObjectByType<CorridorLoop>();var look=Object.FindFirstObjectByType<StationLookControls>();
  var surfaces=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
  var faces=surfaces.Where(r=>r.name=="Bilingual exit face"||r.name=="Rear printed acrylic face"||r.name=="Entry progress").Concat(loop.numberSigns.Where(r=>r&&r.name=="Final number eight")).Distinct().ToArray();
  var print=new Material(Shader.Find("Standard")){name="Unified exit progress print",color=Color.white,mainTexture=loop.numberTextures[0]};
  print.SetTexture("_EmissionMap",loop.numberTextures[0]);print.EnableKeyword("_EMISSION");print.SetColor("_EmissionColor",Color.white*look.signEmission);print.SetFloat("_Glossiness",.25f);print.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;
  AssetDatabase.CreateAsset(print,AssetDatabase.GenerateUniqueAssetPath("Assets/Loop/UnifiedExit.mat"));foreach(var r in faces)r.sharedMaterial=print;loop.numberSigns=faces;look.sign=print;
  var guides=surfaces.Where(r=>r.name=="Tactile route tile").ToArray();var rubber=new Material(guides[0].sharedMaterial){name="Matte tactile rubber"};rubber.SetFloat("_Glossiness",.16f);rubber.SetFloat("_Metallic",0);rubber.SetColor("_Color",new Color(.65f,.48f,.16f));AssetDatabase.CreateAsset(rubber,AssetDatabase.GenerateUniqueAssetPath("Assets/Loop/MatteTactile.mat"));
  foreach(var r in guides){r.sharedMaterial=rubber;GameObjectUtility.SetStaticEditorFlags(r.gameObject,StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic);((MeshRenderer)r).receiveGI=ReceiveGI.LightProbes;r.lightProbeUsage=LightProbeUsage.BlendProbes;r.shadowCastingMode=ShadowCastingMode.Off;}
  var old=GameObject.Find("Tactile route lighting samples");if(old)Object.DestroyImmediate(old);var probes=new GameObject("Tactile route lighting samples").AddComponent<LightProbeGroup>();
  var samples=new List<Vector3>();foreach(int cell in new[]{-1,0,1}){var offset=new Vector3(cell*12.4f,0,cell*26.6f);for(float z=-.3f;z<26.7f;z+=1.2f)foreach(float y in new[]{.20f,1.2f})samples.Add(offset+new Vector3(0,y,z));foreach(int side in new[]{-1,1})for(float x=1.2f;x<6.2f;x+=1.2f)foreach(float y in new[]{.20f,1.2f})samples.Add(offset+new Vector3(side*x,y,side<0?-.3f:26.3f));}probes.probePositions=samples.ToArray();
  look.Apply();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  if(!Lightmapping.Bake())throw new Exception("Guide lighting bake failed");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();BuildCorridorLoop.Export();Debug.Log("GUIDE_LIGHTING_SIGN_FIX_READY faces="+faces.Length);
 }
}
