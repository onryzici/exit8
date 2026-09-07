using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[DefaultExecutionOrder(100)]
public class TurningMotionBlur : MonoBehaviour {
 [Range(0,240)] public float maximumShutterAngle=120;
 public Shader directionalShader;
 Material directionalMaterial; Vector2 smear;
 MotionBlur blur; PostProcessProfile runtimeProfile; Quaternion previous;
 void Start(){
  var volume=FindFirstObjectByType<PostProcessVolume>();
  if(volume){runtimeProfile=volume.profile;blur=runtimeProfile.GetSetting<MotionBlur>();}
  previous=transform.rotation;
 }
 void LateUpdate(){
  var delta=Quaternion.Inverse(previous)*transform.rotation;var angles=delta.eulerAngles;
  var angular=new Vector2(Mathf.DeltaAngle(0,angles.y),-Mathf.DeltaAngle(0,angles.x))/Mathf.Max(Time.deltaTime,.0001f);
  float speed=angular.magnitude;previous=transform.rotation;
  var fps=GetComponentInParent<FirstPerson>();var display=fps.GetComponent<StationDisplay>();bool active=fps.InputCaptured&&!(display&&display.MenuOpen)&&speed<1500;
  smear=active?Vector2.ClampMagnitude(angular/180f,1)*(.009f*maximumShutterAngle/120f):Vector2.zero;
  if(blur==null)return;
  blur.enabled.Override(true);blur.sampleCount.Override(16);
  float target=active?Mathf.SmoothStep(0,maximumShutterAngle,Mathf.InverseLerp(3,100,speed)):0;
  blur.shutterAngle.value=Mathf.Lerp(blur.shutterAngle.value,target,1-Mathf.Exp(-18*Time.deltaTime));
 }
 void OnRenderImage(RenderTexture source,RenderTexture destination){
  if(!directionalShader||smear.sqrMagnitude<.00000001f){Graphics.Blit(source,destination);return;}
  if(!directionalMaterial)directionalMaterial=new Material(directionalShader){hideFlags=HideFlags.HideAndDontSave};
  directionalMaterial.SetVector("_Smear",new Vector4(smear.x,smear.y*source.width/source.height,0,0));Graphics.Blit(source,destination,directionalMaterial);
 }
 void OnDestroy(){if(directionalMaterial)Destroy(directionalMaterial);}
}
