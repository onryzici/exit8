using UnityEditor;
using UnityEngine;

public static class CommuterVisualReview
{
    [MenuItem("Exit 7/Review/Run encounter visual checks")]
    public static void RunChecks()
    {
        if(EditorApplication.isPlaying) { new GameObject("Encounter visual checks").AddComponent<EncounterVisualCheck>(); return; }
        SessionState.SetBool("EncounterVisualReview.Pending",true);
        EditorApplication.isPlaying=true;
    }
    [MenuItem("Exit 7/Review/Run horror gameplay checks")]
    public static void RunHorror(){
        SessionState.SetBool("HorrorReview.Pending",true);
        if(EditorApplication.isPlaying){SessionState.SetBool("HorrorReview.Pending",false);new GameObject("Horror gameplay checks").AddComponent<HorrorGameplayCheck>();}
        else EditorApplication.isPlaying=true;
    }
    [InitializeOnLoadMethod]
    static void Register(){EditorApplication.playModeStateChanged+=OnPlay;}
    static void OnPlay(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode)return;
        if(SessionState.GetBool("HorrorReview.Pending",false)){SessionState.SetBool("HorrorReview.Pending",false);new GameObject("Horror gameplay checks").AddComponent<HorrorGameplayCheck>();}
        if(!SessionState.GetBool("EncounterVisualReview.Pending",false))return;
        SessionState.SetBool("EncounterVisualReview.Pending",false);
        new GameObject("Encounter visual checks").AddComponent<EncounterVisualCheck>();
    }
}
