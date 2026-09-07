using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using Object=UnityEngine.Object;

// Dimensioned environment authoring. All assets and lighting remain editable in Unity.
public static class StationV2 {
    public static void Export(){Capture();BuildStation.BuildPlayer();}
    const string Art="Assets/ArtV2/";
    static Transform root; static Material ceramic,stone,metal,paint,black,ochre,tube,sign,poster;
    static Font font,japanese; static int meshId;
    static readonly Color Ink=new Color(.025f,.027f,.023f);
    static Material Save(Material m,string name){m.name=name;AssetDatabase.CreateAsset(m,Art+name+".mat");return m;}
    static Material Solid(string name,Color c,float metallic=0,float smooth=.35f){var m=new Material(Shader.Find("Standard"));m.color=c;m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",smooth);return Save(m,name);}
    static Texture2D Tex(string path,bool normal=false,bool linear=false){
        var i=(TextureImporter)AssetImporter.GetAtPath(path);if(i==null)throw new Exception("Texture missing: "+path);
        i.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;i.sRGBTexture=!linear&&!normal;i.maxTextureSize=2048;i.anisoLevel=16;i.mipmapEnabled=true;i.textureCompression=TextureImporterCompression.Uncompressed;i.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static Material Scan(string name,string id,Color tint,float gloss,float flat,float grout){
        string p="Assets/Textures/"+id+"/"+id+"_2K-JPG_";
        var m=new Material(Shader.Find("Exit7/Scanned surface"));m.SetTexture("_MainTex",Tex(p+"Color.jpg"));m.SetTexture("_BumpMap",Tex(p+"NormalGL.jpg",true));m.SetTexture("_Roughness",Tex(p+"Roughness.jpg",false,true));m.SetTexture("_Height",Tex(p+"Displacement.jpg",false,true));m.SetColor("_Color",tint);m.SetFloat("_Gloss",gloss);m.SetFloat("_Flatness",flat);m.SetFloat("_Grout",grout);m.SetFloat("_NormalStrength",.8f);return Save(m,name);
    }
    static void Static(GameObject g,float scale=1){GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.ContributeGI|StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);var r=g.GetComponent<MeshRenderer>();if(r){r.scaleInLightmap=scale;r.receiveGI=ReceiveGI.Lightmaps;}}
    static GameObject Box(string n,Vector3 p,Vector3 size,Material mat,Transform parent=null,bool collision=true,float lightmap=1){
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(parent?parent:root,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;if(!collision)Object.DestroyImmediate(g.GetComponent<Collider>());Static(g,lightmap);return g;
    }
    // Quads face local -Z, so texture orientation is consistent with printed signage.
    static GameObject Surface(string n,Vector3 pos,Vector2 size,Quaternion rotation,Material mat,Vector2 uvScale,Transform parent=null,Vector2 uvOffset=default){
        var g=new GameObject(n);g.transform.SetParent(parent?parent:root,false);g.transform.localPosition=pos;g.transform.localRotation=rotation;
        var mesh=new Mesh{name=n};float w=size.x/2,h=size.y/2;mesh.vertices=new[]{new Vector3(-w,-h,0),new Vector3(-w,h,0),new Vector3(w,h,0),new Vector3(w,-h,0)};
        mesh.uv=new[]{uvOffset,uvOffset+new Vector2(0,uvScale.y),uvOffset+uvScale,uvOffset+new Vector2(uvScale.x,0)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateNormals();mesh.RecalculateTangents();Unwrapping.GenerateSecondaryUVSet(mesh);AssetDatabase.CreateAsset(mesh,Art+"Surface"+(meshId++)+".asset");g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;Static(g);return g;
    }
    static void Wall(string name,Vector3 center,Vector2 size,float yaw){Surface(name,center,size,Quaternion.Euler(0,yaw,0),ceramic,size/1.2f);}
    static void Floor(string name,Vector3 center,Vector2 size){Surface(name,center,size,Quaternion.Euler(90,0,0),stone,size/3.6f);}
    static GameObject Cylinder(string name,Vector3 a,Vector3 b,float radius,Material mat,Transform parent=null){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=name;g.transform.SetParent(parent?parent:root,false);g.transform.localPosition=(a+b)*.5f;g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);g.transform.localScale=new Vector3(radius*2,(b-a).magnitude*.5f,radius*2);g.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(g.GetComponent<Collider>());Static(g,.1f);return g;}
    static Transform Group(string name,Vector3 pos,float yaw=0){var g=new GameObject(name);g.transform.SetParent(root,false);g.transform.localPosition=pos;g.transform.localRotation=Quaternion.Euler(0,yaw,0);return g.transform;}
    static void Type(Transform p,string words,Vector3 pos,float height,Color color,bool jp=false){
        var g=new GameObject(words.Replace('\n',' '));g.transform.SetParent(p,false);g.transform.localPosition=pos;var t=g.AddComponent<TextMesh>();t.font=jp?japanese:font;t.fontSize=80;t.characterSize=height/8;t.text=words;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color;
        // Font material owns a dynamic atlas; a depth-tested lit shader prevents text drawing through objects.
        t.GetComponent<Renderer>().sharedMaterial=t.font.material;
    }
    static void Frame(Transform p,float w,float h){
        Box("Backing",new Vector3(0,0,.006f),new Vector3(w+.024f,h+.024f,.035f),black,p,false,.3f);
        for(int s=-1;s<=1;s+=2){Box("Aluminium vertical extrusion",new Vector3(s*(w*.5f+.007f),0,-.022f),new Vector3(.018f,h+.052f,.026f),metal,p,false,.2f);Box("Aluminium horizontal extrusion",new Vector3(0,s*(h*.5f+.017f),-.022f),new Vector3(w+.052f,.018f,.026f),metal,p,false,.2f);}
    }
    static void Fixture(float z){
        var p=Group("Twin fluorescent / 4000 K",new Vector3(0,2.92f,z));
        Box("Pressed steel chassis",Vector3.zero,new Vector3(1.37f,.095f,.24f),paint,p,false,.3f);
        Box("Reflector",new Vector3(0,-.052f,0),new Vector3(1.30f,.014f,.218f),metal,p,false,.1f);
        for(int s=-1;s<=1;s+=2){Cylinder("T8 phosphor glass",new Vector3(-.60f,-.084f,s*.07f),new Vector3(.60f,-.084f,s*.07f),.016f,tube,p);for(int side=-1;side<=1;side+=2)Box("Ceramic lamp socket",new Vector3(side*.618f,-.079f,s*.07f),new Vector3(.04f,.052f,.047f),paint,p,false,.1f);}
        var a=new GameObject("Baked rectangular light");a.transform.SetParent(p,false);a.transform.localPosition=new Vector3(0,-.13f,0);a.transform.localRotation=Quaternion.Euler(90,0,0);var l=a.AddComponent<Light>();l.type=LightType.Rectangle;l.areaSize=new Vector2(1.28f,.24f);l.color=new Color(1,.97f,.90f);l.intensity=7;l.range=8;l.lightmapBakeType=LightmapBakeType.Baked;l.shadows=LightShadows.Soft;
        var b=new GameObject("Realtime specular support");b.transform.SetParent(p,false);b.transform.localPosition=new Vector3(0,-.20f,0);var pl=b.AddComponent<Light>();pl.type=LightType.Point;pl.range=4.3f;pl.intensity=.30f;pl.color=new Color(.91f,.95f,1);pl.shadows=LightShadows.None;
    }
    static void Tactile(Vector3 center,bool dots=false,bool across=false){
        var p=Group("300 mm tactile paving",center,across?90:0);Box("Moulded yellow tile",Vector3.zero,new Vector3(.298f,.016f,.298f),ochre,p,false,.15f);
        if(dots){for(int x=0;x<5;x++)for(int z=0;z<5;z++)Cylinder("Warning stud",new Vector3(-.12f+x*.06f,.008f,-.12f+z*.06f),new Vector3(-.12f+x*.06f,.014f,-.12f+z*.06f),.012f,ochre,p);}
        else for(int i=0;i<4;i++){var r=Box("Rounded directional rib",new Vector3(-.108f+i*.072f,.011f,0),new Vector3(.025f,.012f,.245f),ochre,p,false,0);Cylinder("Rib rounded end",new Vector3(-.108f+i*.072f,.009f,-.1225f),new Vector3(-.108f+i*.072f,.016f,-.1225f),.0125f,ochre,p);Cylinder("Rib rounded end",new Vector3(-.108f+i*.072f,.009f,.1225f),new Vector3(-.108f+i*.072f,.016f,.1225f),.0125f,ochre,p);}
    }
    static void Ad(int index,float z){
        var p=Group("Printed advertisement "+index,new Vector3(-2.085f,1.67f,z),-90);Frame(p,.86f,1.29f);
        Surface("Photo print behind acrylic",new Vector3(0,0,-.019f),new Vector2(.86f,1.29f),Quaternion.identity,poster,new Vector2(1f/3,.5f),p,new Vector2(index%3/3f,index<3?.5f:0));
        for(int s=-1;s<=1;s+=2)Cylinder("Frame screw",new Vector3(s*.423f,-.64f,-.035f),new Vector3(s*.423f,-.64f,-.038f),.004f,metal,p);
    }
    static void Door(float z,string code){
        var p=Group("Recessed utility door "+code,new Vector3(2.083f,1.07f,z),90);
        Box("Dark reveal",new Vector3(0,0,.005f),new Vector3(.98f,2.14f,.06f),black,p);
        Box("Steel leaf",new Vector3(0,-.012f,-.027f),new Vector3(.90f,2.07f,.025f),paint,p);
        for(int s=-1;s<=1;s+=2)Box("Jamb",new Vector3(s*.478f,0,-.035f),new Vector3(.04f,2.18f,.063f),paint,p);
        Box("Lintel",new Vector3(0,1.08f,-.035f),new Vector3(.995f,.04f,.063f),paint,p);
        Cylinder("Lock cylinder",new Vector3(.32f,-.12f,-.05f),new Vector3(.32f,-.12f,-.078f),.025f,metal,p);
        Cylinder("Lever handle",new Vector3(.32f,-.10f,-.09f),new Vector3(.21f,-.10f,-.09f),.012f,metal,p);
        for(int s=-1;s<=1;s+=2)Box("Hinge",new Vector3(-.45f,s*.68f,-.05f),new Vector3(.016f,.09f,.026f),metal,p,false,.1f);
        Type(p,code,new Vector3(0,.75f,-.05f),.055f,Ink);Type(p,"関係者以外立入禁止",new Vector3(0,.57f,-.052f),.034f,Ink,true);
    }
    static void Vent(float z){var p=Group("Wall ventilation grille",new Vector3(2.075f,2.51f,z),90);Frame(p,.57f,.36f);Box("Duct interior",new Vector3(0,0,-.02f),new Vector3(.56f,.35f,.025f),black,p,false,.2f);for(int i=0;i<9;i++){var g=Box("Angled steel louvre",new Vector3(0,-.154f+i*.038f,-.043f),new Vector3(.55f,.014f,.03f),metal,p,false,.1f);g.transform.localRotation=Quaternion.Euler(-25,0,0);}}
    static void ExitSign(){
        var p=Group("Illuminated exit wayfinding",new Vector3(0,2.56f,3.8f));Frame(p,3.04f,.35f);Box("Opal illuminated face",new Vector3(0,0,-.02f),new Vector3(3.04f,.35f,.015f),sign,p,false,.3f);
        Type(p,"↑",new Vector3(-.34f,0,-.036f),.31f,Ink,true);Type(p,"出口",new Vector3(.01f,.047f,-.036f),.105f,Ink,true);Type(p,"Exit",new Vector3(.01f,-.076f,-.036f),.072f,Ink);Type(p,"7",new Vector3(.38f,0,-.036f),.27f,Ink);
        for(int s=-1;s<=1;s+=2)Cylinder("Suspension rod",new Vector3(s*1.2f,2.77f,3.8f),new Vector3(s*1.2f,2.98f,3.8f),.008f,metal);
    }
    static void Stairs(){
        // Right turn at the end of the passage leads to the daylight stairwell.
        Box("Stairwell north backing",new Vector3(6,3.5f,28.1f),new Vector3(8.2f,7,.2f),paint);
        Box("Stairwell south backing",new Vector3(6,3.5f,24.5f),new Vector3(8.2f,7,.2f),paint);
        Wall("Stairwell north ceramic",new Vector3(6,3.5f,27.99f),new Vector2(8.2f,7),0);Wall("Stairwell south ceramic",new Vector3(6,3.5f,24.61f),new Vector2(8.2f,7),180);
        for(int i=0;i<22;i++){float x=2.15f+i*.31f,y=(i+1)*.16f;Box("Stair riser backing",new Vector3(x,y-.08f,26.3f),new Vector3(.31f,.16f,3.4f),paint,null,false,.5f);Floor("Porcelain tread",new Vector3(x,y+.002f,26.3f),new Vector2(.31f,3.4f));Surface("Porcelain riser",new Vector3(x-.155f,y-.08f,26.3f),new Vector2(3.4f,.16f),Quaternion.Euler(0,90,0),ceramic,new Vector2(3.4f/1.2f,.16f/1.2f));Box("Anti-slip stair nosing",new Vector3(x-.136f,y+.007f,26.3f),new Vector3(.032f,.015f,3.36f),metal,null,false,.1f);}
        var ramp=new GameObject("Invisible stair collision");ramp.transform.SetParent(root);var m=new Mesh();m.vertices=new[]{new Vector3(1.98f,0,24.61f),new Vector3(1.98f,0,27.99f),new Vector3(8.82f,3.52f,24.61f),new Vector3(8.82f,3.52f,27.99f)};m.triangles=new[]{0,1,2,1,3,2};m.RecalculateNormals();AssetDatabase.CreateAsset(m,Art+"StairRamp.asset");ramp.AddComponent<MeshCollider>().sharedMesh=m;
        Box("Exit landing",new Vector3(9.75f,3.41f,26.3f),new Vector3(1.92f,.22f,3.4f),paint);Floor("Landing porcelain",new Vector3(9.75f,3.522f,26.3f),new Vector2(1.92f,3.4f));Box("End barrier",new Vector3(10.7f,5.0f,26.3f),new Vector3(.15f,3.5f,3.5f),tube);
        for(int s=-1;s<=1;s+=2){float z=26.3f+s*1.49f;Cylinder("Continuous stainless handrail",new Vector3(1.85f,.92f,z),new Vector3(8.9f,4.49f,z),.029f,metal);Cylinder("Lower handrail",new Vector3(1.85f,.67f,z),new Vector3(8.9f,4.24f,z),.023f,metal);for(int i=0;i<6;i++)Cylinder("Rail bracket",new Vector3(2.2f+i*1.2f,1.0f+i*.60f,z),new Vector3(2.2f+i*1.2f,1.0f+i*.60f,26.3f+s*1.68f),.012f,metal);}
        for(int i=0;i<10;i++)Tactile(new Vector3(1.67f,.02f,24.95f+i*.3f),true);
        var d=new GameObject("Daylight entering stairwell");d.transform.SetParent(root);d.transform.position=new Vector3(8.3f,5.5f,26.3f);d.transform.rotation=Quaternion.Euler(35,-90,0);var l=d.AddComponent<Light>();l.type=LightType.Rectangle;l.areaSize=new Vector2(3,3);l.intensity=13;l.color=new Color(.83f,.90f,1);l.lightmapBakeType=LightmapBakeType.Baked;
    }
    static void Post(Camera cam){
        var layer=cam.gameObject.AddComponent<PostProcessLayer>();layer.volumeLayer=1;layer.volumeTrigger=cam.transform;layer.Init(AssetDatabase.LoadAssetAtPath<PostProcessResources>("Packages/com.unity.postprocessing/PostProcessing/PostProcessResources.asset"));layer.antialiasingMode=PostProcessLayer.Antialiasing.TemporalAntialiasing;layer.temporalAntialiasing.jitterSpread=.75f;layer.temporalAntialiasing.sharpness=.22f;
        var v=new GameObject("Photographic exposure and lens").AddComponent<PostProcessVolume>();v.isGlobal=true;v.priority=1;var profile=ScriptableObject.CreateInstance<PostProcessProfile>();AssetDatabase.CreateAsset(profile,Art+"StationPhotography.asset");v.sharedProfile=profile;
        var ao=profile.AddSettings<AmbientOcclusion>();ao.enabled.Override(true);ao.mode.Override(AmbientOcclusionMode.MultiScaleVolumetricObscurance);ao.intensity.Override(.75f);ao.thicknessModifier.Override(1.0f);ao.directLightingStrength.Override(.25f);
        var bloom=profile.AddSettings<Bloom>();bloom.enabled.Override(true);bloom.intensity.Override(.55f);bloom.threshold.Override(1.1f);bloom.softKnee.Override(.65f);bloom.diffusion.Override(5);
        var grade=profile.AddSettings<ColorGrading>();grade.enabled.Override(true);grade.gradingMode.Override(GradingMode.HighDefinitionRange);grade.tonemapper.Override(Tonemapper.ACES);grade.postExposure.Override(.65f);grade.temperature.Override(-3);grade.saturation.Override(-9);grade.contrast.Override(7);
        var vig=profile.AddSettings<Vignette>();vig.enabled.Override(true);vig.intensity.Override(.17f);vig.smoothness.Override(.65f);
        var grain=profile.AddSettings<Grain>();grain.enabled.Override(true);grain.intensity.Override(.07f);grain.size.Override(.65f);grain.colored.Override(false);
        var ssr=profile.AddSettings<ScreenSpaceReflections>();ssr.enabled.Override(true);ssr.preset.Override(ScreenSpaceReflectionPreset.High);ssr.maximumMarchDistance.Override(24);ssr.vignette.Override(.4f);
        foreach(var setting in profile.settings)AssetDatabase.AddObjectToAsset(setting,profile);
    }
    public static void Build(){
        Directory.CreateDirectory(Art);Directory.CreateDirectory("Screenshots");meshId=0;EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);root=new GameObject("EXIT 7 / architectural environment").transform;
        font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Arial.ttf");japanese=AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Japanese.ttc");if(!japanese)japanese=font;
        ceramic=Scan("Glazed ceramic 300mm","Tiles002",new Color(.96f,.98f,.96f),.82f,0,.56f);
        stone=Scan("Fine aggregate porcelain 450mm","Tiles040",new Color(.82f,.85f,.84f),.52f,.84f,.45f);
        metal=Solid("Brushed stainless",new Color(.63f,.65f,.64f),.92f,.70f);paint=Solid("Powder coated warm grey",new Color(.58f,.60f,.57f),.18f,.42f);black=Solid("Shadow and gaskets",new Color(.033f,.035f,.031f),.1f,.26f);ochre=Solid("Worn tactile rubber",new Color(.71f,.54f,.20f),0,.30f);
        tube=Solid("Fluorescent glass",Color.white,0,.55f);tube.EnableKeyword("_EMISSION");tube.SetColor("_EmissionColor",new Color(.88f,.94f,1)*7);tube.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;
        sign=Solid("Backlit yellow acrylic",new Color(1,.85f,.25f),0,.5f);sign.EnableKeyword("_EMISSION");sign.SetColor("_EmissionColor",new Color(1,.82f,.18f)*1.4f);sign.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;
        poster=Solid("Photo print satin laminate",Color.white,0,.38f);poster.mainTexture=Tex("Assets/Textures/StationPosters.png");
        // Solid shell plus independently UV-mapped, lightmapped inside faces.
        Box("Floor slab",new Vector3(0,-.12f,13),new Vector3(4.4f,.22f,30),paint);Floor("Porcelain floor",new Vector3(0,0,13),new Vector2(4.2f,30));
        Box("West structural wall",new Vector3(-2.20f,1.5f,13),new Vector3(.2f,3,30),paint);Wall("West glazed ceramic",new Vector3(-2.098f,1.5f,13),new Vector2(30,3),-90);
        Box("East structural wall",new Vector3(2.20f,1.5f,11.25f),new Vector3(.2f,3,26.5f),paint);Wall("East glazed ceramic",new Vector3(2.098f,1.5f,11.25f),new Vector2(26.5f,3),90);
        Box("End structural wall",new Vector3(0,1.5f,28.1f),new Vector3(4.4f,3,.2f),paint);Wall("End ceramic",new Vector3(0,1.5f,27.998f),new Vector2(4.2f,3),0);
        Box("Entrance wall",new Vector3(0,1.5f,-2.1f),new Vector3(4.4f,3,.2f),paint);Wall("Entrance ceramic",new Vector3(0,1.5f,-1.998f),new Vector2(4.2f,3),180);
        Box("Ceiling structure",new Vector3(0,3.10f,13),new Vector3(4.4f,.2f,30),black);
        // Thin enamel strips with narrow recessed joints; no exaggerated black bars.
        for(int i=0;i<150;i++)Box("Enamel ceiling strip",new Vector3(0,2.996f,-1.9f+i*.2f),new Vector3(4.19f,.025f,.198f),paint,null,false,.3f);
        for(int s=-1;s<=1;s+=2){float length=s<0?30:26.5f;Box("Recessed skirting",new Vector3(s*2.083f,.045f,s<0?13:11.25f),new Vector3(.025f,.09f,length),paint,null,false,.2f);}
        for(int i=0;i<94;i++)Tactile(new Vector3(-.05f,.015f,-1.65f+i*.3f));for(int i=0;i<6;i++)Tactile(new Vector3(.25f+i*.3f,.015f,26.25f),false,true);
        for(int i=0;i<8;i++)Fixture(.7f+i*3.65f);
        ExitSign();int[] ads={0,3,4,1,2,5};for(int i=0;i<6;i++)Ad(ads[i],2.1f+i*3.10f);
        Door(7.2f,"B-01");Door(12.9f,"B-02");Door(18.8f,"B-03");for(int i=0;i<4;i++)Vent(4.5f+i*5.8f);
        var notice=Group("Security camera notice",new Vector3(2.077f,1.73f,2.8f),90);Box("White printed adhesive",Vector3.zero,new Vector3(.53f,.65f,.002f),Solid("Notice paper",new Color(.9f,.9f,.85f)),notice,false,.1f);Type(notice,"◉   ◉",new Vector3(0,.17f,-.003f),.13f,Ink,true);Type(notice,"防犯カメラ",new Vector3(0,-.015f,-.003f),.081f,Ink,true);Type(notice,"作動中！",new Vector3(0,-.12f,-.003f),.09f,new Color(.55f,.035f,.02f),true);Type(notice,"Security camera in operation",new Vector3(0,-.265f,-.003f),.027f,Ink);
        for(int i=0;i<3;i++){float z=5+i*9;Cylinder("CCTV ceiling base",new Vector3(-1.48f,2.98f,z),new Vector3(-1.48f,2.94f,z),.085f,paint);var d=GameObject.CreatePrimitive(PrimitiveType.Sphere);d.name="Smoked CCTV hemisphere";d.transform.SetParent(root);d.transform.position=new Vector3(-1.48f,2.935f,z);d.transform.localScale=new Vector3(.145f,.12f,.145f);d.GetComponent<Renderer>().sharedMaterial=black;Static(d,.15f);}
        // Flush inspection hatch and recessed drainage channel lend construction detail.
        Box("Ceiling maintenance hatch",new Vector3(.87f,2.974f,10.7f),new Vector3(.54f,.012f,.54f),metal,null,false,.1f);Box("Hatch face",new Vector3(.87f,2.961f,10.7f),new Vector3(.52f,.014f,.52f),paint,null,false,.2f);
        Box("Drainage recess",new Vector3(0,.003f,24.05f),new Vector3(4.18f,.006f,.09f),black,null,false,.1f);for(int i=0;i<139;i++)Box("Drain grate bar",new Vector3(-2.07f+i*.03f,.009f,24.05f),new Vector3(.009f,.009f,.088f),metal,null,false,0);
        Stairs();var way=Group("Stair direction",new Vector3(-2.075f,1.63f,25.8f),-90);Frame(way,.69f,1.04f);Box("Yellow sign face",new Vector3(0,0,-.02f),new Vector3(.69f,1.04f,.012f),sign,way,false,.2f);Type(way,"出口",new Vector3(-.10f,.35f,-.035f),.08f,Ink,true);Type(way,"7 →",new Vector3(0,.15f,-.035f),.25f,Ink,true);Type(way,"北口広場\n市民公園\n中央通り",new Vector3(0,-.24f,-.035f),.05f,Ink,true);
        var player=new GameObject("Player / first person");player.transform.position=new Vector3(.30f,.03f,.20f);var cc=player.AddComponent<CharacterController>();cc.height=1.8f;cc.center=Vector3.up*.9f;cc.radius=.22f;cc.stepOffset=.22f;cc.slopeLimit=50;cc.skinWidth=.02f;
        var cg=new GameObject("First person camera");cg.tag="MainCamera";cg.transform.SetParent(player.transform,false);cg.transform.localPosition=new Vector3(0,1.67f,0);var cam=cg.AddComponent<Camera>();cam.fieldOfView=66;cam.nearClipPlane=.04f;cam.farClipPlane=90;cam.allowHDR=true;cam.renderingPath=RenderingPath.DeferredShading;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.4f,.45f,.50f);cg.AddComponent<AudioListener>();player.AddComponent<FirstPerson>().view=cam;Post(cam);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.06f,.065f,.075f);RenderSettings.ambientIntensity=1;RenderSettings.fog=false;RenderSettings.reflectionIntensity=.8f;RenderSettings.defaultReflectionResolution=256;
        for(int i=0;i<5;i++){var p=new GameObject("Local reflection capture "+i);p.transform.SetParent(root);p.transform.position=new Vector3(0,1.5f,1+i*6);var probe=p.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Baked;probe.resolution=256;probe.size=new Vector3(4.2f,3,9);probe.boxProjection=true;probe.blendDistance=1.3f;probe.clearFlags=ReflectionProbeClearFlags.SolidColor;probe.backgroundColor=Color.gray;probe.intensity=.85f;}
        var probes=new GameObject("Player lighting samples").AddComponent<LightProbeGroup>();var positions=new List<Vector3>();for(int i=0;i<14;i++)foreach(float x in new[]{-1.5f,0,1.5f})foreach(float y in new[]{.4f,1.7f,2.6f})positions.Add(new Vector3(x,y,i*2));probes.probePositions=positions.ToArray();
        QualitySettings.SetQualityLevel(QualitySettings.names.Length-1);QualitySettings.pixelLightCount=8;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.shadowDistance=35;QualitySettings.vSyncCount=1;QualitySettings.anisotropicFiltering=AnisotropicFiltering.ForceEnable;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        var ls=new LightingSettings();ls.name="Station bounced fluorescent lighting";ls.bakedGI=true;ls.realtimeGI=false;ls.lightmapper=LightingSettings.Lightmapper.ProgressiveGPU;ls.lightmapResolution=20;ls.lightmapMaxSize=2048;ls.lightmapPadding=4;ls.directSampleCount=64;ls.indirectSampleCount=256;ls.environmentSampleCount=64;ls.maxBounces=4;ls.ao=true;ls.aoMaxDistance=.32f;ls.aoExponentIndirect=.65f;ls.aoExponentDirect=.30f;AssetDatabase.CreateAsset(ls,Art+"Lighting.asset");Lightmapping.lightingSettings=ls;
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/Underground.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Underground.unity",true)};AssetDatabase.SaveAssets();Debug.Log("STATION_V2_GEOMETRY_READY");
    }
    static void Persist(){
        foreach(string guid in AssetDatabase.FindAssets("",new[]{Art.TrimEnd('/')}))foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)))if(asset)EditorUtility.SetDirty(asset);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/Underground.unity");AssetDatabase.SaveAssets();
    }
    public static void BuildAndBake(){Build();TuneScene();FinishLighting();Bake();}
    public static void FinishPhotography(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");FinishLighting();Bake();}
    static void FinishLighting(){
        root=GameObject.Find("EXIT 7 / architectural environment").transform;
        var face=AssetDatabase.LoadAssetAtPath<Material>(Art+"Printed luminous exit sign.mat");face.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;face.EnableKeyword("_EMISSION");face.SetColor("_EmissionColor",Color.white*1.45f);
        var profile=AssetDatabase.LoadAssetAtPath<PostProcessProfile>(Art+"StationPhotography.asset");profile.GetSetting<ColorGrading>().postExposure.Override(.05f);
        // Broad low energy fill represents diffuse return from the floor and keeps the ceiling readable.
        for(int i=0;i<8;i++){
            string name="Diffuse floor return "+i;if(GameObject.Find(name))continue;
            var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=new Vector3(0,.8f,.7f+i*3.65f);g.transform.rotation=Quaternion.Euler(-90,0,0);var l=g.AddComponent<Light>();l.type=LightType.Rectangle;l.areaSize=new Vector2(3.3f,2.6f);l.intensity=.20f;l.color=new Color(.96f,.97f,1);l.lightmapBakeType=LightmapBakeType.Baked;
        }
        var player=Object.FindFirstObjectByType<FirstPerson>();player.transform.position=new Vector3(.30f,.03f,-1.2f);Persist();
    }
    public static void TuneAndBake(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");TuneScene();Bake();}
    static void CombineTactileMeshes(){
        int index=0;
        foreach(var tr in root.GetComponentsInChildren<Transform>()){
            if(!tr || tr.name!="300 mm tactile paving" || tr.GetComponent<MeshFilter>())continue;
            var source=tr.GetComponentsInChildren<MeshFilter>();var parts=new List<CombineInstance>();
            foreach(var f in source)parts.Add(new CombineInstance{mesh=f.sharedMesh,transform=tr.worldToLocalMatrix*f.transform.localToWorldMatrix});
            var mesh=new Mesh{name="Moulded tactile tile"};mesh.CombineMeshes(parts.ToArray());Unwrapping.GenerateSecondaryUVSet(mesh);AssetDatabase.CreateAsset(mesh,Art+"Tactile"+(index++)+".asset");
            foreach(var f in source)Object.DestroyImmediate(f.gameObject);
            tr.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;tr.gameObject.AddComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"Worn tactile rubber.mat");Static(tr.gameObject,3);
        }
    }
    static void TuneScene(){
        root=GameObject.Find("EXIT 7 / architectural environment").transform;meshId=8000;
        CombineTactileMeshes();
        foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){
            if(l.name=="Baked rectangular light"){l.intensity=10;l.color=new Color(.91f,.95f,1);}
            if(l.name=="Daylight entering stairwell")l.intensity=2.7f;
        }
        var enamel=AssetDatabase.LoadAssetAtPath<Material>(Art+"Powder coated warm grey.mat");enamel.color=new Color(.65f,.67f,.65f);enamel.SetFloat("_Glossiness",.32f);
        AssetDatabase.LoadAssetAtPath<Material>(Art+"Fine aggregate porcelain 450mm.mat").SetColor("_Color",new Color(.62f,.65f,.64f));
        var signage=AssetDatabase.LoadAssetAtPath<Material>(Art+"Backlit yellow acrylic.mat");signage.SetColor("_EmissionColor",new Color(1,.85f,.30f)*.85f);
        foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
            if(r.name.Contains("rib") || r.name.Contains("Rib") || r.name.Contains("stud") || r.name.Contains("grate bar")){
                GameObjectUtility.SetStaticEditorFlags(r.gameObject,StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic);r.receiveGI=ReceiveGI.LightProbes;r.lightProbeUsage=LightProbeUsage.BlendProbes;r.scaleInLightmap=0;
            }
            if(r.name=="Porcelain riser")r.transform.position+=Vector3.left*.003f;
        }
        var p=GameObject.Find("Illuminated exit wayfinding").transform;
        foreach(var t in p.GetComponentsInChildren<TextMesh>())Object.DestroyImmediate(t.gameObject);
        var print=new Material(Shader.Find("Standard"));print.color=Color.white;print.mainTexture=Tex("Assets/Textures/ExitSign.png");print.SetFloat("_Glossiness",.45f);print.EnableKeyword("_EMISSION");print.SetTexture("_EmissionMap",print.mainTexture);print.SetColor("_EmissionColor",Color.white*.95f);Save(print,"Printed luminous exit sign");
        Surface("Bilingual exit face",new Vector3(0,0,-.031f),new Vector2(3.04f,.35f),Quaternion.identity,print,Vector2.one,p);
        var c=GameObject.Find("Security camera notice").transform;foreach(var t in c.GetComponentsInChildren<TextMesh>())Object.DestroyImmediate(t.gameObject);
        var notice=Solid("Printed security notice",Color.white,0,.25f);notice.mainTexture=Tex("Assets/Textures/SecurityNotice.png");Surface("Surveillance print",new Vector3(0,0,-.003f),new Vector2(.53f,.65f),Quaternion.identity,notice,Vector2.one,c);
        var probeGo=new GameObject("Stairwell reflection capture");probeGo.transform.position=new Vector3(5.5f,3,26.3f);var probe=probeGo.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Baked;probe.resolution=256;probe.size=new Vector3(9,7,3.4f);probe.boxProjection=true;probe.blendDistance=1;probe.intensity=.7f;
        var profile=AssetDatabase.LoadAssetAtPath<PostProcessProfile>(Art+"StationPhotography.asset");profile.GetSetting<ScreenSpaceReflections>().enabled.Override(false);profile.GetSetting<AmbientOcclusion>().intensity.Override(.48f);profile.GetSetting<ColorGrading>().temperature.Override(-2);profile.GetSetting<ColorGrading>().postExposure.Override(-.50f);
        RenderSettings.ambientLight=new Color(.10f,.11f,.12f);Lightmapping.lightingSettings.aoMaxDistance=.14f;Lightmapping.lightingSettings.aoExponentIndirect=.3f;Lightmapping.lightingSettings.aoExponentDirect=0;Persist();
    }
    public static void Bake(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");Debug.Log("STATION_V2_BAKE_START");if(!Lightmapping.Bake())throw new Exception("Lightmap bake did not complete");EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("STATION_V2_BAKE_COMPLETE");Capture();}
    public static void Capture(){
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Underground.unity")EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");var cam=Camera.main;var original=cam.transform.position;var rotation=cam.transform.rotation;
        Shot(cam,"corridor-v2",new Vector3(.3f,1.67f,-1.2f),Quaternion.identity);Shot(cam,"ceramic-and-posters-v2",new Vector3(-.8f,1.66f,9.4f),Quaternion.Euler(0,-53,0));Shot(cam,"stairs-v2",new Vector3(-.7f,1.67f,27.05f),Quaternion.Euler(-8,96,0));cam.transform.position=original;cam.transform.rotation=rotation;Debug.Log("STATION_V2_CAPTURE_COMPLETE");
    }
    static void Shot(Camera cam,string name,Vector3 p,Quaternion q){cam.transform.position=p;cam.transform.rotation=q;var rt=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGBHalf);rt.Create();cam.targetTexture=rt;for(int i=0;i<16;i++)cam.Render();RenderTexture.active=rt;var t=new Texture2D(1920,1080,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1920,1080),0,0);t.Apply();var pixels=t.GetPixels();for(int k=0;k<pixels.Length;k++)pixels[k]=pixels[k].gamma;t.SetPixels(pixels);t.Apply();File.WriteAllBytes("Screenshots/"+name+".png",t.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);}
}
