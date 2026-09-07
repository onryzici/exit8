using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
public static class StationImportedModels {
 const string Root="Assets/ImportedModels/";
 static Texture2D Tex(string path,bool normal=false,bool linear=false){var i=(TextureImporter)AssetImporter.GetAtPath(path);i.maxTextureSize=4096;i.anisoLevel=16;i.textureCompression=TextureImporterCompression.CompressedHQ;i.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;i.sRGBTexture=!normal&&!linear;i.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);}
 static Material Material(string kind){var m=new Material(Shader.Find("Exit7/Imported painted metal"));bool door=kind=="Door";m.SetTexture("_MainTex",Tex(Root+kind+(door?"/diffuse.png":"/diff.jpg")));m.SetTexture("_BumpMap",Tex(Root+kind+(door?"/normal.png":"/nor_gl.jpg"),true));m.SetTexture("_Roughness",Tex(Root+kind+(door?"/specular.png":"/rough.jpg"),false,true));if(!door)m.SetTexture("_MetalMap",Tex(Root+kind+"/metal.jpg",false,true));m.SetFloat("_IsSpec",door?1:0);m.SetFloat("_Gloss",door?.65f:.72f);m.SetFloat("_Metallic",door?.24f:.85f);m.SetFloat("_Saturation",door?.12f:.8f);m.SetFloat("_Lift",door?.18f:0);AssetDatabase.CreateAsset(m,Root+kind+"/Surface.mat");return m;}
 static GameObject Model(string kind,Material mat){string path=Root+kind+"/"+kind+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.generateSecondaryUV=true;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();var g=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));g.name="Imported "+kind;
  foreach(var r in g.GetComponentsInChildren<MeshRenderer>()){var mats=new Material[r.sharedMaterials.Length];for(int i=0;i<mats.Length;i++)mats[i]=mat;r.sharedMaterials=mats;GameObjectUtility.SetStaticEditorFlags(r.gameObject,StaticEditorFlags.ContributeGI|StaticEditorFlags.ReflectionProbeStatic|StaticEditorFlags.BatchingStatic);r.scaleInLightmap=1.5f;r.receiveGI=ReceiveGI.Lightmaps;}
  return g;
 }
 static void Replace(Transform p,string kind,Material mat){
  for(int i=p.childCount-1;i>=0;i--)UnityEngine.Object.DestroyImmediate(p.GetChild(i).gameObject);
  var g=Model(kind,mat);g.transform.SetParent(p,false);g.transform.localPosition=Vector3.zero;g.transform.localRotation=kind=="Door"?Quaternion.Euler(0,180,0):Quaternion.Euler(90,0,0);
  var renderers=g.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
  // Measure in parent coordinates; the imported FBX has already converted Z-up to Y-up.
  var local=new Bounds();bool first=true;foreach(var f in g.GetComponentsInChildren<MeshFilter>()){var b=f.sharedMesh.bounds;foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(int z in new[]{-1,1}){var v=p.InverseTransformPoint(f.transform.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3(x,y,z))));if(first){local=new Bounds(v,Vector3.zero);first=false;}else local.Encapsulate(v);}}
  var wanted=kind=="Door"?new Vector3(1.01f,2.17f,.16f):new Vector3(.615f,.425f,.065f);var s=new Vector3(wanted.x/local.size.x,wanted.y/local.size.y,wanted.z/local.size.z);g.transform.localScale=Vector3.Scale(g.transform.localScale,s);g.transform.localPosition=-Vector3.Scale(local.center,s)+new Vector3(0,0,kind=="Door"?-.05f:-.047f);
  if(kind=="Door"){var box=p.gameObject.GetComponent<BoxCollider>();if(!box)box=p.gameObject.AddComponent<BoxCollider>();box.center=new Vector3(0,0,-.05f);box.size=wanted;}
  Debug.Log("IMPORTED_MODEL "+kind+" originalBounds="+local.size+" fitted="+wanted);
 }
 public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");var door=Material("Door");var vent=Material("Vent");foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)){if(!t)continue;if(t.name.StartsWith("Recessed utility door"))Replace(t,"Door",door);else if(t.name=="Wall ventilation grille")Replace(t,"Vent",vent);}
  var fps=UnityEngine.Object.FindFirstObjectByType<FirstPerson>();var display=fps.GetComponent<StationDisplay>();if(!display)display=fps.gameObject.AddComponent<StationDisplay>();display.worldCamera=fps.view;
  PlayerSettings.defaultIsNativeResolution=true;PlayerSettings.defaultScreenWidth=3840;PlayerSettings.defaultScreenHeight=2160;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
  Save();if(!Lightmapping.Bake())throw new Exception("Imported model bake failed");Save();Capture();BuildStation.BuildPlayer();File.Copy("Assets/ImportedModels/CREDITS.txt","Build/ASSET-CREDITS.txt",true);Debug.Log("STATION_IMPORTED_MODELS_4K_COMPLETE");
 }
 static void Save(){EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
 public static void Fix(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)){if(!t)continue;if(t.name.StartsWith("Recessed utility door"))Replace(t,"Door",AssetDatabase.LoadAssetAtPath<Material>(Root+"Door/Surface.mat"));else if(t.name=="Wall ventilation grille")Replace(t,"Vent",AssetDatabase.LoadAssetAtPath<Material>(Root+"Vent/Surface.mat"));}Save();if(!Lightmapping.Bake())throw new Exception("Model correction bake failed");Save();Capture();BuildStation.BuildPlayer();Debug.Log("STATION_IMPORTED_FINAL_COMPLETE");}
 public static void Capture(){if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Underground.unity")EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");var c=Camera.main;var p=c.transform.position;var q=c.transform.rotation;Shot(c,"corridor-4k",new Vector3(.3f,1.67f,-1.2f),Quaternion.identity);Shot(c,"models-4k",new Vector3(.0f,1.7f,5.7f),Quaternion.Euler(-5,60,0));c.transform.SetPositionAndRotation(p,q);}
 static void Shot(Camera cam,string n,Vector3 p,Quaternion q){cam.transform.SetPositionAndRotation(p,q);var rt=new RenderTexture(3840,2160,24,RenderTextureFormat.ARGBHalf);rt.Create();cam.targetTexture=rt;for(int i=0;i<20;i++)cam.Render();RenderTexture.active=rt;var t=new Texture2D(3840,2160,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,3840,2160),0,0);var px=t.GetPixels();for(int i=0;i<px.Length;i++)px[i]=px[i].gamma;t.SetPixels(px);t.Apply();File.WriteAllBytes("Screenshots/"+n+".png",t.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);}
}
