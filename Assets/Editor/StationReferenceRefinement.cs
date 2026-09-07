using System;
using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
public static class StationReferenceRefinement {
 const string Root="Assets/StationRefinement/";
 static Material Mat(string name,Color color,float metallic,float gloss){var m=new Material(Shader.Find("Standard"));m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",gloss);m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ArtV3/GlazeNormal.png"));m.SetFloat("_BumpScale",.04f);m.EnableKeyword("_NORMALMAP");AssetDatabase.CreateAsset(m,Root+name+".mat");return m;}
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");
  var root=GameObject.Find("EXIT 7 / architectural environment").transform;root.localScale=new Vector3(4.5f/4.2f,1,1);
  var paint=Mat("Service door paint",new Color(.49f,.51f,.50f),0,.4f);var steel=Mat("Machined stainless",new Color(.62f,.64f,.65f),.90f,.62f);var seal=Mat("Door seal",new Color(.02f,.022f,.023f),0,.18f);
  var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"ServiceDoor.fbx");importer.generateSecondaryUV=true;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();
  foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)){
   if(!t||!t.name.StartsWith("Recessed utility door"))continue;
   for(int i=t.childCount-1;i>=0;i--)UnityEngine.Object.DestroyImmediate(t.GetChild(i).gameObject);
   var pos=t.localPosition;pos.y=1.09f;t.localPosition=pos;
   var door=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"ServiceDoor.fbx"),t);door.name="Flush steel service door / Blender model";door.transform.localPosition=new Vector3(0,0,-.009f);door.transform.localRotation=Quaternion.Euler(0,180,0);
   foreach(var r in door.GetComponentsInChildren<MeshRenderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++){var n=mats[i]?mats[i].name:"";mats[i]=n.Contains("Stainless")?steel:n.Contains("Seal")?seal:paint;}r.sharedMaterials=mats;GameObjectUtility.SetStaticEditorFlags(r.gameObject,StaticEditorFlags.ContributeGI|StaticEditorFlags.ReflectionProbeStatic|StaticEditorFlags.BatchingStatic);r.receiveGI=ReceiveGI.Lightmaps;r.scaleInLightmap=1;}
   var box=t.GetComponent<BoxCollider>();if(!box)box=t.gameObject.AddComponent<BoxCollider>();box.center=Vector3.zero;box.size=new Vector3(.97f,2.18f,.10f);
  }
  var g=GameObject.Find("GÖRÜNTÜ AYARLARI");if(!g)g=new GameObject("GÖRÜNTÜ AYARLARI");var c=g.GetComponent<StationLookControls>();if(!c)c=g.AddComponent<StationLookControls>();
  c.wall=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Glazed ceramic 300mm.mat");c.floor=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Fine aggregate porcelain 450mm.mat");c.ceiling=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV3/Satin enamel aluminium.mat");c.door=paint;c.tubes=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV4/T8 phosphor glass.mat");c.sign=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Printed luminous exit sign.mat");c.lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);c.probes=UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);c.view=Camera.main;c.volume=UnityEngine.Object.FindFirstObjectByType<PostProcessVolume>();c.wallNormal=.55f;c.grout=.28f;c.motionBlur=120;c.Apply();
  AssetDatabase.CreateAsset(new Preset(c),Root+"ReferenceLook.preset");Save();if(!Lightmapping.Bake())throw new Exception("Reference refinement bake failed");Save();StationImportedModels.Capture();BuildStation.BuildPlayer();Debug.Log("STATION_REFERENCE_AND_INSPECTOR_READY");
 }
 static void Save(){EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
 public static void OpenControls(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");StationLookControlsEditor.SelectControls();if(SceneView.lastActiveSceneView){SceneView.lastActiveSceneView.sceneLighting=true;SceneView.lastActiveSceneView.LookAtDirect(new Vector3(.3f,1.67f,4),Quaternion.identity);}Debug.Log("STATION_LOOK_INSPECTOR_SELECTED");}
}
