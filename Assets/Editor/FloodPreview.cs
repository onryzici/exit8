using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class FloodPreview {
 public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");new GameObject("Flood preview recorder").AddComponent<FloodPreviewRecorder>();EditorApplication.isPlaying=true;}
}
