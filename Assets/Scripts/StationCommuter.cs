using UnityEngine;
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
 public bool Walking=>started&&!finished;
 public float DistanceWalked{get;private set;}
 CharacterController motor;AudioSource audioSource;int segment;bool started,finished;float stepDistance;
 readonly Vector3[] route={new Vector3(.95f,0,24.4f),new Vector3(.95f,0,1.3f),new Vector3(0,0,-.3f),new Vector3(-5.5f,0,-.3f)};
 void Awake(){motor=GetComponent<CharacterController>();motor.minMoveDistance=0;audioSource=GetComponent<AudioSource>();}
 public void ResetRoute(){if(!motor)motor=GetComponent<CharacterController>();motor.enabled=false;transform.SetPositionAndRotation(route[0]+Vector3.up*.03f,Quaternion.Euler(0,180,0));motor.enabled=true;segment=1;started=false;finished=false;DistanceWalked=0;stepDistance=0;foreach(var r in GetComponentsInChildren<Renderer>(true))r.enabled=true;walking.Play("Walk");walking["Walk"].time=0;walking["Walk"].speed=0;if(audioSource)audioSource.Stop();}
 void Update(){if(!started&&player.transform.position.z>1.2f&&Mathf.Abs(player.transform.position.x)<2.2f)started=true;if(!started||finished)return;
  var delta=route[segment]-transform.position;delta.y=0;if(delta.magnitude<.12f){if(segment<route.Length-1)segment++;else{finished=true;walking["Walk"].speed=0;return;}}
  var target=route[segment];if(segment==1&&Vector3.Distance(player.transform.position,transform.position)<2&&player.transform.position.x>.45f)target.x=1.55f;
  delta=target-transform.position;delta.y=0;var before=transform.position;motor.Move(delta.normalized*speed*Time.deltaTime+Vector3.down*2*Time.deltaTime);var moved=transform.position-before;moved.y=0;
  if(delta.sqrMagnitude>.001f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),1-Mathf.Exp(-6*Time.deltaTime));walking["Walk"].speed=moved.magnitude>.0001f?speed/1.15f:0;DistanceWalked+=moved.magnitude;stepDistance+=moved.magnitude;
  if(stepDistance>.61f&&footsteps.Length>0){stepDistance%=.61f;audioSource.pitch=1;audioSource.PlayOneShot(footsteps[Random.Range(0,footsteps.Length)],.48f);}
 }
 void OnControllerColliderHit(ControllerColliderHit hit){if(hit.normal.y<.9f)LastObstacle=hit.collider.name+" "+hit.normal;}
 void LateUpdate(){if(hips){var p=hips.localPosition;p.x=Mathf.Clamp(p.x,-.035f,.035f);p.z=0;hips.localPosition=p;}if(briefcase&&rightHand){briefcase.position=rightHand.position-Vector3.up*.205f+transform.right*.035f;briefcase.rotation=transform.rotation*Quaternion.Euler(Mathf.Sin(Time.time*6)*1.5f,0,Mathf.Sin(Time.time*6)*2);}if(footShadows!=null&&footShadows.Length==2){if(shadowProperties==null)shadowProperties=new MaterialPropertyBlock();for(int i=0;i<2;i++){var foot=i==0?leftFoot:rightFoot;var p=foot.position;footShadows[i].transform.SetPositionAndRotation(new Vector3(p.x,.009f,p.z),Quaternion.Euler(90,transform.eulerAngles.y,0));shadowProperties.SetColor("_Color",new Color(0,0,0,.22f*Mathf.InverseLerp(.32f,.09f,p.y)));footShadows[i].SetPropertyBlock(shadowProperties);}}}
}
