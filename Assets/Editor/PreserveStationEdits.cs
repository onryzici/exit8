using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
[InitializeOnLoad] public static class PreserveStationEdits {
 static PreserveStationEdits(){EditorApplication.delayCall+=Save;}
 static void Save(){if(!File.Exists("save-before-loop.request"))return;if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling){EditorApplication.delayCall+=Save;return;}EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();File.WriteAllText("save-before-loop.done","Saved existing user scene and material settings.");File.Delete("save-before-loop.request");}
}
