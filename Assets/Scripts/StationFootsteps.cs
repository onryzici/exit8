using UnityEngine;

[RequireComponent(typeof(CharacterController),typeof(AudioSource))]
public class StationFootsteps : MonoBehaviour {
 public AudioClip[] clips;
 AudioSource source;CharacterController motor;FirstPerson player;Vector3 previous;
 float distance,airborneTime,nextStepAt;int last=-1;bool grounded;
 public int StepsPlayed {get;private set;}
 public float LastStepTime {get;private set;}=-100;
 void Awake(){
  motor=GetComponent<CharacterController>();player=GetComponent<FirstPerson>();source=GetComponent<AudioSource>();
  source.playOnAwake=false;source.loop=false;source.spatialBlend=0;source.volume=.60f;source.dopplerLevel=0;previous=transform.position;
 }
 void LateUpdate(){
  Vector3 delta=transform.position-previous;previous=transform.position;delta.y=0;
  bool onFloor=motor.isGrounded;
  if(delta.sqrMagnitude>4||!player.InputCaptured){distance=0;airborneTime=0;grounded=onFloor;return;}
  bool crouch=motor.height<1.5f;
  // A floor seam or the raised tactile strip is not a jump landing.
  if(!onFloor)airborneTime+=Time.deltaTime;
  if(onFloor&&!grounded&&airborneTime>.12f&&motor.velocity.y<-.5f){
   if(Play(crouch?.28f:.72f))distance=0;
  }
  if(onFloor)airborneTime=0;
  grounded=onFloor;
  if(!onFloor||delta.sqrMagnitude<.000001f){if(airborneTime>.12f)distance=0;return;}
  distance+=delta.magnitude;
  float speed=delta.magnitude/Mathf.Max(Time.deltaTime,.0001f);
  float stride=crouch?.95f:speed>3.4f?1.5f:1.28f;
  if(distance>=stride&&Play(crouch?.27f:speed>3.4f?.95f:.65f))distance%=stride;
 }
 bool Play(float volume){
  if(clips==null||clips.Length==0||Time.time<nextStepAt)return false;
  int next=Random.Range(0,clips.Length);if(clips.Length>1&&next==last)next=(next+1)%clips.Length;last=next;
  source.pitch=Random.Range(.985f,1.015f);source.panStereo=StepsPlayed%2==0?-.045f:.045f;
  nextStepAt=Time.time+Mathf.Max(.24f,clips[next].length/source.pitch+.012f);
  source.PlayOneShot(clips[next],volume*Random.Range(.96f,1.02f));LastStepTime=Time.time;StepsPlayed++;return true;
 }
}
