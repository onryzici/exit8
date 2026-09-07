using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
public static class RepeatHorrorSetup {
 public static void FixTileImport(){
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Loop/TactileTile.fbx"));var f=model.GetComponentInChildren<MeshFilter>();var mesh=Object.Instantiate(f.sharedMesh);var v=mesh.vertices;var normals=mesh.normals;var matrix=f.transform.localToWorldMatrix;for(int i=0;i<v.Length;i++){v[i]=matrix.MultiplyPoint3x4(v[i]);normals[i]=matrix.inverse.transpose.MultiplyVector(normals[i]).normalized;}mesh.vertices=v;mesh.RecalculateBounds();var b=mesh.bounds;for(int i=0;i<v.Length;i++)v[i]=new Vector3((v[i].x-b.center.x)*.298f/b.size.x,(v[i].y-b.min.y)*.021f/b.size.y-.007f,(v[i].z-b.center.z)*.298f/b.size.z);mesh.vertices=v;mesh.normals=normals;mesh.RecalculateBounds();mesh.RecalculateTangents();Unwrapping.GenerateSecondaryUVSet(mesh);if(matrix.determinant<0){var indices=mesh.triangles;for(int i=0;i<indices.Length;i+=3){int t=indices[i];indices[i]=indices[i+2];indices[i+2]=t;}mesh.triangles=indices;}AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath("Assets/Loop/CorrectedGuideMesh.asset"));Object.DestroyImmediate(model);
  foreach(var r in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(r=>r.name=="Tactile route tile")){if(Mathf.Abs(r.transform.localPosition.x)>.1f||(r.transform.localPosition.z>0&&r.transform.localPosition.z<26))r.sharedMesh=mesh;}
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();if(!Lightmapping.Bake())throw new Exception("Tile bake failed");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();BuildCorridorLoop.Export();Debug.Log("TACTILE_IMPORT_FIXED");
 }
 public static void Run(){
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");var loop=Object.FindFirstObjectByType<CorridorLoop>();
  var previous=GameObject.Find("Previous passage").transform;previous.SetPositionAndRotation(new Vector3(-12.4f,0,-.6f),Quaternion.Euler(0,180,0));
  var guides=GameObject.Find("Guide passage -1").transform;guides.SetPositionAndRotation(previous.position,previous.rotation);
  var probeGroup=GameObject.Find("Tactile route lighting samples");if(probeGroup)Object.DestroyImmediate(probeGroup);
  var imported=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Loop/TactileTile.fbx").GetComponentInChildren<MeshFilter>();var mesh=Object.Instantiate(imported.sharedMesh);mesh.name="Clean moulded rubber tile";Unwrapping.GenerateSecondaryUVSet(mesh);AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath("Assets/Loop/CleanGuideMesh.asset"));
  var rubber=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Worn tactile rubber.mat"));rubber.SetFloat("_Glossiness",.27f);AssetDatabase.CreateAsset(rubber,AssetDatabase.GenerateUniqueAssetPath("Assets/Loop/RestoredRubber.mat"));
  foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name=="Tactile route tile")){
   var anchor=r.transform.Find("Guide light sample");if(anchor)Object.DestroyImmediate(anchor.gameObject);r.probeAnchor=null;
   if(Mathf.Abs(r.transform.localPosition.x)>.1f||(r.transform.localPosition.z>0&&r.transform.localPosition.z<26))r.GetComponent<MeshFilter>().sharedMesh=mesh;
   r.sharedMaterial=rubber;r.shadowCastingMode=ShadowCastingMode.On;GameObjectUtility.SetStaticEditorFlags(r.gameObject,StaticEditorFlags.ContributeGI|StaticEditorFlags.ReflectionProbeStatic|StaticEditorFlags.BatchingStatic);r.receiveGI=ReceiveGI.Lightmaps;r.scaleInLightmap=4;
  }
  var h=new GameObject("ANOMALIES / horror encounters").AddComponent<CorridorHorror>();loop.horror=h;h.loop=loop;h.footsteps=loop.player.GetComponent<StationFootsteps>().clips;
  var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Loop/Watcher.fbx");h.watcher=Object.Instantiate(model);h.watcher.name="Watcher / sculpted silhouette";
  var skin=new Material(Shader.Find("Standard"));skin.color=new Color(.032f,.037f,.035f);skin.SetFloat("_Glossiness",.32f);AssetDatabase.CreateAsset(skin,"Assets/Loop/WatcherSkin.mat");
  var eyes=new Material(Shader.Find("Standard"));eyes.color=new Color(.43f,.46f,.37f);eyes.EnableKeyword("_EMISSION");eyes.SetColor("_EmissionColor",new Color(.5f,.53f,.40f)*.8f);AssetDatabase.CreateAsset(eyes,"Assets/Loop/WatcherEyes.mat");
  foreach(var r in h.watcher.GetComponentsInChildren<Renderer>())r.sharedMaterial=r.name.Contains("Eye")?eyes:skin;h.watcher.SetActive(false);
  h.water=new GameObject("Flood / approaching from far end");var filter=h.water.AddComponent<MeshFilter>();var waterMesh=new Mesh{name="Subdivided flood surface"};const int nx=33,nz=193;var verts=new Vector3[nx*nz];var uv=new Vector2[verts.Length];var tris=new int[(nx-1)*(nz-1)*6];
  for(int z=0;z<nz;z++)for(int x=0;x<nx;x++){int i=z*nx+x;verts[i]=new Vector3(x/(float)(nx-1)-.5f,0,z/(float)(nz-1)-.5f);uv[i]=new Vector2(x/(float)(nx-1),z/(float)(nz-1));}
  int k=0;for(int z=0;z<nz-1;z++)for(int x=0;x<nx-1;x++){int i=z*nx+x;tris[k++]=i;tris[k++]=i+nx;tris[k++]=i+1;tris[k++]=i+1;tris[k++]=i+nx;tris[k++]=i+nx+1;}waterMesh.vertices=verts;waterMesh.uv=uv;waterMesh.triangles=tris;waterMesh.RecalculateNormals();waterMesh.RecalculateTangents();waterMesh.bounds=new Bounds(Vector3.zero,new Vector3(1,2,1));AssetDatabase.CreateAsset(waterMesh,"Assets/Loop/FloodMesh.asset");filter.sharedMesh=waterMesh;
  var waterMat=new Material(Shader.Find("Station/AnomalyWater"));AssetDatabase.CreateAsset(waterMat,"Assets/Loop/FloodMaterial.mat");var wr=h.water.AddComponent<MeshRenderer>();wr.sharedMaterial=waterMat;wr.shadowCastingMode=ShadowCastingMode.Off;var wa=new GameObject("Water light anchor").transform;wa.SetParent(h.water.transform,false);wa.localPosition=Vector3.up*1.2f;wr.probeAnchor=wa;h.water.SetActive(false);
  h.emergency=new GameObject("Emergency anomaly light").AddComponent<Light>();h.emergency.type=LightType.Point;h.emergency.color=new Color(1,.055f,.015f);h.emergency.range=14;h.emergency.intensity=8;h.emergency.transform.position=new Vector3(.8f,2.9f,16);h.emergency.gameObject.SetActive(false);
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();if(!Lightmapping.Bake())throw new Exception("Repeated layout bake failed");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();BuildCorridorLoop.Export();Debug.Log("REPEAT_HORROR_SETUP_COMPLETE");
 }
}
