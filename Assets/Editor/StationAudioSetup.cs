using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public static class StationAudioSetup {
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");
  var fps=UnityEngine.Object.FindFirstObjectByType<FirstPerson>();
  var feet=fps.GetComponent<StationFootsteps>();if(!feet)feet=fps.gameObject.AddComponent<StationFootsteps>();feet.clips=new AudioClip[6];
  for(int i=0;i<6;i++){
   string path="Assets/Audio/Footsteps/"+(i+1).ToString("D2")+"-footstep.wav";
   var importer=(AudioImporter)AssetImporter.GetAtPath(path);importer.forceToMono=true;var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
   feet.clips[i]=AssetDatabase.LoadAssetAtPath<AudioClip>(path);if(!feet.clips[i]||feet.clips[i].length<=0)throw new Exception("Invalid step recording: "+path);
   Debug.Log("FOOTSTEP_IMPORTED "+path+" seconds="+feet.clips[i].length);
  }
  var source=fps.GetComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=.68f;
  var profile=AssetDatabase.LoadAssetAtPath<PostProcessProfile>("Assets/ArtV2/StationPhotography.asset");var blur=profile.GetSetting<MotionBlur>();if(!blur){blur=profile.AddSettings<MotionBlur>();AssetDatabase.AddObjectToAsset(blur,profile);}blur.enabled.Override(true);blur.shutterAngle.Override(0);blur.sampleCount.Override(8);EditorUtility.SetDirty(blur);EditorUtility.SetDirty(profile);
  if(!fps.view.GetComponent<TurningMotionBlur>())fps.view.gameObject.AddComponent<TurningMotionBlur>();
  EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
  BuildStation.BuildPlayer();Debug.Log("STATION_AUDIO_AND_BLUR_READY");
 }
}
