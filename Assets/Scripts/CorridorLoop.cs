using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CorridorLoop:MonoBehaviour {
 [Header("DÖNGÜ KURALLARI")]
 [Range(1,20),InspectorName("Çıkış için doğru karar sayısı")] public int requiredCorrect=8;
 [Range(0,1),InspectorName("Anomali olasılığı")] public float anomalyChance=.5f;
 [InspectorName("Rastgele tohum (0: her oyun farklı)")] public int seed=0;
 [InspectorName("İlk tur normal olsun")] public bool firstRoundNormal=true;
 [Header("SAHNE BAĞLANTILARI")]
 public FirstPerson player;
 public GameObject[] posters;
 public GameObject vent,door,sign,cctv;
 public GameObject[] tactileSection;
 public Renderer[] lightTubes;
 public Light accentLight;
 public Renderer[] numberSigns;
 public Texture2D[] numberTextures;
 public GameObject exitRoom;
 public CorridorHorror horror;
 public StationCommuter commuter;
 public const int AnomalyCount=14;
 int normalStreak;
 [SerializeField] int progress,round;
 [SerializeField] int activeAnomaly=-1;
 public int Progress=>progress;
 public int Round=>round;
 public int ActiveAnomaly=>activeAnomaly;
 public bool ExitUnlocked{get;private set;}
 public bool Escaped{get;private set;}
 public bool EnteredFromStart{get;private set;}=true;
 readonly List<Action> restore=new List<Action>();readonly List<Material> signMaterials=new List<Material>();
 System.Random random;int previousAnomaly=-1;float cooldown,started;
 void Start(){foreach(var r in numberSigns)if(r)signMaterials.Add(r.material);ResetRun();}
 void Update(){
  if(Escaped)return;
  var p=player.transform.position;
  if(ExitUnlocked){if(p.x>26&&p.z>6){Escaped=true;Debug.Log("STATION_ESCAPED");}return;}
  if(Time.time<cooldown)return;
  if(p.x>6.2f&&p.z>24.6f&&p.z<28.1f)CrossBoundary(false);
  else if(p.x< -6.2f&&p.z> -2.1f&&p.z<1.4f)CrossBoundary(true);
 }
 public void ResetRun(){
  ClearAnomaly();player.ResetBreath();random=new System.Random(seed==0?Environment.TickCount:seed);progress=0;round=0;previousAnomaly=-1;ExitUnlocked=false;Escaped=false;EnteredFromStart=true;started=Time.time;
  normalStreak=firstRoundNormal?1:0;if(exitRoom)exitRoom.SetActive(false);player.Teleport(new Vector3(-4.8f,.04f,-.3f),90,true);cooldown=Time.time+.5f;
  SetAnomaly(firstRoundNormal?-1:RollAnomaly());UpdateSigns();if(commuter)commuter.ResetRoute();Debug.Log("STATION_LOOP_STARTED");
 }
 int RollAnomaly(){
  // The first encounter introduces a visible threat; later rounds mix quiet observation and pursuit.
  if(round==1){normalStreak=0;previousAnomaly=random.Next(2)==0?12:13;return previousAnomaly;}
  float chance=Mathf.Lerp(Mathf.Max(.62f,anomalyChance),.82f,Mathf.Clamp01(progress/6f));
  if(normalStreak<1&&random.NextDouble()>chance){normalStreak++;return -1;}
  normalStreak=0;
  int[] pool={0,1,2,3,4,5,6,7,8,8,9,9,10,11,12,12,13,13};
  int next=pool[random.Next(pool.Length)];
  if(next==previousAnomaly)next=(next+1)%AnomalyCount;
  previousAnomaly=next;return next;
 }
 public void CrossBoundary(bool startBoundary){
  if(ExitUnlocked)return;
  bool turnedBack=startBoundary;
  bool correct=turnedBack==(activeAnomaly>=0);
  progress=correct?progress+1:0;round++;ClearAnomaly();
  if(progress>=requiredCorrect){ExitUnlocked=true;if(exitRoom)exitRoom.SetActive(true);player.Teleport(new Vector3(30,.04f,.6f),0,true);UpdateSigns();Debug.Log("STATION_EXIT_UNLOCKED");return;}
  var old=player.transform.position;
  EnteredFromStart=true;
  var destination=startBoundary?new Vector3(-12.4f-old.x,old.y,-.6f-old.z):old+new Vector3(-12.4f,0,-26.6f);
  player.Teleport(destination,player.transform.eulerAngles.y+(startBoundary?180:0),false);cooldown=Time.time+.45f;SetAnomaly(RollAnomaly());UpdateSigns();if(commuter)commuter.ResetRoute();
  Debug.Log("STATION_LOOP_DECISION round="+round+" correct="+correct+" progress="+progress);
 }
 void UpdateSigns(){int index=Mathf.Clamp(progress,0,numberTextures.Length-1);foreach(var m in signMaterials){m.mainTexture=numberTextures[index];m.SetTexture("_EmissionMap",numberTextures[index]);}}
 void TransformChange(Transform t,Vector3 position,Quaternion rotation,Vector3 scale){var p=t.localPosition;var q=t.localRotation;var s=t.localScale;restore.Add(()=>{if(t){t.localPosition=p;t.localRotation=q;t.localScale=s;}});t.localPosition=position;t.localRotation=rotation;t.localScale=scale;}
 void Hide(GameObject go){if(!go)return;bool was=go.activeSelf;restore.Add(()=>{if(go)go.SetActive(was);});go.SetActive(false);}
 public void SetAnomaly(int index){ClearAnomaly();activeAnomaly=index;if(index<0)return;
  switch(index){
   case 0:Hide(posters[2]);break;
   case 1:{var t=posters[1].transform;TransformChange(t,t.localPosition,t.localRotation*Quaternion.Euler(0,0,180),t.localScale);break;}
   case 2:{var t=vent.transform;TransformChange(t,t.localPosition,t.localRotation,t.localScale*2.0f);break;}
   case 3:{var t=door.transform;TransformChange(t,t.localPosition+Vector3.left*.40f,t.localRotation,t.localScale);break;}
   case 4:if(horror)horror.Begin(index);break;
   case 5:foreach(var g in tactileSection)Hide(g);break;
   case 6:{var t=cctv.transform;TransformChange(t,t.localPosition-Vector3.up*1.4f,t.localRotation,t.localScale*1.5f);break;}
   case 7:
    foreach(var r in lightTubes){var old=r.sharedMaterial;var m=new Material(old);m.SetColor("_EmissionColor",new Color(1,.015f,.01f)*7);r.sharedMaterial=m;restore.Add(()=>{if(r)r.sharedMaterial=old;Destroy(m);});}
    if(accentLight){var color=accentLight.color;float intensity=accentLight.intensity;restore.Add(()=>{if(accentLight){accentLight.color=color;accentLight.intensity=intensity;}});accentLight.color=Color.red;accentLight.intensity=2;}break;
   case 8:case 9:case 10:case 11:case 12:case 13:if(horror)horror.Begin(index);break;
  }
 }
 public void ClearAnomaly(){if(horror)horror.Clear();for(int i=restore.Count-1;i>=0;i--)restore[i]();restore.Clear();activeAnomaly=-1;}
 void OnDestroy(){ClearAnomaly();foreach(var m in signMaterials)if(m)Destroy(m);}
 void OnGUI(){if(!Escaped)return;GUI.color=Color.white;var style=new GUIStyle(GUI.skin.box){fontSize=28,alignment=TextAnchor.MiddleCenter};GUI.Box(new Rect(Screen.width/2-270,Screen.height/2-100,540,200),"ÇIKIŞ 8\nKoridordan çıktın.\n\nR — Yeniden başla",style);}
}
