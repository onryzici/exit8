using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class StationQA {
    public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");new GameObject("Temporary runtime QA").AddComponent<StationRuntimeCheck>();EditorApplication.isPlaying=true;}
}
