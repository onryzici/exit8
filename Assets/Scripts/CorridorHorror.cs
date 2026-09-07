using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CorridorHorror:MonoBehaviour {
 public CorridorLoop loop;
 public GameObject watcher,water,entryWater; public ParticleSystem floodSpray;
 public Light emergency;
 public AudioClip[] footsteps;
 [Range(0,1)] public float soundVolume=.55f;
 [Range(.1f,3)] public float stalkingSpeed=1.2f;
 [Range(3,8)] public float floodSpeed=5.5f;
 [Range(5,18)] public float floodTriggerZ=11.5f;
 [Range(.2f,3)] public float floodWarningSeconds=1.2f;
 public bool FloodTriggered=>floodStarted;
 public bool FloodVisible=>water.activeSelf;
 public int Mode{get;private set;}=-1;
 public bool HasVisibleChange=>watcher.activeSelf||water.activeSelf||emergency.gameObject.activeSelf||Mode==4;
 AudioSource source;AudioClip knock,breath,rush;Vector3 doorPosition;Quaternion doorRotation;ColorGrading grade;float exposure,age,nextSound,floodTravel,warningAt;bool initialized,floodStarted;Material waterMaterial,entryMaterial;
 void Init(){if(initialized)return;initialized=true;source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=1;source.minDistance=1.5f;source.maxDistance=24;source.rolloffMode=AudioRolloffMode.Logarithmic;source.dopplerLevel=0;source.reverbZoneMix=.75f;knock=Sound(false);breath=Sound(true);var data=new float[22050*4];var rng=new System.Random(85);float noise=0;for(int i=0;i<data.Length;i++){noise=Mathf.Lerp(noise,(float)rng.NextDouble()*2-1,.13f);data[i]=noise*(.45f+.1f*Mathf.Sin(i/22050f*5));}rush=AudioClip.Create("Water rushing through passage",data.Length,1,22050,false);rush.SetData(data,0);waterMaterial=water.GetComponent<Renderer>().material;entryMaterial=entryWater.GetComponent<Renderer>().material;}
 AudioClip Sound(bool breathing){int rate=22050;float duration=breathing?2.4f:.65f;var data=new float[(int)(rate*duration)];var random=new System.Random(breathing?391:519);float low=0;for(int i=0;i<data.Length;i++){float t=i/(float)rate;float noise=(float)random.NextDouble()*2-1;low=Mathf.Lerp(low,noise,.055f);data[i]=breathing?low*Mathf.Pow(Mathf.Sin(Mathf.PI*t/duration),2)*.6f:(Mathf.Sin(t*2*Mathf.PI*71)*.42f+Mathf.Sin(t*2*Mathf.PI*137)*.22f+noise*.10f)*Mathf.Exp(-12*t)*Mathf.Min(1,t*500);}var c=AudioClip.Create(breathing?"Breathing air":"Door impact",data.Length,1,rate,false);c.SetData(data,0);return c;}
 public void Begin(int index){Clear();Init();Mode=index;age=0;nextSound=.35f;doorPosition=loop.door.transform.localPosition;doorRotation=loop.door.transform.localRotation;
  if(index==4)loop.door.transform.localPosition=doorPosition+Vector3.left*.004f;
  if(index==8||index==9||index==11){watcher.SetActive(true);watcher.transform.SetPositionAndRotation(new Vector3(.65f,index==9?3.38f:0,index==9?17:21),index==9?Quaternion.Euler(0,180,180):Quaternion.Euler(0,180,0));}
  if(index==10){floodTravel=0;warningAt=-1;floodStarted=false;water.SetActive(false);entryWater.SetActive(false);waterMaterial.SetFloat("_Rise",1.25f);waterMaterial.SetFloat("_Crest",.85f);entryMaterial.SetFloat("_Rise",1.25f);entryMaterial.SetFloat("_Crest",.85f);}  if(index==11){var volume=FindFirstObjectByType<PostProcessVolume>();grade=volume.profile.GetSetting<ColorGrading>();exposure=grade.postExposure.value;emergency.gameObject.SetActive(true);}
 }
 void Update(){if(Mode<0)return;float dt=Time.deltaTime;age+=dt;var p=loop.player.transform.position;bool inside=Mathf.Abs(p.x)<2.4f&&p.z>2&&p.z<25;
  if(Mode==4){float pulse=Mathf.Pow(Mathf.Max(0,Mathf.Sin(age*8)),12);loop.door.transform.localPosition=doorPosition+Vector3.left*(.004f+pulse*.009f);source.transform.position=loop.door.transform.position+Vector3.up;
   if(age>nextSound&&inside){source.pitch=.86f;source.PlayOneShot(knock,soundVolume);nextSound=age+(Mathf.Sin(age)>0?.36f:2.1f);}}
  if(Mode==8&&inside){var head=watcher.transform.position+Vector3.up*1.8f;var v=loop.player.view.WorldToViewportPoint(head);bool looking=v.z>0&&v.x>.03f&&v.x<.97f&&v.y>0&&v.y<1&&!Physics.Linecast(loop.player.view.transform.position,head);
   if(!looking&&Vector3.Distance(p,watcher.transform.position)>3.5f){var target=new Vector3(p.x,0,p.z);watcher.transform.position=Vector3.MoveTowards(watcher.transform.position,target,stalkingSpeed*dt);}
   var direction=p-watcher.transform.position;direction.y=0;if(direction.sqrMagnitude>.01f)watcher.transform.rotation=Quaternion.LookRotation(direction);
  }
  if(Mode==9&&inside){var target=new Vector3(Mathf.Sin(age*.65f)*.3f,3.38f,Mathf.Max(p.z+3,9));watcher.transform.position=Vector3.MoveTowards(watcher.transform.position,target,.55f*dt);}
  if((Mode==8||Mode==9)&&inside&&age>nextSound){source.transform.position=watcher.transform.position+Vector3.up*(Mode==9?-1.8f:1.8f);source.pitch=Mode==9?.68f:.82f;source.PlayOneShot(breath,soundVolume*.65f);nextSound=age+3.8f;}
  if(Mode==10){
   if(inside&&p.z>=floodTriggerZ&&warningAt<0){warningAt=age;source.transform.position=new Vector3(5.6f,1.2f,26.3f);source.clip=rush;source.pitch=.65f;source.volume=soundVolume;source.loop=true;source.Play();}
   if(warningAt>=0&&age-warningAt>=floodWarningSeconds){floodStarted=true;floodTravel=(age-warningAt-floodWarningSeconds)*floodSpeed;entryWater.SetActive(true);float entryLength=Mathf.Min(6.4f,.4f+floodTravel);entryWater.transform.SetPositionAndRotation(new Vector3(6.2f-entryLength*.5f,.01f,26.3f),Quaternion.Euler(0,90,0));entryWater.transform.localScale=new Vector3(3.34f,1,entryLength);entryMaterial.SetFloat("_FrontZ",6.2f-entryLength);
    if(floodTravel>3.8f){water.SetActive(true);float length=Mathf.Min(27,.4f+floodTravel-3.8f);float front=26.3f-length;water.transform.localScale=new Vector3(4.46f,1,length);water.transform.position=new Vector3(0,.01f,26.3f-length*.5f);waterMaterial.SetFloat("_FrontZ",front);source.transform.position=new Vector3(0,1,front+.5f);if(floodSpray){floodSpray.transform.position=new Vector3(0,1.35f,front+.6f);if(!floodSpray.isPlaying)floodSpray.Play();}
     if(inside&&p.z>front+.4f){loop.ResetRun();return;}
    }
   }
  }  if(Mode==11){float target=inside?exposure-3.2f:exposure;grade.postExposure.value=Mathf.Lerp(grade.postExposure.value,target,1-Mathf.Exp(-3*dt));emergency.intensity=8+Mathf.Sin(age*1.7f)*1.3f;
   if(inside&&age>nextSound&&footsteps.Length>0){source.transform.position=p-loop.player.transform.forward*2.8f;source.pitch=.68f;source.PlayOneShot(footsteps[(int)(age*3)%footsteps.Length],soundVolume*.8f);nextSound=age+.62f;}}
 }
 public void Clear(){if(Mode==4&&loop&&loop.door){loop.door.transform.localPosition=doorPosition;loop.door.transform.localRotation=doorRotation;}if(grade){grade.postExposure.value=exposure;grade=null;}if(source){source.Stop();source.loop=false;source.volume=1;}if(watcher)watcher.SetActive(false);if(water)water.SetActive(false);if(entryWater)entryWater.SetActive(false);if(floodSpray)floodSpray.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);if(emergency)emergency.gameObject.SetActive(false);Mode=-1;}
 void OnDestroy(){Clear();if(knock)Destroy(knock);if(breath)Destroy(breath);if(rush)Destroy(rush);if(waterMaterial)Destroy(waterMaterial);if(entryMaterial)Destroy(entryMaterial);}
}
