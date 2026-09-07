using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(StationLookControls))]
public class StationLookControlsEditor:Editor {
 public override void OnInspectorGUI(){
  EditorGUILayout.HelpBox("Scene görünümünde Effects ve Scene Lighting açık olsun. Kamera kadrajını Play ile görebilirsin. Renk, yansıma ve efektler anında değişir; dolaylı ışık değişiklikleri için aşağıdaki ışık hesaplama düğmesini kullan. Kalıcı ayarları Play kapalıyken yap.",MessageType.Info);
  DrawDefaultInspector();var controls=(StationLookControls)target;
  EditorGUILayout.Space();
  if(GUILayout.Button("Ayarları uygula ve sahneyi kaydet")){controls.Apply();EditorUtility.SetDirty(controls);EditorSceneManager.MarkSceneDirty(controls.gameObject.scene);EditorSceneManager.SaveScene(controls.gameObject.scene);AssetDatabase.SaveAssets();}
  using(new EditorGUI.DisabledScope(Application.isPlaying||Lightmapping.isRunning))if(GUILayout.Button("Işıkları yeniden hesapla (Bake)")){controls.Apply();Lightmapping.BakeAsync();}
  if(Lightmapping.isRunning)EditorGUILayout.HelpBox("Işıklar hesaplanıyor…",MessageType.Info);
  if(GUILayout.Button("Başlangıç görünümüne dön")){var preset=AssetDatabase.LoadAssetAtPath<Preset>("Assets/StationRefinement/ReferenceLook.preset");if(preset){Undo.RecordObject(controls,"Restore reference look");preset.ApplyTo(controls);controls.Apply();EditorUtility.SetDirty(controls);}}
  if(GUILayout.Button("Bu görünümü preset olarak kaydet…")){var path=EditorUtility.SaveFilePanelInProject("Görünümü kaydet","MyStationLook","preset","Preset dosyası");if(!string.IsNullOrEmpty(path))AssetDatabase.CreateAsset(new Preset(controls),path);}
 }
 [MenuItem("Exit 7/Görüntü Ayarlarını Seç")]
 public static void SelectControls(){var c=Object.FindFirstObjectByType<StationLookControls>();if(c){Selection.activeGameObject=c.gameObject;EditorGUIUtility.PingObject(c);}}
}
