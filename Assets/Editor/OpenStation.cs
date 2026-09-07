using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class OpenStation {
    [InitializeOnLoadMethod]
    static void ReportState(){EditorApplication.delayCall+=()=>{System.IO.File.WriteAllText("editor-state.txt","Playing="+EditorApplication.isPlaying+"; Scene="+UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);};}
    [MenuItem("Exit 7/Open playable station")]
    public static void Open(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");EditorApplication.playModeStateChanged+=ReportPlay;EditorApplication.delayCall+=()=>{EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.isPlaying=true;};}
    static void ReportPlay(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode){System.IO.File.WriteAllText("editor-state.txt","Playing=True; Scene="+UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);UnityEngine.Debug.Log("STATION_PLAY_MODE_READY");EditorApplication.playModeStateChanged-=ReportPlay;}}
    public static void Validate(){
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Underground.unity")EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");
        var cc=Object.FindFirstObjectByType<CharacterController>();
        if(cc==null || Camera.main==null)throw new System.Exception("Missing FPS player");
        Physics.SyncTransforms();RaycastHit hit;
        if(!Physics.Raycast(new Vector3(0,1,10),Vector3.down,out hit,2))throw new System.Exception("Floor collision missing");
        if(!Physics.Raycast(new Vector3(0,1,10),Vector3.right,out hit,3))throw new System.Exception("Wall collision missing");
        if(!Physics.Raycast(new Vector3(0,6,35),Vector3.down,out hit,6) || !hit.collider.name.Contains("stair collision"))throw new System.Exception("Stair ramp missing");
        if(Shader.Find("Exit7/Ceramic")==null)throw new System.Exception("Missing ceramic shader");
        Debug.Log("EXIT7_VALIDATION_PASSED: camera, controller, floor, walls, stair collision.");
    }
}
