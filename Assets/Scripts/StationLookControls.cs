using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[ExecuteAlways]
public class StationLookControls:MonoBehaviour {
 [Header("IŞIK — dolaylı ışık için yeniden hesapla")]
 [InspectorName("Floresan ışık şiddeti"),Range(0,20)] public float fluorescentPower=8;
 [InspectorName("Işık rengi")] public Color lightColor=new Color(.97f,.98f,1);
 [InspectorName("Canlı ışık / parlama"),Range(0,2)] public float realtimePower=.85f;
 [InspectorName("Zeminden dönen ışık"),Range(0,1)] public float bouncePower=.12f;
 [InspectorName("Floresan tüp parlaklığı"),Range(0,15)] public float tubeEmission=7;
 [InspectorName("Tabela parlaklığı"),Range(0,5)] public float signEmission=1.45f;
 [Header("KAMERA / RENK — anında önizleme")]
 [InspectorName("Pozlama"),Range(-3,3)] public float exposure=-.35f;
 [InspectorName("Beyaz dengesi"),Range(-60,60)] public float temperature=-1;
 [InspectorName("Kontrast"),Range(-40,60)] public float contrast=12;
 [InspectorName("Doygunluk"),Range(-100,50)] public float saturation=-5;
 [InspectorName("Kamera görüş açısı"),Range(50,90)] public float fieldOfView=66;
 [Header("DUVAR — yansıma bulanıklığı için pürüzlülüğü artır")]
 [InspectorName("Seramik rengi")] public Color wallColor=new Color(.86f,.86f,.83f);
 [InspectorName("Yansıma pürüzlülüğü"),Range(0,.7f)] public float wallRoughness=.12f;
 [InspectorName("Mikro yüzey dokusu"),Range(0,1)] public float wallMicro=.30f;
 [InspectorName("Seramik kabartısı"),Range(0,2)] public float wallNormal=.7f;
 [InspectorName("Derz koyuluğu"),Range(0,1)] public float grout=.34f;
 [Header("ZEMİN")]
 [InspectorName("Zemin rengi")] public Color floorColor=new Color(.51f,.52f,.51f);
 [InspectorName("Zemin yansıma pürüzlülüğü"),Range(0,1)] public float floorRoughness=.36f;
 [InspectorName("Zemin mikro dokusu"),Range(0,1)] public float floorMicro=.22f;
 [Header("TAVAN / KAPILAR")]
 [InspectorName("Tavan rengi")] public Color ceilingColor=new Color(.70f,.705f,.69f);
 [InspectorName("Tavan parlaklığı"),Range(0,1)] public float ceilingGloss=.34f;
 [InspectorName("Kapı boya rengi")] public Color doorColor=new Color(.49f,.51f,.50f);
 [InspectorName("Kapı parlaklığı"),Range(0,1)] public float doorGloss=.40f;
 [Header("YANSIMA / EFEKTLER")]
 [InspectorName("Yansıma gücü"),Range(0,2)] public float reflectionStrength=1;
 [InspectorName("Ekran uzayında yansıma (SSR)")] public bool screenReflections=false;
 [InspectorName("Bloom"),Range(0,2)] public float bloom=.20f;
 [InspectorName("Bloom eşiği"),Range(.5f,5)] public float bloomThreshold=1.4f;
 [InspectorName("Temas gölgeleri"),Range(0,2)] public float ambientOcclusion=.48f;
 [InspectorName("Kenar kararması"),Range(0,.5f)] public float vignette=.10f;
 [InspectorName("Film greni"),Range(0,.5f)] public float grain=.04f;
 [InspectorName("Motion blur"),Range(0,240)] public float motionBlur=120;
 [HideInInspector] public Material wall,floor,ceiling,door,tubes,sign;
 [HideInInspector] public Light[] lights;
 [HideInInspector] public ReflectionProbe[] probes;
 [HideInInspector] public Camera view;
 [HideInInspector] public PostProcessVolume volume;
 void OnEnable(){Apply();}
 void OnValidate(){
  #if UNITY_EDITOR
  UnityEditor.EditorApplication.delayCall+=DelayedApply;
  #endif
 }
 #if UNITY_EDITOR
 void DelayedApply(){if(this){Apply();UnityEditor.SceneView.RepaintAll();}}
 #endif
 public void Apply(){
  if(lights!=null)foreach(var l in lights){if(!l)continue;if(l.name=="Baked rectangular light"){l.intensity=fluorescentPower;l.color=lightColor;}if(l.name=="Realtime specular support"){l.intensity=realtimePower;l.color=lightColor;}if(l.name.StartsWith("Diffuse floor return"))l.intensity=bouncePower;}
  if(wall){wall.SetColor("_Color",wallColor);wall.SetFloat("_Gloss",1-wallRoughness);wall.SetFloat("_MicroStrength",wallMicro);wall.SetFloat("_NormalStrength",wallNormal);wall.SetFloat("_Grout",grout);}
  if(floor){floor.SetColor("_Color",floorColor);floor.SetFloat("_Gloss",1-floorRoughness);floor.SetFloat("_MicroStrength",floorMicro);}
  if(ceiling){ceiling.color=ceilingColor;ceiling.SetFloat("_Glossiness",ceilingGloss);}if(door){door.color=doorColor;door.SetFloat("_Glossiness",doorGloss);}
  if(tubes)tubes.SetColor("_EmissionColor",new Color(.88f,.94f,1)*tubeEmission);if(sign)sign.SetColor("_EmissionColor",Color.white*signEmission);
  if(probes!=null)foreach(var p in probes)if(p)p.intensity=reflectionStrength;
  if(view){view.fieldOfView=fieldOfView;var b=view.GetComponent<TurningMotionBlur>();if(b)b.maximumShutterAngle=motionBlur;}
  if(volume){var p=Application.isPlaying?volume.profile:volume.sharedProfile;if(p){var c=p.GetSetting<ColorGrading>();c.postExposure.Override(exposure);c.temperature.Override(temperature);c.contrast.Override(contrast);c.saturation.Override(saturation);var b=p.GetSetting<Bloom>();b.intensity.Override(bloom);b.threshold.Override(bloomThreshold);p.GetSetting<AmbientOcclusion>().intensity.Override(ambientOcclusion);p.GetSetting<Vignette>().intensity.Override(vignette);p.GetSetting<Grain>().intensity.Override(grain);p.GetSetting<ScreenSpaceReflections>().enabled.Override(screenReflections);}}
  #if UNITY_EDITOR
  if(!Application.isPlaying){foreach(var m in new[]{wall,floor,ceiling,door,tubes,sign})if(m)UnityEditor.EditorUtility.SetDirty(m);if(volume&&volume.sharedProfile){UnityEditor.EditorUtility.SetDirty(volume.sharedProfile);foreach(var s in volume.sharedProfile.settings)UnityEditor.EditorUtility.SetDirty(s);}}
  #endif
 }
}
