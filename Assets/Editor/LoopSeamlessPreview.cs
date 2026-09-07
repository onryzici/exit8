using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class LoopSeamlessPreview {
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");if(GameObject.Find("LOOP / neighbouring passage previews"))throw new Exception("Previews already built");
  var connectors=GameObject.Find("LOOP / concealed return corridors").transform;
  foreach(var t in connectors.GetComponentsInChildren<Transform>()){
   if(!t)continue;string n=t.name;
   if(n.StartsWith("Portal corridor end")){Object.DestroyImmediate(t.gameObject);continue;}
   if(n=="Connector ceiling panel"||n=="Connector tactile tile"||n=="Connector tactile rib"){if(Mathf.Abs(t.position.x)>6.19f)Object.DestroyImmediate(t.gameObject);continue;}
   if(n=="Connector floor backing"||n=="Connector wall backing"||n=="Connector ceiling backing"){var s=t.localScale;s.x=4;t.localScale=s;var p=t.position;p.x=Mathf.Sign(p.x)*4.2f;t.position=p;}
   if(n=="Connector porcelain"||n=="Connector glazed ceramic"){
    var f=t.GetComponent<MeshFilter>();var m=f.sharedMesh;var vs=m.vertices;var uv=m.uv;for(int i=0;i<vs.Length;i++){vs[i].x*=4f/5.3f;uv[i].x*=4f/5.3f;}m.vertices=vs;m.uv=uv;m.RecalculateBounds();Unwrapping.GenerateSecondaryUVSet(m);EditorUtility.SetDirty(m);var p=t.position;p.x=Mathf.Sign(p.x)*4.2f;t.position=p;
   }
   if(n=="Connector twin fluorescent"||n=="Connector reflection"){var p=t.position;p.x=Mathf.Sign(p.x)*4.2f;t.position=p;}
  }
  var previews=new GameObject("LOOP / neighbouring passage previews").transform;
  foreach(int side in new[]{-1,1}){
   var cell=new GameObject(side<0?"Previous passage":"Next passage").transform;cell.SetParent(previews);cell.position=new Vector3(side*12.4f,0,side*26.6f);
   var architecture=Object.Instantiate(GameObject.Find("EXIT 7 / architectural environment"),cell);architecture.name="Neighbour architecture";architecture.transform.localPosition=Vector3.zero;
   var joins=Object.Instantiate(connectors.gameObject,cell);joins.name="Neighbour corner corridors";joins.transform.localPosition=Vector3.zero;
   foreach(var t in joins.GetComponentsInChildren<Transform>())if(t&&t.name.StartsWith("FINAL EXIT"))Object.DestroyImmediate(t.gameObject);
  }
  var look=Object.FindFirstObjectByType<StationLookControls>();look.lights=Object.FindObjectsByType<Light>(FindObjectsSortMode.None);look.probes=Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);look.Apply();
  EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
  if(!Lightmapping.Bake())throw new Exception("Neighbour bake failed");EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();BuildCorridorLoop.Export();Debug.Log("SEAMLESS_NEIGHBOUR_PREVIEWS_READY");
 }
}
