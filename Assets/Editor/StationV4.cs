using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using Object=UnityEngine.Object;

public static class StationV4 {
 const string Art="Assets/ArtV4/";
 static Material steel,paint,rubber,white,glass; static int meshId;
 static Material Mat(string n,Color c,float metallic,float gloss){var m=new Material(Shader.Find("Standard"));m.color=c;m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",gloss);m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ArtV3/GlazeNormal.png"));m.SetFloat("_BumpScale",.045f);m.EnableKeyword("_NORMALMAP");AssetDatabase.CreateAsset(m,Art+n+".mat");return m;}
 static void Static(GameObject g){GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.ContributeGI|StaticEditorFlags.ReflectionProbeStatic|StaticEditorFlags.BatchingStatic);var r=g.GetComponent<MeshRenderer>();r.receiveGI=ReceiveGI.Lightmaps;r.scaleInLightmap=.4f;}
 // Bevelled, closed extruded octagon: actual edge highlights instead of sharp primitive silhouettes.
 static GameObject Part(Transform p,string n,Vector3 pos,Vector3 size,Material mat,float bevel=.003f){
  float x=size.x/2,y=size.y/2,z=size.z/2,b=Mathf.Min(bevel,Mathf.Min(x,Mathf.Min(y,z))*.8f);
  var points=new[]{new Vector2(-x+b,-y),new Vector2(x-b,-y),new Vector2(x,-y+b),new Vector2(x,y-b),new Vector2(x-b,y),new Vector2(-x+b,y),new Vector2(-x,y-b),new Vector2(-x,-y+b)};
  var v=new List<Vector3>();var uv=new List<Vector2>();var tr=new List<int>();
  Action<Vector3,Vector3,Vector3> tri=(a,c,d)=>{int k=v.Count;v.Add(a);v.Add(c);v.Add(d);uv.Add(new Vector2(a.x+a.z,a.y+a.z));uv.Add(new Vector2(c.x+c.z,c.y+c.z));uv.Add(new Vector2(d.x+d.z,d.y+d.z));tr.AddRange(new[]{k,k+1,k+2});};
  var rings=new Vector3[4,8];for(int j=0;j<4;j++)for(int i=0;i<8;i++){var q=points[i];if(j==0||j==3)q=new Vector2(q.x*(x-b)/x,q.y*(y-b)/y);rings[j,i]=new Vector3(q.x,q.y,j==0?-z:j==1?-z+b:j==2?z-b:z);}
  for(int i=0;i<8;i++){int k=(i+1)%8;tri(new Vector3(0,0,-z),rings[0,k],rings[0,i]);tri(new Vector3(0,0,z),rings[3,i],rings[3,k]);for(int j=0;j<3;j++){tri(rings[j,i],rings[j,k],rings[j+1,k]);tri(rings[j,i],rings[j+1,k],rings[j+1,i]);}}
  var mesh=new Mesh{name=n};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tr,0);mesh.RecalculateNormals();mesh.RecalculateTangents();Unwrapping.GenerateSecondaryUVSet(mesh);AssetDatabase.CreateAsset(mesh,Art+"Part"+(meshId++)+".asset");
  var g=new GameObject(n);g.transform.SetParent(p,false);g.transform.localPosition=pos;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;Static(g);return g;
 }
 static GameObject Rod(Transform p,string n,Vector3 a,Vector3 b,float radius,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=(a+b)/2;g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);g.transform.localScale=new Vector3(radius*2,(b-a).magnitude/2,radius*2);Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=m;Static(g);return g;}
 static void Screw(Transform p,float x,float y,float z){Rod(p,"Countersunk screw",new Vector3(x,y,z+.003f),new Vector3(x,y,z),.004f,steel);Part(p,"Screw drive",new Vector3(x,y,z-.0005f),new Vector3(.005f,.001f,.001f),rubber,.0001f);}
 static void Clear(Transform p,Func<Transform,bool> keep=null){for(int i=p.childCount-1;i>=0;i--){var t=p.GetChild(i);if(keep==null||!keep(t))Object.DestroyImmediate(t.gameObject);}}
 static void Door(Transform p){
  Clear(p,t=>t.GetComponent<TextMesh>());foreach(var t in p.GetComponentsInChildren<TextMesh>()){var q=t.transform.localPosition;q.z=-.093f;t.transform.localPosition=q;}
  Part(p,"Continuous EPDM perimeter seal",Vector3.zero,new Vector3(.978f,2.14f,.09f),rubber);
  var leaf=Part(p,"Folded powder coated steel door leaf",new Vector3(0,-.012f,-.049f),new Vector3(.897f,2.066f,.065f),paint,.004f);leaf.AddComponent<BoxCollider>();
  for(int s=-1;s<=1;s+=2){Part(p,"Rebated jamb outer flange",new Vector3(s*.480f,0,-.043f),new Vector3(.048f,2.19f,.10f),paint);Part(p,"Door stop folded return",new Vector3(s*.451f,0,-.035f),new Vector3(.012f,2.1f,.05f),steel);}
  Part(p,"Steel frame head",new Vector3(0,1.078f,-.044f),new Vector3(1.008f,.045f,.102f),paint);
  Part(p,"Brushed threshold",new Vector3(0,-1.055f,-.025f),new Vector3(.94f,.012f,.15f),steel);
  Part(p,"Lower stainless kick plate",new Vector3(0,-.84f,-.083f),new Vector3(.847f,.27f,.002f),steel,.0005f);
  foreach(float y in new[]{-.72f,0,.74f}){Part(p,"Hinge mounting leaf",new Vector3(-.444f,y,-.085f),new Vector3(.033f,.105f,.005f),steel);Rod(p,"Three knuckle hinge barrel",new Vector3(-.46f,y-.05f,-.098f),new Vector3(-.46f,y+.05f,-.098f),.009f,steel);foreach(float d in new[]{-.026f,.026f})Rod(p,"Hinge knuckle seam",new Vector3(-.46f,y+d,-.098f),new Vector3(-.46f,y+d+.001f,-.098f),.0094f,rubber);}
  Rod(p,"Lever rose",new Vector3(.32f,-.12f,-.083f),new Vector3(.32f,-.12f,-.095f),.029f,steel);
  Rod(p,"Lever spindle",new Vector3(.32f,-.12f,-.095f),new Vector3(.32f,-.12f,-.142f),.012f,steel);
  Rod(p,"Return lever grip",new Vector3(.32f,-.12f,-.142f),new Vector3(.20f,-.12f,-.142f),.010f,steel);
  Rod(p,"Lever safety return",new Vector3(.20f,-.12f,-.142f),new Vector3(.20f,-.12f,-.117f),.010f,steel);
  Rod(p,"Euro cylinder escutcheon",new Vector3(.32f,-.24f,-.083f),new Vector3(.32f,-.24f,-.091f),.023f,steel);Part(p,"Keyway",new Vector3(.32f,-.24f,-.092f),new Vector3(.003f,.014f,.002f),rubber);
  Part(p,"Hydraulic door closer",new Vector3(-.22f,.91f,-.12f),new Vector3(.22f,.055f,.074f),steel,.008f);
  Rod(p,"Closer articulated arm",new Vector3(-.22f,.943f,-.13f),new Vector3(.025f,.943f,-.18f),.006f,steel);Rod(p,"Closer return arm",new Vector3(.025f,.943f,-.18f),new Vector3(.18f,1.044f,-.08f),.006f,steel);
  foreach(float x in new[]{-.39f,.39f})foreach(float y in new[]{-.95f,-.73f})Screw(p,x,y,-.086f);
 }
 static void Vent(Transform p){
  Clear(p);Part(p,"Deep black duct cavity",new Vector3(0,0,-.014f),new Vector3(.58f,.365f,.03f),rubber);
  for(int s=-1;s<=1;s+=2){Part(p,"Extruded grille side frame",new Vector3(s*.304f,0,-.053f),new Vector3(.029f,.412f,.082f),steel);Part(p,"Extruded grille horizontal frame",new Vector3(0,s*.191f,-.053f),new Vector3(.58f,.03f,.082f),steel);}
  for(int i=0;i<9;i++){var g=Part(p,"Folded louvre blade",new Vector3(0,-.155f+i*.038f,-.071f),new Vector3(.579f,.004f,.058f),steel,.001f);g.transform.localRotation=Quaternion.Euler(-32,0,0);Part(p,"Louvre rolled leading lip",new Vector3(0,-.14f+i*.038f,-.095f),new Vector3(.574f,.006f,.004f),steel,.001f);}
  for(int i=0;i<19;i++)Rod(p,"Insect screen vertical wire",new Vector3(-.27f+i*.03f,-.17f,-.034f),new Vector3(-.27f+i*.03f,.17f,-.034f),.0007f,steel);
  foreach(float x in new[]{-.302f,.302f})foreach(float y in new[]{-.165f,.165f})Screw(p,x,y,-.096f);
 }
 static void Sign(Transform p){
  Clear(p,t=>t.name=="Bilingual exit face");var face=p.Find("Bilingual exit face");face.localPosition=new Vector3(0,0,-.097f);
  Part(p,"Double sided lightbox aluminium housing",new Vector3(0,0,.005f),new Vector3(3.12f,.43f,.18f),paint,.012f);
  Part(p,"Recessed neoprene face gasket",new Vector3(0,0,-.088f),new Vector3(3.069f,.379f,.011f),rubber);
  for(int s=-1;s<=1;s+=2){Part(p,"Lightbox horizontal retaining extrusion",new Vector3(0,s*.194f,-.09f),new Vector3(3.095f,.023f,.025f),steel);Part(p,"Removable lightbox end cap",new Vector3(s*1.552f,0,0),new Vector3(.016f,.421f,.18f),steel);Screw(p,s*1.53f,.176f,-.105f);Screw(p,s*1.53f,-.176f,-.105f);Part(p,"Suspension mounting collar",new Vector3(s*1.2f,.221f,0),new Vector3(.065f,.023f,.065f),steel);}
  var back=Object.Instantiate(face.gameObject,p);back.name="Rear printed acrylic face";back.transform.localPosition=new Vector3(0,0,.101f);back.transform.localRotation=Quaternion.Euler(0,180,0);
 }
 static void Fixture(Transform p){
  Clear(p,t=>t.GetComponent<Light>());
  Part(p,"Pressed steel ballast housing",new Vector3(0,.007f,0),new Vector3(1.38f,.089f,.252f),white,.012f);
  Part(p,"Reflector central spine",new Vector3(0,-.043f,0),new Vector3(1.29f,.012f,.09f),white);
  for(int s=-1;s<=1;s+=2){var wing=Part(p,"Angled enamel reflector wing",new Vector3(0,-.042f,s*.087f),new Vector3(1.28f,.007f,.104f),white);wing.transform.localRotation=Quaternion.Euler(s*24,0,0);
   Part(p,"Folded reflector outer lip",new Vector3(0,-.022f,s*.135f),new Vector3(1.31f,.022f,.006f),white);
   Rod(p,"T8 26 mm phosphor tube",new Vector3(-.577f,-.095f,s*.074f),new Vector3(.577f,-.095f,s*.074f),.013f,glass);
   foreach(int end in new[]{-1,1}){Rod(p,"Aluminium lamp end cap",new Vector3(end*.578f,-.095f,s*.074f),new Vector3(end*.604f,-.095f,s*.074f),.0134f,steel);Part(p,"G13 bi pin lampholder",new Vector3(end*.628f,-.078f,s*.074f),new Vector3(.039f,.073f,.039f),white,.007f);Rod(p,"Lamp socket dark seating ring",new Vector3(end*.606f,-.095f,s*.074f),new Vector3(end*.610f,-.095f,s*.074f),.014f,rubber);}
  }
  foreach(int end in new[]{-1,1}){Part(p,"Pressed fixture end plate",new Vector3(end*.681f,-.016f,0),new Vector3(.009f,.102f,.268f),white);Part(p,"Ceiling mounting foot",new Vector3(end*.45f,.067f,0),new Vector3(.12f,.03f,.10f),steel);}
  foreach(var l in p.GetComponentsInChildren<Light>()){if(l.type==LightType.Point)l.intensity=.08f;else{l.areaSize=new Vector2(1.19f,.17f);l.intensity=8;}}
 }
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");Directory.CreateDirectory(Art);
  steel=Mat("Satin brushed aluminium",new Color(.62f,.64f,.66f),.94f,.61f);steel.SetTextureScale("_BumpMap",new Vector2(2,45));
  paint=Mat("Powder coated steel",new Color(.45f,.48f,.48f),0,.39f);rubber=Mat("EPDM and cavity",new Color(.024f,.026f,.027f),0,.22f);white=Mat("Vitreous enamel reflector",new Color(.82f,.83f,.81f),0,.57f);
  glass=Mat("T8 phosphor glass",new Color(.9f,.94f,.97f),0,.72f);glass.EnableKeyword("_EMISSION");glass.SetColor("_EmissionColor",new Color(.94f,.97f,1)*4.8f);glass.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;
  bool savedDoor=false,savedVent=false,savedFixture=false;
  foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)){
   if(!t)continue;
   if(t.name.StartsWith("Recessed utility door")){Door(t);if(!savedDoor){PrefabUtility.SaveAsPrefabAsset(t.gameObject,Art+"UtilityDoor.prefab");savedDoor=true;}}
   else if(t.name=="Wall ventilation grille"){Vent(t);if(!savedVent){PrefabUtility.SaveAsPrefabAsset(t.gameObject,Art+"LouvredVent.prefab");savedVent=true;}}
   else if(t.name=="Illuminated exit wayfinding"){Sign(t);PrefabUtility.SaveAsPrefabAsset(t.gameObject,Art+"ExitLightbox.prefab");}
   else if(t.name=="Twin fluorescent / 4000 K"){Fixture(t);if(!savedFixture){PrefabUtility.SaveAsPrefabAsset(t.gameObject,Art+"TwinT8Fixture.prefab");savedFixture=true;}}
  }
  var wall=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Glazed ceramic 300mm.mat");wall.SetFloat("_Gloss",.86f);wall.SetFloat("_RoughnessWeight",.16f);wall.SetFloat("_MicroStrength",.16f);
  var floor=AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtV2/Fine aggregate porcelain 450mm.mat");floor.SetFloat("_Gloss",.58f);floor.SetFloat("_RoughnessWeight",.25f);floor.SetFloat("_MicroStrength",.12f);
  var profile=AssetDatabase.LoadAssetAtPath<PostProcessProfile>("Assets/ArtV2/StationPhotography.asset");var ssr=profile.GetSetting<ScreenSpaceReflections>();ssr.enabled.Override(true);ssr.preset.Override(ScreenSpaceReflectionPreset.High);ssr.maximumMarchDistance.Override(18);ssr.distanceFade.Override(.65f);ssr.vignette.Override(.5f);profile.GetSetting<Bloom>().intensity.Override(.16f);
  foreach(var probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)){probe.boxProjection=true;probe.resolution=512;probe.blendDistance=1.5f;}
  Save();if(!Lightmapping.Bake())throw new Exception("V4 bake failed");Save();Capture();BuildStation.BuildPlayer();Debug.Log("STATION_V4_COMPLETE");
 }
 static void Save(){foreach(var folder in new[]{"Assets/ArtV2","Assets/ArtV3","Assets/ArtV4"})foreach(var guid in AssetDatabase.FindAssets("",new[]{folder}))foreach(var a in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)))if(a)EditorUtility.SetDirty(a);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
 public static void Finish(){EditorSceneManager.OpenScene("Assets/Scenes/Underground.unity");foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){if(l.name=="Baked rectangular light")l.intensity=14;if(l.name=="Realtime specular support")l.intensity=.18f;if(l.name.StartsWith("Diffuse floor return"))l.intensity=.30f;}var profile=AssetDatabase.LoadAssetAtPath<PostProcessProfile>("Assets/ArtV2/StationPhotography.asset");profile.GetSetting<ColorGrading>().postExposure.Override(.10f);Save();if(!Lightmapping.Bake())throw new Exception("Lighting bake failed");Save();Capture();BuildStation.BuildPlayer();Debug.Log("STATION_V4_FINAL_COMPLETE");}
 public static void Capture(){var c=Camera.main;var p=c.transform.position;var q=c.transform.rotation;Shot(c,"corridor-v4",new Vector3(.3f,1.67f,-1.2f),Quaternion.identity);Shot(c,"door-v4",new Vector3(.15f,1.55f,5.9f),Quaternion.Euler(4,60,0));Shot(c,"fixture-v4",new Vector3(.05f,2.25f,-1.1f),Quaternion.Euler(-32,0,0));c.transform.position=p;c.transform.rotation=q;}
 static void Shot(Camera cam,string n,Vector3 p,Quaternion q){cam.transform.SetPositionAndRotation(p,q);var rt=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGBHalf);rt.Create();cam.targetTexture=rt;for(int i=0;i<24;i++)cam.Render();RenderTexture.active=rt;var t=new Texture2D(1920,1080,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1920,1080),0,0);var px=t.GetPixels();for(int i=0;i<px.Length;i++)px[i]=px[i].gamma;t.SetPixels(px);t.Apply();File.WriteAllBytes("Screenshots/"+n+".png",t.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);}
}
