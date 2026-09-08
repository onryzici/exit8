#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
public class HorrorGameplayCheck:MonoBehaviour {
 CorridorLoop loop;readonly StringBuilder report=new StringBuilder();bool passed=true;
 void Check(bool value,string name){passed&=value;report.AppendLine((value?"PASS ":"FAIL ")+name);}
 void Capture(string name){
  var c=loop.player.view;var old=c.targetTexture;var active=RenderTexture.active;
  var rt=RenderTexture.GetTemporary(1440,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
  c.targetTexture=rt;c.Render();RenderTexture.active=rt;
  var image=new Texture2D(1440,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1440,900),0,0);image.Apply();
  File.WriteAllBytes("Temp/HorrorReview/"+name+".png",image.EncodeToPNG());
  c.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);Destroy(image);
 }
 IEnumerator Start(){
  Directory.CreateDirectory("Temp/HorrorReview");yield return null;
  foreach(string shaderName in new[]{"Station/VentBlood","Station/CreatureSkin"}){
   var shader=Shader.Find(shaderName);bool clean=shader&&shader.isSupported;
   if(shader)foreach(var issue in UnityEditor.ShaderUtil.GetShaderMessages(shader)){if(issue.severity.ToString()=="Error"){clean=false;report.AppendLine(issue.message);}}
   Check(clean,shaderName+" compiles without shader errors");
  }
  loop=FindAnyObjectByType<CorridorLoop>();loop.ResetRun();loop.player.enabled=false;
  var camera=loop.player.view;var localPosition=camera.transform.localPosition;
  loop.player.Teleport(new Vector3(0,.04f,8),0,true);loop.SetAnomaly(8);
  yield return new WaitForSeconds(1.7f);
  var monster=loop.horror.watcher;
  var start=monster.transform.position;
  yield return new WaitForSeconds(.6f);Check(Vector3.Distance(start,monster.transform.position)<.03f,"Looking at watcher stops its approach");
  loop.player.Teleport(loop.player.transform.position,180,true);
  yield return new WaitForSeconds(.7f);Check(Vector3.Distance(start,monster.transform.position)>1,"Watcher closes distance when unseen");
  loop.player.Teleport(new Vector3(0,.04f,15),0,true);
  monster.transform.position=new Vector3(.5f,0,19);
  yield return new WaitForSeconds(.15f);Capture("01-watcher");
  loop.ClearAnomaly();Check(!loop.horror.HasVisibleChange,"Watcher clears fully");
  loop.player.Teleport(new Vector3(0,.04f,13),0,true);loop.SetAnomaly(9);
  yield return new WaitForSeconds(.6f);
  camera.transform.LookAt(loop.horror.watcher.transform.position+Vector3.down*.65f);
  Capture("02-ceiling-crawler");
  loop.ClearAnomaly();
  loop.SetAnomaly(12);yield return new WaitForSeconds(6);
  var vent=loop.vent.GetComponentInChildren<Renderer>().bounds;
  camera.transform.position=vent.center+new Vector3(-Mathf.Sign(vent.center.x)*2.1f,-.6f,-.5f);
  camera.transform.LookAt(new Vector3(vent.center.x,vent.center.y-.75f,vent.center.z));
  Capture("03-vent-blood");Check(loop.horror.HasVisibleChange,"Blood anomaly is active");
  loop.ClearAnomaly();Check(!loop.horror.HasVisibleChange,"Blood clears on next round");
  var door=loop.door.transform.position;
  loop.SetAnomaly(13);yield return new WaitForSeconds(2);
  camera.transform.position=door+new Vector3(-Mathf.Sign(door.x)*2.2f,1.3f,-.6f);
  camera.transform.LookAt(door+Vector3.up*1.25f);
  Capture("04-door-hands");Check(loop.horror.HasVisibleChange,"Door hands are active");
  loop.ClearAnomaly();Check(Vector3.Distance(loop.door.transform.position,door)<.001f,"Door returns to its saved transform");
  camera.transform.localPosition=localPosition;
  loop.player.Teleport(new Vector3(0,.04f,10),180,true);loop.SetAnomaly(8);
  yield return new WaitForSeconds(1.4f);
  loop.horror.watcher.transform.position=loop.player.transform.position+Vector3.forward*.7f;
  yield return null;yield return null;
  Check(loop.horror.Encounters.Catching&&loop.player.HorrorLocked,"Close encounter catches and briefly locks player");
  yield return new WaitForSeconds(1.1f);
  Check(loop.Progress==0&&loop.ActiveAnomaly==-1&&!loop.player.HorrorLocked,"Capture resets the run and releases player");
  loop.player.ResetBreath();
  bool sprint=true;for(int i=0;i<80;i++)sprint=loop.player.UpdateStamina(.05f,true);
  Check(!sprint&&loop.player.Exhaustion>.9f,"Continuous sprint exhausts the player");
  for(int i=0;i<160;i++)loop.player.UpdateStamina(.05f,false);
  Check(loop.player.Exhaustion<.01f&&loop.player.UpdateStamina(.01f,true),"Walking/rest restores sprint");
  loop.ResetRun();loop.CrossBoundary(false);
  Check(loop.ActiveAnomaly==12||loop.ActiveAnomaly==13,"First anomaly introduces blood or door threat");
  loop.ResetRun();loop.player.enabled=true;
  report.AppendLine("RESULT "+(passed?"PASS":"FAIL"));File.WriteAllText("Temp/HorrorReview/horror-validation.txt",report.ToString());Debug.Log(report.ToString());Destroy(gameObject);
 }
}
#endif
