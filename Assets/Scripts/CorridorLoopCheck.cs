using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
public class CorridorLoopCheck:MonoBehaviour {
 CorridorLoop loop;CharacterController cc;bool ok=true;string report="Repeating corridor runtime verification\n";
 void Check(bool value,string name){ok&=value;report+=name+": "+(value?"PASS":"FAIL")+"\n";}
 void CheckNumbers(){var texture=loop.numberTextures[Mathf.Clamp(loop.Progress,0,8)];Check(loop.numberSigns.All(r=>r&&r.sharedMaterial.mainTexture==texture&&r.sharedMaterial.GetTexture("_EmissionMap")==texture),"All sign faces and emission agree at "+loop.Progress);}
 IEnumerator Walk(Vector3 target){int round=loop.Round;for(int i=0;i<650;i++){var delta=target-cc.transform.position;delta.y=0;if(delta.magnitude<.09f||loop.Round!=round)yield break;cc.Move(Vector3.ClampMagnitude(delta,.09f)+Vector3.down*.04f);yield return null;}Check(false,"Reach waypoint "+target);}
 string Signature(){var ts=loop.posters.Concat(new[]{loop.vent,loop.door,loop.sign,loop.cctv}).Concat(loop.tactileSection);string s="";foreach(var g in ts)s+=g.activeSelf+" "+g.transform.localPosition+g.transform.localRotation+g.transform.localScale;foreach(var r in loop.lightTubes)s+=MaterialIdentity(r.sharedMaterial);if(loop.horror)s+=loop.horror.HasVisibleChange;return s;}
 static string MaterialIdentity(Material material){
#if UNITY_6000_5_OR_NEWER
  return material.GetEntityId().ToString();
#else
  return material.GetInstanceID().ToString();
#endif
 }
 IEnumerator Start(){yield return null;loop=FindFirstObjectByType<CorridorLoop>();loop.player.enabled=false;cc=loop.player.GetComponent<CharacterController>();yield return new WaitForSeconds(.6f);
  Check(loop.ActiveAnomaly==-1&&loop.Progress==0,"First pass is normal");Check(!loop.exitRoom.activeSelf&&!GameObject.Find("Invisible stair collision"),"No immediate stair exit");
  yield return Walk(new Vector3(0,0,-.3f));yield return Walk(new Vector3(0,0,26.3f));yield return Walk(new Vector3(6.5f,0,26.3f));Check(loop.Progress==1&&loop.EnteredFromStart,"Normal forward traversal loops and advances");
  loop.SetAnomaly(0);yield return new WaitForSeconds(.5f);yield return Walk(new Vector3(0,0,-.3f));yield return Walk(new Vector3(0,0,8));yield return Walk(new Vector3(0,0,-.3f));yield return Walk(new Vector3(-6.5f,0,-.3f));Check(loop.Progress==2&&loop.EnteredFromStart,"Anomaly retreat returns to canonical entrance");
  loop.SetAnomaly(-1);yield return new WaitForSeconds(.5f);yield return Walk(new Vector3(0,0,-.3f));yield return Walk(new Vector3(0,0,26.3f));yield return Walk(new Vector3(6.5f,0,26.3f));Check(loop.Progress==3&&loop.EnteredFromStart,"Normal traversal after retreat uses the same corridor");
  loop.SetAnomaly(1);loop.CrossBoundary(!loop.EnteredFromStart);Check(loop.Progress==0,"Missing an anomaly resets progress");
  loop.SetAnomaly(-1);loop.CrossBoundary(loop.EnteredFromStart);Check(loop.Progress==0,"Turning back on a normal passage resets progress");
  loop.ClearAnomaly();string clean=Signature();for(int i=0;i<CorridorLoop.AnomalyCount;i++){loop.SetAnomaly(i);Check(i==10?loop.horror.Mode==10&&!loop.horror.water.activeSelf:Signature()!=clean,"Anomaly "+i+" changes scene or arms hidden encounter");loop.ClearAnomaly();Check(Signature()==clean,"Anomaly "+i+" restores cleanly");}
  CheckNumbers();for(int i=0;i<8;i++){loop.SetAnomaly(-1);loop.CrossBoundary(!loop.EnteredFromStart);CheckNumbers();}Check(loop.ExitUnlocked&&loop.Progress==8&&loop.exitRoom.activeSelf,"Eight correct decisions unlock final passage");
  yield return Walk(new Vector3(30,0,6.5f));Check(loop.Escaped,"Walking through final exit completes run");
  loop.ResetRun();Check(loop.Progress==0&&!loop.ExitUnlocked&&!loop.Escaped&&loop.ActiveAnomaly==-1,"Restart clears run and anomalies");
  CheckNumbers();
  yield return HorrorChecks();
  report+="RESULT: "+(ok?"PASS":"FAIL");File.WriteAllText("loop-validation.txt",report);Debug.Log(report);
  #if UNITY_EDITOR
  UnityEditor.EditorApplication.Exit(ok?0:1);
  #endif
 }
 void Shot(string name){var c=loop.player.view;var old=c.targetTexture;var rt=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGBHalf);c.targetTexture=rt;for(int i=0;i<6;i++)c.Render();var active=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1920,1080,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1920,1080),0,0);var pixels=t.GetPixels();for(int i=0;i<pixels.Length;i++)pixels[i]=pixels[i].gamma;t.SetPixels(pixels);t.Apply();File.WriteAllBytes("Screenshots/"+name+".png",t.EncodeToPNG());Destroy(t);RenderTexture.active=active;c.targetTexture=old;RenderTexture.ReleaseTemporary(rt);}
 IEnumerator HorrorChecks(){
  var signPosition=loop.sign.transform.position;var signRotation=loop.sign.transform.rotation;
  loop.SetAnomaly(-1);loop.player.Teleport(new Vector3(6.3f,.04f,26.3f),90,true);loop.CrossBoundary(false);var position=loop.player.transform.position;var rotation=loop.player.transform.rotation;
  loop.SetAnomaly(0);loop.player.Teleport(new Vector3(-6.3f,.04f,-.3f),270,true);loop.CrossBoundary(true);Check(Vector3.Distance(position,loop.player.transform.position)<.001f&&Quaternion.Angle(rotation,loop.player.transform.rotation)<.01f,"Advance and retreat arrive at identical entrance pose");
  Check(loop.sign.transform.position==signPosition&&loop.sign.transform.rotation==signRotation,"Sign location never changes between rounds");
  loop.player.Teleport(new Vector3(0,.04f,7),0,true);loop.SetAnomaly(8);yield return new WaitForSeconds(1.4f);Shot("watcher-anomaly");var initial=loop.horror.watcher.transform.position;loop.player.Teleport(loop.player.transform.position,180,true);yield return new WaitForSeconds(.6f);Check(Vector3.Distance(initial,loop.horror.watcher.transform.position)>.1f,"Watcher advances when unseen");loop.ClearAnomaly();
  loop.player.Teleport(new Vector3(0,.04f,9),0,true);loop.SetAnomaly(9);yield return new WaitForSeconds(.3f);Shot("ceiling-anomaly");loop.ClearAnomaly();
  loop.player.Teleport(new Vector3(0,.04f,7),0,true);loop.SetAnomaly(10);yield return new WaitForSeconds(.7f);Check(!loop.horror.water.activeSelf&&!loop.horror.entryWater.activeSelf,"Flood hidden before trigger depth");
  loop.player.Teleport(new Vector3(0,.04f,12),0,true);yield return new WaitForSeconds(.5f);Check(!loop.horror.water.activeSelf&&!loop.horror.entryWater.activeSelf,"Flood warning remains behind corner");yield return new WaitForSeconds(2.8f);var material=loop.horror.water.GetComponent<Renderer>().material;Check(loop.horror.water.activeSelf&&loop.horror.entryWater.activeSelf&&material.GetFloat("_Rise")+material.GetFloat("_Crest")>=1.8f,"Large surge emerges from corner");Shot("flood-anomaly");
  loop.player.Teleport(new Vector3(0,.04f,material.GetFloat("_FrontZ")+1),0,true);yield return null;yield return null;Check(loop.ActiveAnomaly==-1&&loop.Progress==0&&!loop.horror.water.activeSelf&&!loop.horror.entryWater.activeSelf&&!loop.horror.floodSpray.isPlaying,"Flood catch clears both surfaces and spray");
  loop.ResetRun();loop.player.Teleport(new Vector3(-.25f,.04f,15),0,true);yield return new WaitForSeconds(4);Debug.Log("NPC_CHECK distance="+loop.commuter.DistanceWalked+" walking="+loop.commuter.Walking+" anim="+loop.commuter.walking.IsPlaying("Walk")+" pos="+loop.commuter.transform.position+" obstacle="+loop.commuter.LastObstacle+" speed="+loop.commuter.walking["Walk"].speed);Check(loop.commuter.DistanceWalked>3&&loop.commuter.walking.IsPlaying("Walk"),"Commuter walks through every normal passage");Check(Vector3.Distance(loop.commuter.briefcase.position,loop.commuter.rightHand.position)<.3f,"Briefcase follows carrying hand");Shot("commuter-in-game");
  var tile=GameObject.Find("Guide passage 0").GetComponentInChildren<MeshRenderer>();Check(Mathf.Abs(tile.bounds.size.x-.448f)<.01f,"Tactile route is one floor tile wide");  loop.player.Teleport(new Vector3(0,.04f,7),0,true);loop.SetAnomaly(11);yield return new WaitForSeconds(.5f);Shot("blackout-anomaly");loop.ClearAnomaly();Check(!loop.horror.HasVisibleChange,"Horror state clears completely");loop.ResetRun();
 }
}
