using UnityEngine;
using System.Collections;

public class StationDisplay : MonoBehaviour {
 public Camera worldCamera;
 public bool MenuOpen {get;private set;}
 public int RenderWidth => target?target.width:0;
 public int RenderHeight => target?target.height:0;
 RenderTexture target;Camera presenter;int quality;bool fullscreen=true;
 readonly string[] labels={"3840 × 2160 · 4K","2560 × 1440","1920 × 1080"};
 IEnumerator Start(){
  if(Application.isBatchMode)yield break;
  quality=Mathf.Clamp(PlayerPrefs.GetInt("RenderQuality",0),0,2);fullscreen=PlayerPrefs.GetInt("Fullscreen",1)==1;
  var g=new GameObject("Final display output");g.transform.SetParent(transform,false);presenter=g.AddComponent<Camera>();presenter.depth=100;presenter.cullingMask=0;presenter.clearFlags=CameraClearFlags.Nothing;presenter.allowHDR=false;g.AddComponent<StationDisplayBlit>().owner=this;
  Apply();yield return null;yield return null;
  Debug.Log("STATION_DISPLAY render="+RenderWidth+"x"+RenderHeight+" output="+Screen.width+"x"+Screen.height+" mode="+Screen.fullScreenMode);
 }
 void Update(){if(Input.GetKeyDown(KeyCode.F2)){MenuOpen=!MenuOpen;Cursor.lockState=MenuOpen?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=MenuOpen;}if(Input.GetKeyDown(KeyCode.F11)){fullscreen=!fullscreen;Apply();}}
 void Apply(){
  int w=quality==0?3840:quality==1?2560:1920,h=w*9/16;
  if(target){worldCamera.targetTexture=null;target.Release();Destroy(target);}
  target=new RenderTexture(w,h,24,RenderTextureFormat.ARGBHalf){name="Station render resolution",filterMode=FilterMode.Bilinear};target.Create();worldCamera.targetTexture=target;worldCamera.aspect=16f/9f;
  if(fullscreen)Screen.SetResolution(Display.main.systemWidth,Display.main.systemHeight,FullScreenMode.FullScreenWindow);else Screen.SetResolution(1920,1080,FullScreenMode.Windowed);
  QualitySettings.vSyncCount=1;PlayerPrefs.SetInt("RenderQuality",quality);PlayerPrefs.SetInt("Fullscreen",fullscreen?1:0);PlayerPrefs.Save();
 }
 public void Present(RenderTexture destination){if(target)Graphics.Blit(target,destination);}
 void OnGUI(){
  if(!MenuOpen)return;
  var old=GUI.matrix;float scale=Mathf.Max(1,Screen.height/1080f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
  float x=(Screen.width/scale-430)/2,y=(Screen.height/scale-310)/2;
  GUI.Box(new Rect(x,y,430,310),"GÖRÜNTÜ AYARLARI");GUI.Label(new Rect(x+22,y+38,380,25),"Sahne çözünürlüğü");
  int selected=GUI.SelectionGrid(new Rect(x+22,y+66,386,95),quality,labels,1);if(selected!=quality){quality=selected;Apply();}
  bool fs=GUI.Toggle(new Rect(x+22,y+178,386,25),fullscreen,"Tam ekran · F11");if(fs!=fullscreen){fullscreen=fs;Apply();}
  GUI.Label(new Rect(x+22,y+211,390,38),"4K görüntü, ekran çözünürlüğüne ölçeklenir.");
  if(GUI.Button(new Rect(x+22,y+260,386,30),"Devam et · F2")){MenuOpen=false;Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}GUI.matrix=old;
 }
 void OnDestroy(){if(worldCamera)worldCamera.targetTexture=null;if(target){target.Release();Destroy(target);}}
}
