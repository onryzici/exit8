using System.Collections.Generic;
using UnityEngine;

// Pose the existing skinned human mesh after sampling the walk, preserving its authored skin weights.
public class HollowActor:MonoBehaviour {
 public bool Crawl;
 public float Movement;
 public AnimationClip walk;
 Animation animationPlayer;
 readonly Dictionary<string,Transform> bones=new Dictionary<string,Transform>();
 float clock;
 void Awake(){foreach(var bone in GetComponentsInChildren<Transform>())bones[bone.name]=bone;}
 void Start(){
  animationPlayer=GetComponent<Animation>();if(!animationPlayer)animationPlayer=gameObject.AddComponent<Animation>();
  animationPlayer.AddClip(walk,"Hollow walk");animationPlayer.cullingType=AnimationCullingType.AlwaysAnimate;animationPlayer.Play("Hollow walk");animationPlayer["Hollow walk"].speed=0;
 }
 Transform Bone(string name){return bones.TryGetValue("Bip01 "+name,out var t)?t:null;}
 void Aim(Transform parent,Transform child,Vector3 target){
  if(!parent||!child)return;
  parent.rotation=Quaternion.FromToRotation(child.position-parent.position,target-parent.position)*parent.rotation;
  child.position=target;
 }
 void LateUpdate(){
  if(!animationPlayer)return;
  clock+=Time.deltaTime*Mathf.Clamp(Movement,.0f,2);
  animationPlayer["Hollow walk"].time=.18f+clock;animationPlayer.Sample();
  if(bones.TryGetValue("Bip01",out var root)){var p=root.localPosition;p.x=0;p.z=0;root.localPosition=p;}
  if(!Crawl){
   foreach(string side in new[]{"L","R"}){
    var arm=Bone(side+" Forearm");var hand=Bone(side+" Hand");
    if(arm)arm.localPosition*=1.18f;if(hand)hand.localPosition*=1.28f;
   }
   var leftFoot=Bone("L Foot");var rightFoot=Bone("R Foot");
   if(root&&leftFoot&&rightFoot)root.position+=Vector3.up*(transform.parent.position.y+.075f-Mathf.Min(leftFoot.position.y,rightFoot.position.y));
   var head=Bone("Head");if(head)head.localRotation*=Quaternion.Euler(0,12,32+Mathf.Sin(Time.time*1.3f)*2);
  }else{
   var space=transform.parent;
   for(int i=0;i<2;i++){
    string side=i==0?"L":"R";float sign=i==0?-1:1;
    float phase=clock*5+i*Mathf.PI,step=Mathf.Sin(phase)*.16f,lift=Mathf.Max(0,Mathf.Cos(phase))*.08f;
    var upper=Bone(side+" UpperArm");var lower=Bone(side+" Forearm");var hand=Bone(side+" Hand");
    Aim(upper,lower,space.TransformPoint(new Vector3(sign*.55f,.24f,.1f+step)));
    Aim(lower,hand,space.TransformPoint(new Vector3(sign*.72f,.035f+lift,.60f+step)));
    var thigh=Bone(side+" Thigh");var calf=Bone(side+" Calf");var foot=Bone(side+" Foot");
    Aim(thigh,calf,space.TransformPoint(new Vector3(sign*.43f,.29f,-.8f-step)));
    Aim(calf,foot,space.TransformPoint(new Vector3(sign*.52f,.055f+lift,-.3f-step)));
   }
  }
 }
}
