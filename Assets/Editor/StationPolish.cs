using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Object=UnityEngine.Object;
public static class StationPolish {
 public static void Run(){
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");
  var loop=Object.FindFirstObjectByType<CorridorLoop>();
  var all=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
  var template=all.First(t=>t.name=="300 mm tactile paving"&&Mathf.Abs(t.position.x)<1&&t.position.z>4&&t.position.z<6);
  var mesh=template.GetComponent<MeshFilter>().sharedMesh;var mat=template.GetComponent<Renderer>().sharedMaterial;
  foreach(var t in all){if(!t)continue;string n=t.name;if(n=="300 mm tactile paving"||n.StartsWith("Connector tactile")||n=="Corner guide continuation"||n.StartsWith("Return progress")||n.StartsWith("Return instructions"))Object.DestroyImmediate(t.gameObject);}
  var guide=new GameObject("Continuous tactile route").transform;var anomalyTiles=new List<GameObject>();
  foreach(int cell in new[]{-1,0,1}){
   var parent=new GameObject("Guide passage "+cell).transform;parent.SetParent(guide);parent.position=new Vector3(cell*12.4f,0,cell*26.6f);
   for(int i=1;i<89;i++){float z=-.3f+i*(26.6f/89);var g=Tile(parent,mesh,mat,new Vector3(0,.015f,z),0,new Vector3(1,1,(26.6f/89)/.3f));if(cell==0&&z>10&&z<13)anomalyTiles.Add(g);}
   foreach(int side in new[]{-1,1}){
    float z=side<0?-.3f:26.3f;Corner(parent,mat,z);
    const float step=6.05f/20;
    for(int i=0;i<20;i++)Tile(parent,mesh,mat,new Vector3(side*(.15f+(i+.5f)*step),.015f,z),90,new Vector3(1,1,step/.3f));
   }
  }
  loop.tactileSection=anomalyTiles.ToArray();
  loop.numberSigns=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.name=="Entry progress").Concat(loop.numberSigns.Where(r=>r&&r.name=="Final number eight")).ToArray();
  foreach(var r in loop.sign.GetComponentsInChildren<Renderer>())if(r.sharedMaterial&&r.sharedMaterial.mainTexture&&r.sharedMaterial.mainTexture.name.StartsWith("Exit")){r.sharedMaterial.mainTexture=loop.numberTextures[8];EditorUtility.SetDirty(r.sharedMaterial);}
  var camera=loop.player.view;camera.GetComponent<TurningMotionBlur>().directionalShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/TurningSmear.shader");
  camera.GetComponent<PostProcessLayer>().finalBlitToCameraTarget=false;
  var look=Object.FindFirstObjectByType<StationLookControls>();look.motionBlur=160;look.Apply();
  Debug.Assert(loop.numberSigns.Length==4,"One entry counter per passage plus final counter");Debug.Assert(loop.tactileSection.Length>0,"Anomaly guide references preserved");
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  if(!Lightmapping.Bake())throw new Exception("Route bake failed");
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();BuildCorridorLoop.Export();Debug.Log("STATION_POLISH_READY");
 }
 static GameObject Tile(Transform parent,Mesh mesh,Material mat,Vector3 p,float yaw,Vector3 scale){var g=new GameObject("Tactile route tile");g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localRotation=Quaternion.Euler(0,yaw,0);g.transform.localScale=scale;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.ContributeGI|StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic);return g;}
 static void Corner(Transform parent,Material mat,float z){
  var parts=new List<CombineInstance>();var temp=new List<GameObject>();
  var slab=GameObject.CreatePrimitive(PrimitiveType.Cube);slab.transform.localScale=new Vector3(.298f,.016f,.298f);temp.Add(slab);
  for(int x=0;x<4;x++)for(int y=0;y<4;y++){var stud=GameObject.CreatePrimitive(PrimitiveType.Cylinder);stud.transform.position=new Vector3(-.108f+x*.072f,.011f,-.108f+y*.072f);stud.transform.localScale=new Vector3(.025f,.005f,.025f);temp.Add(stud);}
  foreach(var g in temp)parts.Add(new CombineInstance{mesh=g.GetComponent<MeshFilter>().sharedMesh,transform=g.transform.localToWorldMatrix});
  var mesh=new Mesh{name="Tactile corner dots"};mesh.CombineMeshes(parts.ToArray());Unwrapping.GenerateSecondaryUVSet(mesh);AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath("Assets/Loop/CornerGuide.asset"));foreach(var g in temp)Object.DestroyImmediate(g);
  Tile(parent,mesh,mat,new Vector3(0,.015f,z),0,Vector3.one);
 }
}
