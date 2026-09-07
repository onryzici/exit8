using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class CorridorLoopQA {
 public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");new GameObject("Temporary loop QA").AddComponent<CorridorLoopCheck>();EditorApplication.isPlaying=true;}
}
