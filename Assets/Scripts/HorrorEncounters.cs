using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class HorrorEncounters : MonoBehaviour
{
    CorridorHorror host;
    CorridorLoop Loop=>host.loop;
    GameObject standing,crawler,bloodRoot,doorRoot;
    Transform standingLight,crawlerLight,doorLight;
    readonly List<Material> materials=new List<Material>();
    Material blood,puddle;
    ParticleSystem drips;
    AudioSource voice,ambience;
    AudioClip scrape,impact,drip,heartbeat;
    Vector3 savedDoor;
    Quaternion savedDoorRotation;
    Vector3 doorInward,openingPosition;Transform doorOpening;ColorGrading encounterGrade;float savedExposure;
    float age,alert,unseen,catchAge=-1,nextCue,pressure;
    int mode=-1;
    bool armed;
    public bool HasVisibleChange=>mode==12||mode==13;
    public bool Catching=>catchAge>=0;
    public float Pressure=>pressure;
    public bool Armed=>armed;
    Material Material(string shader,Color color){
        var template=Resources.Load<Material>("Horror/"+shader.Substring(shader.LastIndexOf("/")+1));
        var m=template?new Material(template):new Material(Shader.Find(shader));m.color=color;materials.Add(m);return m;
    }
    public void Initialize(CorridorHorror owner){
        host=owner;
        var skin=Material("Station/CreatureSkin",new Color(.40f,.36f,.31f));
        var dark=Material("Standard",new Color(.012f,.007f,.007f));dark.SetFloat("_Glossiness",.16f);
        var teeth=Material("Standard",new Color(.35f,.30f,.21f));teeth.SetFloat("_Glossiness",.25f);
        standing=Human(false);
        crawler=Human(true);
        host.watcher.SetActive(false);host.watcher=standing;
        BuildBlood();BuildDoor(skin,dark,teeth);
        standingLight=LightAnchor(standing);crawlerLight=LightAnchor(crawler);doorLight=LightAnchor(doorRoot);
        voice=new GameObject("Threat voice").AddComponent<AudioSource>();voice.transform.SetParent(transform);voice.playOnAwake=false;voice.spatialBlend=1;voice.minDistance=1;voice.maxDistance=22;voice.dopplerLevel=0;voice.volume=.45f;
        ambience=new GameObject("Threat heartbeat").AddComponent<AudioSource>();ambience.transform.SetParent(transform);ambience.playOnAwake=false;ambience.spatialBlend=0;ambience.loop=true;
        scrape=MakeSound("Duct metal strain",1.7f,0);
        impact=MakeSound("Encounter impact",.7f,1);
        drip=MakeSound("Wet vent drip",.22f,2);
        heartbeat=MakeSound("Low heartbeat",1.1f,3);
        ambience.clip=heartbeat;ambience.volume=0;ambience.Play();
    }
    GameObject Human(bool crawl){
        var wrapper=new GameObject(crawl?"Ceiling hollow":"The hollow commuter");wrapper.transform.SetParent(transform);
        var go=Instantiate(Resources.Load<GameObject>("Horror/HollowCommuter"),wrapper.transform);
        go.transform.localPosition=crawl?new Vector3(0,.22f,-1.15f):Vector3.zero;
        go.transform.localRotation=crawl?Quaternion.Euler(90,0,0):Quaternion.identity;
        go.transform.localScale=crawl?new Vector3(.9f,1.15f,.9f):new Vector3(.80f,1.45f,.82f);
        var originals=Loop.commuter.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach(var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>()){
            foreach(var original in originals)if(original.name==renderer.name){
                var copies=new Material[original.sharedMaterials.Length];
                for(int i=0;i<copies.Length;i++){
                    copies[i]=new Material(original.sharedMaterials[i]);materials.Add(copies[i]);
                    bool face=copies[i].mainTexture&&copies[i].mainTexture.name.Contains("head");
                    if(face){copies[i].mainTexture=Resources.Load<Texture2D>("Horror/HollowHead");copies[i].color=new Color(.92f,.92f,.92f);}
                    else copies[i].color=new Color(.38f,.42f,.40f);
                    copies[i].SetFloat("_Glossiness",.22f);
                }
                renderer.sharedMaterials=copies;break;
            }
            renderer.updateWhenOffscreen=true;
        }
        var actor=go.AddComponent<HollowActor>();actor.Crawl=crawl;actor.walk=Loop.commuter.walking.GetClip("Walk");
        wrapper.SetActive(false);return wrapper;
    }
    GameObject Model(string name,Material skin,Material dark,Material teeth){
        var prefab=Resources.Load<GameObject>("Horror/"+name);
        var go=new GameObject(name);go.transform.SetParent(transform);Instantiate(prefab,go.transform);
        foreach(var renderer in go.GetComponentsInChildren<Renderer>()){
            renderer.sharedMaterial=renderer.name.Contains("darkness")||renderer.name.Contains("socket")?dark:renderer.name.Contains("tooth")?teeth:skin;
            renderer.shadowCastingMode=ShadowCastingMode.On;
        }
        go.SetActive(false);return go;
    }
    static Bounds BoundsOf(GameObject go){
        var renderers=go.GetComponentsInChildren<Renderer>();
        var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);return bounds;
    }
    GameObject Quad(string name,Transform parent,Vector3 position,Quaternion rotation,Vector3 size,Material material){
        var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name=name;Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(parent);go.transform.SetPositionAndRotation(position,rotation);go.transform.localScale=size;
        go.GetComponent<Renderer>().sharedMaterial=material;go.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;return go;
    }
    void BuildBlood(){
        bloodRoot=new GameObject("Blood / vent runoff");bloodRoot.transform.SetParent(transform);
        var vent=BoundsOf(Loop.vent);Vector3 inward=new Vector3(-Mathf.Sign(vent.center.x),0,0);
        float wallX=vent.center.x+inward.x*(vent.extents.x+.015f);
        float height=Mathf.Max(.5f,vent.min.y-.02f);
        blood=Material("Station/VentBlood",new Color(.40f,.018f,.025f));
        puddle=Material("Station/VentBlood",new Color(.30f,.012f,.018f));puddle.SetFloat("_Puddle",1);
        Quad("Uneven blood trails",bloodRoot.transform,new Vector3(wallX,height*.5f+.02f,vent.center.z),Quaternion.LookRotation(inward),new Vector3(Mathf.Max(.65f,vent.size.z),height,1),blood);
        Quad("Blood pooling on tiles",bloodRoot.transform,new Vector3(wallX+inward.x*.32f,.024f,vent.center.z),Quaternion.Euler(90,0,0),new Vector3(.85f,1.1f,1),puddle);
        var drops=new GameObject("Blood drops");drops.transform.SetParent(bloodRoot.transform);
        drops.transform.position=new Vector3(wallX+inward.x*.025f,vent.min.y,vent.center.z);
        drips=drops.AddComponent<ParticleSystem>();var main=drips.main;main.playOnAwake=false;main.startLifetime=1.5f;main.startSpeed=0;main.startSize=new ParticleSystem.MinMaxCurve(.012f,.027f);main.gravityModifier=.8f;main.maxParticles=80;
        var shape=drips.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(.025f,.01f,Mathf.Max(.5f,vent.size.z*.8f));
        var emission=drips.emission;emission.rateOverTime=18;
        var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var renderer=drops.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Mesh;renderer.mesh=sphere.GetComponent<MeshFilter>().sharedMesh;
        var wet=Material("Standard",new Color(.32f,.012f,.018f));wet.SetFloat("_Glossiness",.9f);renderer.sharedMaterial=wet;Destroy(sphere);
        drips.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);bloodRoot.SetActive(false);
    }
    void BuildDoor(Material skin,Material dark,Material teeth){
        doorRoot=new GameObject("Door / grasping hands");doorRoot.transform.SetParent(transform);
        var bounds=BoundsOf(Loop.door);doorInward=new Vector3(-Mathf.Sign(bounds.center.x),0,0);
        for(int i=0;i<2;i++){
            var sourceSkin=Loop.commuter.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
            foreach(var r in Loop.commuter.GetComponentsInChildren<SkinnedMeshRenderer>())foreach(var m in r.sharedMaterials)if(m.mainTexture&&m.mainTexture.name.Contains("body"))sourceSkin=m;
            var handSkin=new Material(sourceSkin);handSkin.color=new Color(.65f,.64f,.61f);materials.Add(handSkin);
            var hand=Model("DetailedDoorHand",handSkin,dark,teeth);hand.transform.SetParent(doorRoot.transform);
            hand.transform.SetPositionAndRotation(new Vector3(bounds.center.x+doorInward.x*(bounds.extents.x+.03f),1.25f+i*.53f,bounds.min.z+.10f+i*.04f),Quaternion.LookRotation(Vector3.forward,-doorInward)*Quaternion.Euler(0,i==0?-8:12,0));
            hand.SetActive(true);
        }
        var gap=Material("Standard",new Color(.003f,.003f,.003f));
        openingPosition=new Vector3(bounds.center.x+doorInward.x*(bounds.extents.x+.115f),1.05f,bounds.min.z+.075f);
        doorOpening=Quad("Dark opening at door edge",doorRoot.transform,openingPosition,Quaternion.LookRotation(-doorInward),new Vector3(.17f,1.95f,1),gap).transform;
        doorRoot.SetActive(false);
    }
    Transform LightAnchor(GameObject root){
        var anchor=new GameObject("Horror light sample").transform;anchor.SetParent(root.transform);
        foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))renderer.probeAnchor=anchor;
        return anchor;
    }
    void UpdateLight(Transform anchor,Vector3 point){
        if(anchor)anchor.position=new Vector3(Mathf.Clamp(point.x,-1.4f,1.4f),1.6f,Mathf.Clamp(point.z,.4f,25.4f));
    }
    public void Begin(int index){
        Clear();mode=index;age=0;nextCue=.6f;
        savedDoor=Loop.door.transform.position;savedDoorRotation=Loop.door.transform.rotation;
        if(index==8||index==9||index==13){
            var volume=FindAnyObjectByType<PostProcessVolume>();encounterGrade=volume.profile.GetSetting<ColorGrading>();
            savedExposure=encounterGrade.postExposure.value;encounterGrade.postExposure.value=savedExposure-(index==13?.3f:.65f);
        }
        if(index==8||index==11){
            host.watcher=standing;standing.SetActive(true);standing.transform.SetPositionAndRotation(new Vector3(.55f,0,22),Quaternion.Euler(0,180,0));
        }
        if(index==9){
            host.watcher=crawler;crawler.SetActive(true);crawler.transform.SetPositionAndRotation(new Vector3(.35f,3.36f,19),Quaternion.Euler(0,180,180));
        }
        if(index==12){bloodRoot.SetActive(true);blood.SetFloat("_Progress",.22f);puddle.SetFloat("_Progress",.07f);drips.Play();}
        if(index==13){doorRoot.SetActive(true);Loop.door.transform.position=savedDoor+doorInward*.10f;}
    }
    bool PlayerVisible(Vector3 target){
        var camera=Loop.player.view;Vector3 eye=camera.transform.position;
        var point=camera.WorldToViewportPoint(target);if(point.z<=0||point.x<.04f||point.x>.96f||point.y<.03f||point.y>.97f)return false;
        foreach(var hit in Physics.RaycastAll(eye,(target-eye).normalized,Vector3.Distance(eye,target),Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
            if(!hit.transform.IsChildOf(Loop.player.transform)&&!hit.transform.IsChildOf(transform))return false;
        return true;
    }
    void Cue(AudioClip clip,Vector3 position,float volume){
        voice.transform.position=position;voice.pitch=1;voice.PlayOneShot(clip,volume);
    }
    public void Tick(float dt){
        if(mode<0)return;
        age+=dt;
        if(Catching){
            catchAge+=dt;
            if(catchAge>.95f){Loop.player.HorrorLocked=false;Loop.ResetRun();}
            return;
        }
        var player=Loop.player;Vector3 p=player.transform.position;
        bool inside=Mathf.Abs(p.x)<2.3f&&p.z>2&&p.z<25.5f;
        float targetPressure=0;
        UpdateLight(standingLight,standing.transform.position);
        UpdateLight(crawlerLight,crawler.transform.position);
        UpdateLight(doorLight,savedDoor);
        if(mode==8||mode==11||mode==9){
            var monster=host.watcher.transform;
            float distance=Vector2.Distance(new Vector2(p.x,p.z),new Vector2(monster.position.x,monster.position.z));
            bool watched=PlayerVisible(monster.position+(mode==9?Vector3.down*.65f:Vector3.up*2.25f));
            if(inside&&p.z>5&&!armed){armed=true;alert=age;Cue(scrape,monster.position,.65f);}
            if(armed&&inside){
                targetPressure=Mathf.InverseLerp(17,1,distance);
                unseen=watched?0:unseen+dt;
                bool warned=age-alert>1.25f;
                float speed=0;
                if(warned){
                    if(mode==9)speed=player.IsSprinting?5.7f:1.8f;
                    else if(mode==11)speed=player.IsSprinting?5.8f:2.25f;
                    else if(!watched)speed=player.IsSprinting?5.9f:3.1f;
                    // Sprint noise provokes a short lunge even while staring.
                    else if(player.IsSprinting&&age-alert>2.1f)speed=4.9f;
                }
                Vector3 target=new Vector3(p.x,mode==9?3.36f:0,p.z);
                monster.position=Vector3.MoveTowards(monster.position,target,speed*dt);
                var actor=monster.GetComponentInChildren<HollowActor>();if(actor)actor.Movement=speed*.42f;
                var direction=p-monster.position;direction.y=0;
                if(direction.sqrMagnitude>.001f){
                    var face=Quaternion.LookRotation(direction);
                    monster.rotation=mode==9?face*Quaternion.Euler(0,0,180):face*Quaternion.Euler(0,0,Mathf.Sin(age*.7f)*4);
                }
                if(warned&&distance<1.15f)Catch();
                if(age>nextCue&&distance<14){Cue(scrape,monster.position,.22f+targetPressure*.3f);nextCue=age+3.2f;}
            }
        }
        if(mode==12){
            float flow=Mathf.Clamp01(.22f+age*.075f);
            blood.SetFloat("_Progress",flow);puddle.SetFloat("_Progress",Mathf.Clamp01(age*.045f));
            targetPressure=inside&&Vector3.Distance(p,bloodRoot.GetComponentInChildren<Renderer>().bounds.center)<7?.22f:0;
            if(age>nextCue){Cue(drip,drips.transform.position,.55f);nextCue=age+Random.Range(.5f,1.1f);}
        }
        if(mode==13){
            float distance=Vector3.Distance(p,new Vector3(savedDoor.x,p.y,savedDoor.z));
            if(inside&&distance<7&&!armed){armed=true;alert=age;Cue(scrape,savedDoor,.6f);}
            float reach=armed?Mathf.SmoothStep(0,.42f,(age-alert)/2):0;
            doorRoot.transform.position=doorInward*reach;
            doorOpening.position=openingPosition+doorInward*(reach*.35f);
            Loop.door.transform.position=savedDoor+doorInward*(.1f+reach*.35f);
            Loop.door.transform.rotation=savedDoorRotation*Quaternion.Euler(0,Mathf.Sin(age*2)*2,0);
            targetPressure=Mathf.InverseLerp(7,1,distance)*.7f;
            if(armed&&age-alert>1.3f&&distance<1.5f&&player.IsSprinting)Catch();
        }
        pressure=Mathf.Lerp(pressure,targetPressure,1-Mathf.Exp(-3*dt));
        ambience.volume=Mathf.Min(.2f,pressure*.20f+player.Exhaustion*.07f);
    }
    void Catch(){
        if(Catching)return;
        catchAge=0;Loop.player.HorrorLocked=true;
        Cue(impact,Loop.player.view.transform.position+Loop.player.view.transform.forward*.5f,.75f);
        if(mode==8||mode==11){
            standing.transform.position=Loop.player.view.transform.position+Loop.player.view.transform.forward*.68f-Vector3.up*2.3f;
            standing.transform.LookAt(new Vector3(Loop.player.transform.position.x,standing.transform.position.y,Loop.player.transform.position.z));
        }
    }
    public void Clear(){
        if(encounterGrade){encounterGrade.postExposure.value=savedExposure;encounterGrade=null;}
        if(mode==13&&Loop&&Loop.door){Loop.door.transform.position=savedDoor;Loop.door.transform.rotation=savedDoorRotation;}
        if(standing)standing.SetActive(false);if(crawler)crawler.SetActive(false);if(bloodRoot)bloodRoot.SetActive(false);
        if(doorRoot){doorRoot.SetActive(false);doorRoot.transform.position=Vector3.zero;}
        if(drips)drips.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        if(voice)voice.Stop();if(ambience)ambience.volume=0;
        if(host&&Loop&&Loop.player)Loop.player.HorrorLocked=false;
        mode=-1;armed=false;catchAge=-1;pressure=0;unseen=0;
    }
    void OnGUI(){
        if(!Catching)return;
        GUI.color=new Color(0,0,0,Mathf.Clamp01((catchAge-.15f)/.55f));
        GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);
        if(catchAge>.55f){GUI.color=new Color(.75f,.72f,.69f,1);var style=new GUIStyle(GUI.skin.label){fontSize=25,alignment=TextAnchor.MiddleCenter};GUI.Label(new Rect(0,Screen.height*.46f,Screen.width,70),"YAKALANDIN",style);}
        GUI.color=Color.white;
    }
    AudioClip MakeSound(string name,float seconds,int kind){
        int rate=24000;var samples=new float[(int)(rate*seconds)];var random=new System.Random(kind+410);float low=0;
        for(int i=0;i<samples.Length;i++){
            float t=i/(float)rate;low=Mathf.Lerp(low,(float)random.NextDouble()*2-1,.12f);
            float fade=Mathf.Min(1,t/.02f)*Mathf.Min(1,(seconds-t)/.06f),value=0;
            if(kind==0)value=(low*.4f+Mathf.Sin(t*142+t*t*13)*.12f+Mathf.Sin(t*713+Mathf.Sin(t*9)*4)*.055f)*Mathf.Pow(Mathf.Sin(Mathf.PI*t/seconds),2);
            if(kind==1)value=(Mathf.Sin(t*2*Mathf.PI*48)*.5f+low*.38f)*Mathf.Exp(-t*7);
            if(kind==2)value=Mathf.Sin(t*2*Mathf.PI*(530-1100*t))*Mathf.Exp(-t*30)*.35f;
            if(kind==3){float pulse=Mathf.Exp(-Mathf.Pow((t-.10f)*28,2))+.65f*Mathf.Exp(-Mathf.Pow((t-.29f)*25,2));value=Mathf.Sin(t*2*Mathf.PI*52)*pulse*.65f;}
            samples[i]=value*fade;
        }
        var clip=AudioClip.Create(name,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
    }
    void OnDestroy(){
        Clear();foreach(var m in materials)if(m)Destroy(m);
        foreach(var clip in new[]{scrape,impact,drip,heartbeat})if(clip)Destroy(clip);
    }
}
