using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class CommuterFinish {
 public static void Run(){var scene=EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");var loop=Object.FindFirstObjectByType<CorridorLoop>();var c=loop.commuter;var ts=c.GetComponentsInChildren<Transform>();c.leftFoot=ts.First(t=>t.name=="Bip01 L Foot");c.rightFoot=ts.First(t=>t.name=="Bip01 R Foot");var mat=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Loop/FloodSpray.mat"));mat.SetColor("_Color",new Color(0,0,0,.2f));AssetDatabase.CreateAsset(mat,"Assets/Commuter/Foot contact.mat");c.footShadows=new Renderer[2];for(int i=0;i<2;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name="Soft foot contact";Object.DestroyImmediate(g.GetComponent<Collider>());g.transform.SetParent(c.transform);g.transform.localScale=new Vector3(.26f,.40f,1);var r=g.GetComponent<Renderer>();r.sharedMaterial=mat;c.footShadows[i]=r;}loop.horror.water.GetComponent<MeshFilter>().sharedMesh.bounds=new Bounds(new Vector3(0,1,0),new Vector3(1,4,1));EditorUtility.SetDirty(loop.horror.water.GetComponent<MeshFilter>().sharedMesh);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();BuildStation.BuildPlayer();Debug.Log("COMMUTER_FINAL_BUILD_READY");}
}
