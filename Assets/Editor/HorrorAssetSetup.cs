using UnityEditor;
using UnityEngine;
public static class HorrorAssetSetup {
 [InitializeOnLoadMethod] static void Queue(){EditorApplication.delayCall+=EnsureMaterials;}
 static void EnsureMaterials(){
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;
  foreach(string name in new[]{"CreatureSkin","VentBlood"}){
   string path="Assets/Resources/Horror/"+name+".mat";
   if(AssetDatabase.LoadAssetAtPath<Material>(path))continue;
   var shader=Shader.Find("Station/"+name);if(!shader)continue;
   AssetDatabase.CreateAsset(new Material(shader),path);
  }
  AssetDatabase.SaveAssets();
 }
}
