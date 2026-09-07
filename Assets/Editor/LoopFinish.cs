using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
public static class LoopFinish {
 public static void Run() {
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");
  var all=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
  foreach(var t in all.Where(t=>t.name=="Connector ceiling backing")) {
   var p=t.localPosition;p.y=3.62f;t.localPosition=p;
  }
  var yellow=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Tactile ochre polymer.mat");
  if(!yellow)yellow=all.First(t=>t.name=="Connector tactile tile").GetComponent<Renderer>().sharedMaterial;
  foreach(var root in all.Where(t=>t.name=="LOOP / concealed return corridors"||t.name=="Neighbour corner corridors")) {
   if(root.Find("Corner guide continuation"))continue;
   var guide=new GameObject("Corner guide continuation").transform;guide.SetParent(root,false);
   foreach(int side in new[]{-1,1})for(int i=0;i<8;i++) {
    float x=side*i*.3f,z=side<0?-.3f:26.3f;
    Cube(guide,new Vector3(x,.012f,z),new Vector3(.298f,.018f,.298f),yellow);
    for(int rib=0;rib<4;rib++)Cube(guide,new Vector3(x,.026f,z-.108f+rib*.072f),new Vector3(.26f,.009f,.025f),yellow);
   }
  }
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  if(!Lightmapping.Bake())throw new Exception("Final corner lighting failed");
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  BuildCorridorLoop.Export();Debug.Log("LOOP_FINISH_SUCCESS");
 }
 public static void RepairMaterial() {
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");
  var renderers=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
  var yellow=renderers.First(r=>r.name=="Connector tactile tile").sharedMaterial;
  if(!yellow)throw new Exception("Tactile material missing");
  foreach(var r in renderers.Where(r=>r.name=="Guide surface"))r.sharedMaterial=yellow;
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  if(!Lightmapping.Bake())throw new Exception("Guide lighting failed");
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  BuildCorridorLoop.Export();Debug.Log("GUIDE_MATERIAL_FIXED");
 }
 static void Cube(Transform parent,Vector3 p,Vector3 size,Material mat) {
  var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Guide surface";g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;
  Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;
  GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.ContributeGI|StaticEditorFlags.ReflectionProbeStatic|StaticEditorFlags.BatchingStatic);
 }
}
