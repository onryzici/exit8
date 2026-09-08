using UnityEngine;
using UnityEngine.Rendering;
public class StationCommuter:MonoBehaviour {
 public FirstPerson player;
 public Animation walking;
 public Transform hips,rightHand,briefcase;
 public Transform leftFoot,rightFoot;
 public Renderer[] footShadows;
 MaterialPropertyBlock shadowProperties;
 public AudioClip[] footsteps;
 [Range(.5f,1.8f)] public float speed=1.15f;
 public string LastObstacle {get;private set;}="";
 public bool Walking=>started&&!finished&&!floodHidden;
 public void HideInFlood(){floodHidden=true;walking["Walk"].speed=0;if(audioSource)audioSource.Stop();foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;}
 public void RestoreAfterFlood(){if(!floodHidden)return;floodHidden=false;foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=!finished;walking["Walk"].speed=Walking?speed/1.15f:0;}
 public float DistanceWalked{get;private set;}
 CharacterController motor;AudioSource audioSource;int segment;bool started,finished,floodHidden;float stepDistance;Transform lightingAnchor;int lastFootstep=-1;
 readonly Vector3[] route={new Vector3(5.4f,0,26.3f),new Vector3(.95f,0,26.3f),new Vector3(.95f,0,1.3f),new Vector3(0,0,-.3f),new Vector3(-5.5f,0,-.3f)};
 void Awake(){motor=GetComponent<CharacterController>();motor.minMoveDistance=0;audioSource=GetComponent<AudioSource>();
  lightingAnchor=new GameObject("Commuter light sample").transform;lightingAnchor.SetParent(transform,false);
  foreach(var renderer in GetComponentsInChildren<Renderer>()){
   renderer.probeAnchor=lightingAnchor;renderer.lightProbeUsage=LightProbeUsage.BlendProbes;
  }
  UpdateLighting();
  audioSource.playOnAwake=false;audioSource.loop=false;audioSource.spatialBlend=1;audioSource.dopplerLevel=0;audioSource.minDistance=1;audioSource.maxDistance=16;audioSource.rolloffMode=AudioRolloffMode.Linear;audioSource.reverbZoneMix=.35f;
 }
 public void ResetRoute(){if(!motor)motor=GetComponent<CharacterController>();motor.enabled=false;transform.SetPositionAndRotation(route[0]+Vector3.up*.03f,Quaternion.Euler(0,270,0));motor.enabled=true;segment=1;started=true;finished=false;floodHidden=false;DistanceWalked=0;stepDistance=0;foreach(var r in GetComponentsInChildren<Renderer>(true))r.enabled=true;walking.Play("Walk");walking["Walk"].time=0;walking["Walk"].speed=speed/1.15f;if(audioSource)audioSource.Stop();}
 void Update(){if(!Walking)return;
  var delta=route[segment]-transform.position;delta.y=0;if(delta.magnitude<.12f){if(segment<route.Length-1)segment++;else{finished=true;walking["Walk"].speed=0;foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;return;}}
  var target=route[segment];if(segment==2&&Vector3.Distance(player.transform.position,transform.position)<2&&player.transform.position.x>.45f)target.x=1.55f;
  delta=target-transform.position;delta.y=0;var before=transform.position;motor.Move(delta.normalized*speed*Time.deltaTime+Vector3.down*2*Time.deltaTime);var moved=transform.position-before;moved.y=0;
  if(delta.sqrMagnitude>.001f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),1-Mathf.Exp(-6*Time.deltaTime));walking["Walk"].speed=moved.magnitude>.0001f?speed/1.15f:0;DistanceWalked+=moved.magnitude;stepDistance+=moved.magnitude;
  if(stepDistance>.61f&&footsteps.Length>0){
   stepDistance%=.61f;
   int next=Random.Range(0,footsteps.Length);if(footsteps.Length>1&&next==lastFootstep)next=(next+1)%footsteps.Length;lastFootstep=next;
   audioSource.pitch=1;audioSource.PlayOneShot(footsteps[next],.40f*FootstepAudibility);
  }
 }
 // The baked samples cover x +/-1.5 and z 0..26, but the walking route includes side passages.
 // A shared chest-height anchor avoids extrapolated black lighting and mismatched bag/body shading.
 void UpdateLighting(){
  if(!lightingAnchor)return;
  lightingAnchor.position=new Vector3(Mathf.Clamp(transform.position.x,-1.4f,1.4f),1.3f,Mathf.Clamp(transform.position.z,.4f,25.4f));
 }
 public Vector3 LightingSamplePosition=>lightingAnchor?lightingAnchor.position:transform.position;
 public float FootstepAudibility{
  get{
   if(!player||!player.view)return 0;
   Vector3 ear=player.view.transform.position;
   Vector3 foot=transform.position+Vector3.up*.25f;
   // Ignore both character capsules; only corridor geometry should muffle the sound.
   foreach(var hit in Physics.RaycastAll(ear,(foot-ear).normalized,Vector3.Distance(ear,foot),Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
    if(!hit.transform.IsChildOf(transform)&&!hit.transform.IsChildOf(player.transform))return .12f;
   return 1;
  }
 }
 void OnControllerColliderHit(ControllerColliderHit hit){if(hit.normal.y<.9f)LastObstacle=hit.collider.name+" "+hit.normal;}
 // The grip is measured from the palm, not the wrist. Apply after the walk animation.
 Transform[] carryingFingers;Vector3 gripInBag;bool gripReady;
 public Vector3 GripPosition=>rightHand.TransformPoint(new Vector3(-.098f,.025f,0));
 public float GripError=>gripReady?Vector3.Distance(briefcase.TransformPoint(gripInBag),GripPosition):0;
 void UpdateGrip(){
  if(!gripReady){
   carryingFingers=rightHand.GetComponentsInChildren<Transform>();
   foreach(var r in briefcase.GetComponentsInChildren<Renderer>())if(r.name=="Leather grip")gripInBag=briefcase.InverseTransformPoint(r.bounds.center);
   gripReady=true;
  }
  foreach(var finger in carryingFingers){
   string name=finger.name;
   if(!name.StartsWith("Bip01 R Finger")||name.StartsWith("Bip01 R Finger0"))continue;
   string digit=name.Substring("Bip01 R Finger".Length);
   // Negative Z curls this rig's fingers into the palm around the handle.
   var angles=finger.localEulerAngles;angles.z=digit.Length==1?-65:digit.EndsWith("1")?-75:-40;
   finger.localRotation=Quaternion.Euler(angles);
  }
  var alongHandle=Vector3.ProjectOnPlane(rightHand.forward,Vector3.up).normalized;
  if(alongHandle.sqrMagnitude<.1f)alongHandle=transform.forward;
  briefcase.rotation=Quaternion.LookRotation(alongHandle,Vector3.up);
  briefcase.position+=GripPosition-briefcase.TransformPoint(gripInBag);
 }
 void LateUpdate(){UpdateLighting();if(hips){var p=hips.localPosition;p.x=Mathf.Clamp(p.x,-.035f,.035f);p.z=0;hips.localPosition=p;}if(briefcase&&rightHand)UpdateGrip();if(footShadows!=null&&footShadows.Length==2){if(shadowProperties==null)shadowProperties=new MaterialPropertyBlock();for(int i=0;i<2;i++){var foot=i==0?leftFoot:rightFoot;var p=foot.position;footShadows[i].transform.SetPositionAndRotation(new Vector3(p.x,.009f,p.z),Quaternion.Euler(90,transform.eulerAngles.y,0));shadowProperties.SetColor("_Color",new Color(0,0,0,.22f*Mathf.InverseLerp(.32f,.09f,p.y)));footShadows[i].SetPropertyBlock(shadowProperties);}}}
}
