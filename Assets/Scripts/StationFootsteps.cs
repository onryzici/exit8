using UnityEngine;

[RequireComponent(typeof(CharacterController),typeof(AudioSource))]
public class StationFootsteps : MonoBehaviour {
 public AudioClip[] clips;
 AudioSource source; CharacterController motor; Vector3 previous; float distance; int last=-1; bool grounded;
 public int StepsPlayed { get; private set; }
 void Awake(){motor=GetComponent<CharacterController>();source=GetComponent<AudioSource>();source.playOnAwake=false;source.loop=false;source.spatialBlend=0;source.volume=.68f;previous=transform.position;}
 void LateUpdate(){
  Vector3 delta=transform.position-previous;previous=transform.position;delta.y=0;
  bool onFloor=motor.isGrounded;
  if(delta.sqrMagnitude>4 || !GetComponent<FirstPerson>().InputCaptured){distance=0;grounded=onFloor;return;}
  bool crouch=motor.height<1.5f;
  if(onFloor&&!grounded && motor.velocity.y<-.5f)Play(crouch?.28f:.72f);
  grounded=onFloor;
  if(!onFloor || delta.sqrMagnitude<.000001f){distance=0;return;}
  distance+=delta.magnitude;
  float speed=delta.magnitude/Mathf.Max(Time.deltaTime,.0001f);
  float stride=crouch?.95f:speed>3.4f?1.5f:1.28f;
  if(distance>=stride){distance%=stride;Play(crouch?.27f:speed>3.4f?.95f:.65f);}
 }
 void Play(float volume){
  if(clips==null||clips.Length==0)return;
  int next=Random.Range(0,clips.Length);if(clips.Length>1&&next==last)next=(next+1)%clips.Length;last=next;
  source.pitch=Random.Range(.97f,1.035f);source.panStereo=StepsPlayed%2==0?-.045f:.045f;source.PlayOneShot(clips[next],volume*Random.Range(.94f,1.04f));StepsPlayed++;
 }
}
