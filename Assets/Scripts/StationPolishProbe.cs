#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
public class StationPolishProbe:MonoBehaviour {
 IEnumerator Start(){
  yield return new WaitForSeconds(.8f);
  var player=FindFirstObjectByType<FirstPerson>();var blur=player.view.GetComponent<TurningMotionBlur>();
  var field=typeof(TurningMotionBlur).GetField("smear",BindingFlags.NonPublic|BindingFlags.Instance);
  bool ok=blur.directionalShader&&blur.directionalShader.isSupported&&!player.view.GetComponent<PostProcessLayer>().finalBlitToCameraTarget;
  for(int i=0;i<12;i++){player.transform.Rotate(0,120*Time.deltaTime,0);yield return null;}
  ok&=((Vector2)field.GetValue(blur)).magnitude>.003f;
  yield return null;yield return null;
  ok&=((Vector2)field.GetValue(blur)).magnitude<.00001f;
  var source=new Texture2D(256,16,TextureFormat.RGBA32,false,true);
  for(int x=0;x<256;x++)for(int y=0;y<16;y++)source.SetPixel(x,y,(x%8)<4?Color.white:Color.black);source.Apply();
  var target=RenderTexture.GetTemporary(256,16,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);var mat=new Material(blur.directionalShader);mat.SetVector("_Smear",new Vector4(.02f,0,0,0));Graphics.Blit(source,target,mat);
  var old=RenderTexture.active;RenderTexture.active=target;var read=new Texture2D(256,16,TextureFormat.RGBA32,false,true);read.ReadPixels(new Rect(0,0,256,16),0,0);read.Apply();RenderTexture.active=old;
  float pixel=read.GetPixel(128,8).r;ok&=pixel>.1f&&pixel<.9f;
  Destroy(source);Destroy(read);Destroy(mat);RenderTexture.ReleaseTemporary(target);
  var flags=BindingFlags.NonPublic|BindingFlags.Instance;
  typeof(FirstPerson).GetField("runningBlend",flags).SetValue(player,1f);
  typeof(FirstPerson).GetField("running",flags).SetValue(player,false);
  float baseFov=FindFirstObjectByType<StationLookControls>().fieldOfView;player.view.fieldOfView=baseFov+player.sprintFovIncrease;
  float elapsed=0;while(elapsed<.25f){typeof(FirstPerson).GetMethod("LateUpdate",flags).Invoke(player,null);elapsed+=Time.deltaTime;yield return null;}
  bool recovery=Mathf.Abs(player.view.fieldOfView-baseFov)<.5f;ok&=recovery;Debug.Log("SPRINT_RECOVERY_UNDER_250MS "+(recovery?"PASS":"FAIL"));
  System.IO.File.WriteAllText("camera-effect-validation.txt","Turning response, stationary sharpness, post-process routing and GPU blur: "+(ok?"PASS":"FAIL"));
  Debug.Log("CAMERA_EFFECT_VALIDATION "+(ok?"PASS":"FAIL"));if(!ok)EditorApplication.Exit(1);
 }
}

#endif
