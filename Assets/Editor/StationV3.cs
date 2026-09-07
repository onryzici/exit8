using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using Object=UnityEngine.Object;

public static class StationV3 {
    const string Art="Assets/ArtV3/";
    const float Height=3.55f;
    static Transform ceilingRoot;
    static Material ceiling,metal;
    static Texture2D micro,finish;
    public static void BuildBakeExport(){Upgrade();BakeCapture();BuildStation.BuildPlayer();}
    static float Noise(float x,float y){return Mathf.PerlinNoise(x,y)*.62f+Mathf.PerlinNoise(x*3.7f+11,y*3.7f+19)*.26f+Mathf.PerlinNoise(x*13.1f+7,y*13.1f+37)*.12f;}
    static float Periodic(float u,float v){float a=Noise(u*32,v*32),b=Noise((u-1)*32,v*32),c=Noise(u*32,(v-1)*32),d=Noise((u-1)*32,(v-1)*32);return Mathf.Lerp(Mathf.Lerp(a,b,u),Mathf.Lerp(c,d,u),v);}
    static void DetailMaps(){
        const int n=1024;var values=new float[n*n];var normals=new Color[n*n];var mask=new Color[n*n];
        for(int y=0;y<n;y++)for(int x=0;x<n;x++)values[y*n+x]=Periodic(x/(float)n,y/(float)n);
        for(int y=0;y<n;y++)for(int x=0;x<n;x++){
            float dx=values[y*n+(x+1)%n]-values[y*n+(x+n-1)%n],dy=values[((y+1)%n)*n+x]-values[((y+n-1)%n)*n+x];var normal=new Vector3(-dx*2.6f,-dy*2.6f,1).normalized;
            normals[y*n+x]=new Color(normal.x*.5f+.5f,normal.y*.5f+.5f,normal.z*.5f+.5f,1);float f=values[y*n+x];mask[y*n+x]=new Color(f,f,f,1);
        }
        WriteMap("GlazeNormal",normals,n,true);WriteMap("FinishVariation",mask,n,false);micro=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"GlazeNormal.png");finish=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"FinishVariation.png");
    }
    static void WriteMap(string name,Color[] data,int n,bool normal){var t=new Texture2D(n,n,TextureFormat.RGB24,false,true);t.SetPixels(data);t.Apply();string path=Art+name+".png";File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;imp.sRGBTexture=false;imp.anisoLevel=16;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.SaveAndReimport();}
    static void Static(GameObject go,float scale=1){GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.ContributeGI|StaticEditorFlags.ReflectionProbeStatic|StaticEditorFlags.BatchingStatic);var r=go.GetComponent<MeshRenderer>();r.receiveGI=ReceiveGI.Lightmaps;r.scaleInLightmap=scale;}
    static GameObject Box(string name,Vector3 p,Vector3 s,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(ceilingRoot);g.transform.position=p;g.transform.localScale=s;Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=m;Static(g);return g;}
    static Mesh CeilingProfile(){
        // A roll-formed aluminium strip with a 2 mm lip, curved lower corners, and folded returns.
        var cross=new[]{new Vector2(-.0735f,.019f),new Vector2(-.0735f,.004f),new Vector2(-.073f,-.001f),new Vector2(-.0715f,-.003f),new Vector2(-.0685f,-.004f),new Vector2(.0685f,-.004f),new Vector2(.0715f,-.003f),new Vector2(.073f,-.001f),new Vector2(.0735f,.004f),new Vector2(.0735f,.019f)};
        var vs=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
        for(int j=0;j<cross.Length;j++){vs.Add(new Vector3(-2.073f,cross[j].y,cross[j].x));vs.Add(new Vector3(2.073f,cross[j].y,cross[j].x));uv.Add(new Vector2(0,j*.035f));uv.Add(new Vector2(8.3f,j*.035f));}
        for(int j=0;j<cross.Length-1;j++){int a=j*2;tris.AddRange(new[]{a,a+1,a+3,a,a+3,a+2});}
        var m=new Mesh{name="Folded aluminium ceiling section"};m.SetVertices(vs);m.SetUVs(0,uv);m.SetTriangles(tris,0);m.RecalculateNormals();m.RecalculateTangents();Unwrapping.GenerateSecondaryUVSet(m);AssetDatabase.CreateAsset(m,Art+"CeilingProfile.asset");return m;
    }
    public static void Upgrade(){
        EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");if(GameObject.Find("V3 / folded metal ceiling"))throw new Exception("V3 already applied; use BakeCapture or Export.");
        Directory.CreateDirectory(Art);DetailMaps();
        var sceneRoot=GameObject.Find("EXIT 7 / architectural environment").transform;
        var all=sceneRoot.GetComponentsInChildren<Transform>();
        foreach(var tr in all){
            if(!tr)continue;
            if(tr.name=="Enamel ceiling strip"){Object.DestroyImmediate(tr.gameObject);continue;}
            if(tr.name=="West structural wall"||tr.name=="East structural wall"||tr.name=="End structural wall"||tr.name=="Entrance wall"){
                var p=tr.position;p.y=Height*.5f;tr.position=p;var s=tr.localScale;s.y=Height;tr.localScale=s;
            }
            if(tr.name=="West glazed ceramic"||tr.name=="East glazed ceramic"||tr.name=="End ceramic"||tr.name=="Entrance ceramic"){
                var f=tr.GetComponent<MeshFilter>();var mesh=Object.Instantiate(f.sharedMesh);var vs=mesh.vertices;var uv=mesh.uv;
                for(int i=0;i<vs.Length;i++){vs[i].y*=Height/3;uv[i].y*=Height/3;}mesh.vertices=vs;mesh.uv=uv;mesh.RecalculateBounds();Unwrapping.GenerateSecondaryUVSet(mesh);AssetDatabase.CreateAsset(mesh,Art+tr.name+".asset");f.sharedMesh=mesh;tr.position=new Vector3(tr.position.x,Height*.5f,tr.position.z);
            }
            if(tr.name=="Ceiling structure"||tr.name=="Twin fluorescent / 4000 K"||tr.name=="CCTV ceiling base"||tr.name=="Smoked CCTV hemisphere"||tr.name=="Ceiling maintenance hatch"||tr.name=="Hatch face")tr.position+=Vector3.up*.55f;
            if(tr.name=="Illuminated exit wayfinding")tr.position+=Vector3.up*.30f;
            if(tr.name=="Wall ventilation grille")tr.position+=Vector3.up*.25f;
            if(tr.name=="Suspension rod"){tr.position=new Vector3(tr.position.x,3.3025f,tr.position.z);tr.localScale=new Vector3(.016f,.2375f,.016f);}
        }
        ceilingRoot=new GameObject("V3 / folded metal ceiling").transform;ceilingRoot.SetParent(sceneRoot);
        ceiling=new Material(Shader.Find("Standard"));ceiling.color=new Color(.63f,.635f,.62f);ceiling.SetFloat("_Metallic",.08f);ceiling.SetFloat("_Glossiness",.64f);ceiling.SetTexture("_BumpMap",micro);ceiling.SetFloat("_BumpScale",.16f);ceiling.EnableKeyword("_NORMALMAP");AssetDatabase.CreateAsset(ceiling,Art+"Satin enamel aluminium.mat");
        metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Brushed stainless.mat");
        var meshProfile=CeilingProfile();
        for(int i=0;i<200;i++){var go=new GameObject("150 mm folded aluminium strip");go.transform.SetParent(ceilingRoot);go.transform.position=new Vector3(0,Height,-1.925f+i*.15f);go.AddComponent<MeshFilter>().sharedMesh=meshProfile;go.AddComponent<MeshRenderer>().sharedMaterial=ceiling;Static(go,.65f);}
        for(int side=-1;side<=1;side+=2){Box("Perimeter trim horizontal lip",new Vector3(side*2.065f,3.525f,13),new Vector3(.055f,.016f,30),ceiling);Box("Perimeter trim wall return",new Vector3(side*2.087f,3.54f,13),new Vector3(.012f,.055f,30),ceiling);}
        // Framed, flush access panels use a real reveal and recessed latch.
        foreach(float z in new[]{10.7f,21.05f}){
            Box("Access panel dark reveal",new Vector3(.82f,3.531f,z),new Vector3(.608f,.012f,.608f),AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Shadow and gaskets.mat"));
            Box("Flush service panel",new Vector3(.82f,3.521f,z),new Vector3(.588f,.016f,.588f),ceiling);
            Box("Quarter turn hatch latch",new Vector3(.82f,3.510f,z-.23f),new Vector3(.036f,.005f,.018f),metal);
        }
        foreach(var r in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)){r.resolution=512;if(r.name.StartsWith("Local reflection")){r.size=new Vector3(4.2f,Height,9);r.transform.position=new Vector3(0,Height*.5f,r.transform.position.z);r.intensity=1;}}
        TuneMaterials();
        foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){
            if(l.name=="Baked rectangular light"){l.intensity=8;l.color=new Color(.97f,.98f,1);}
            if(l.name=="Realtime specular support"){l.intensity=.85f;l.range=6;l.color=new Color(.96f,.98f,1);}
            if(l.name.StartsWith("Diffuse floor return"))l.intensity=.12f;
        }
        var profile=AssetDatabase.LoadAssetAtPath<PostProcessProfile>("Assets/ArtV2/StationPhotography.asset");profile.GetSetting<ColorGrading>().postExposure.Override(-.35f);profile.GetSetting<ColorGrading>().temperature.Override(-1);profile.GetSetting<ColorGrading>().contrast.Override(12);profile.GetSetting<ColorGrading>().saturation.Override(-5);profile.GetSetting<Bloom>().intensity.Override(.20f);profile.GetSetting<Bloom>().threshold.Override(1.4f);profile.GetSetting<Vignette>().intensity.Override(.10f);
        Lightmapping.lightingSettings.indirectSampleCount=512;Lightmapping.lightingSettings.directSampleCount=128;Lightmapping.lightingSettings.lightmapResolution=28;
        Persist();Debug.Log("STATION_V3_HEIGHT_3_55_M_READY");
    }
    static void TuneMaterials(){
        var wall=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Glazed ceramic 300mm.mat");
        var floor=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Fine aggregate porcelain 450mm.mat");
        foreach(var m in new[]{wall,floor}){m.SetTexture("_MicroNormal",micro);m.SetTexture("_Finish",finish);m.SetFloat("_MicroScale",8);}
        wall.SetFloat("_Gloss",.94f);wall.SetFloat("_RoughnessWeight",.13f);wall.SetFloat("_MicroStrength",.42f);wall.SetFloat("_NormalStrength",.7f);wall.SetFloat("_Grout",.34f);wall.SetColor("_Color",new Color(.86f,.86f,.83f));
        floor.SetFloat("_Gloss",.64f);floor.SetFloat("_RoughnessWeight",.28f);floor.SetFloat("_MicroStrength",.22f);floor.SetFloat("_Flatness",.64f);floor.SetColor("_Color",new Color(.51f,.52f,.51f));
        metal.SetTexture("_BumpMap",micro);metal.SetTextureScale("_BumpMap",new Vector2(6,35));metal.SetFloat("_BumpScale",.08f);metal.EnableKeyword("_NORMALMAP");metal.SetFloat("_Glossiness",.74f);
        var door=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Powder coated warm grey.mat");door.SetTexture("_BumpMap",micro);door.SetFloat("_BumpScale",.10f);door.EnableKeyword("_NORMALMAP");door.SetFloat("_Glossiness",.48f);door.color=new Color(.46f,.49f,.48f);
        var paper=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Photo print satin laminate.mat");paper.SetFloat("_Glossiness",.57f);
    }
    static void Persist(){foreach(var folder in new[]{"Assets/ArtV2","Assets/ArtV3"})foreach(var guid in AssetDatabase.FindAssets("",new[]{folder}))foreach(var a in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)))if(a)EditorUtility.SetDirty(a);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
    public static void BakeCapture(){if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Underground.unity")EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");if(!Lightmapping.Bake())throw new Exception("V3 bake failed");Persist();Capture();Debug.Log("STATION_V3_BAKE_COMPLETE");}
    public static void Capture(){
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Underground.unity")EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");
        var cam=Camera.main;var p=cam.transform.position;var q=cam.transform.rotation;
        Shot(cam,"corridor-v3",new Vector3(.3f,1.67f,-1.2f),Quaternion.identity);Shot(cam,"ceiling-v3",new Vector3(.3f,1.67f,1),Quaternion.Euler(-25,0,0));Shot(cam,"surface-v3",new Vector3(-1.1f,1.65f,10.1f),Quaternion.Euler(-4,-62,0));cam.transform.position=p;cam.transform.rotation=q;
    }
    static void Shot(Camera cam,string name,Vector3 p,Quaternion q){cam.transform.position=p;cam.transform.rotation=q;var rt=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGBHalf);rt.Create();cam.targetTexture=rt;for(int i=0;i<20;i++)cam.Render();RenderTexture.active=rt;var t=new Texture2D(1920,1080,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1920,1080),0,0);var pixels=t.GetPixels();for(int i=0;i<pixels.Length;i++)pixels[i]=pixels[i].gamma;t.SetPixels(pixels);t.Apply();File.WriteAllBytes("Screenshots/"+name+".png",t.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);}
}
