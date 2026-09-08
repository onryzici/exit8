#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

public class EncounterVisualCheck : MonoBehaviour
{
    readonly StringBuilder report=new StringBuilder();
    CorridorLoop loop;
    string folder="Temp/VisualReview";
    void Check(bool pass,string name){report.AppendLine((pass?"PASS ":"FAIL ")+name);}
    void Capture(string name)
    {
        var camera=loop.player.view;
        var old=camera.targetTexture;var active=RenderTexture.active;
        var rt=RenderTexture.GetTemporary(1440,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
        camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
        var image=new Texture2D(1440,900,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,1440,900),0,0);image.Apply();
        File.WriteAllBytes(folder+"/"+name+".png",image.EncodeToPNG());
        camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);Destroy(image);
    }
    IEnumerator CheckWalkingAudio(float speed,float duration,string label)
    {
        loop.player.Teleport(new Vector3(-.6f,.04f,3),0,true);
        var motor=loop.player.GetComponent<CharacterController>();var sound=loop.player.GetComponent<StationFootsteps>();
        yield return null;
        int before=sound.StepsPlayed,observed=before;float previousStep=-100,minGap=100;
        float began=Time.time;
        while(Time.time-began<duration){
            float lateral=Mathf.Sin((Time.time-began)*3)*.8f;
            motor.Move(new Vector3(lateral,-2,speed)*Time.deltaTime);
            yield return new WaitForEndOfFrame();
            if(sound.StepsPlayed>observed){minGap=Mathf.Min(minGap,sound.LastStepTime-previousStep);previousStep=sound.LastStepTime;observed=sound.StepsPlayed;}
            yield return null;
        }
        int count=sound.StepsPlayed-before;
        Check(count>=5&&count<=12,label+" cadence across floor strips: "+count+" steps");
        Check(minGap>=.239f,label+" has no double triggers; minimum interval="+minGap);
        bool smooth=true;
        foreach(var clip in sound.clips){
            var first=new float[clip.channels];var last=new float[clip.channels];
            clip.GetData(first,0);clip.GetData(last,clip.samples-1);
            foreach(float value in first)smooth&=Mathf.Abs(value)<.001f;
            foreach(float value in last)smooth&=Mathf.Abs(value)<.001f;
        }
        Check(smooth,"Footstep clip boundaries are silent ("+label+")");
    }
    IEnumerator Start()
    {
        Directory.CreateDirectory(folder);
        yield return null;
        loop=FindAnyObjectByType<CorridorLoop>();
        loop.ResetRun();loop.player.enabled=false;
        var camera=loop.player.view;var localPosition=camera.transform.localPosition;var localRotation=camera.transform.localRotation;
        loop.player.Teleport(new Vector3(0,.04f,6),0,true);
        var commuter=loop.commuter;
        Check(commuter.transform.position.x>4.5f&&commuter.transform.position.z>25,"Commuter starts inside distant right passage");
        yield return new WaitForSeconds(.2f);Capture("01-hidden-entry");
        Check(commuter.FootstepAudibility<.5f,"Wall muffles footsteps before corner");
        yield return new WaitForSeconds(2.6f);
        float oldFov=camera.fieldOfView;camera.fieldOfView=18;Capture("02a-corner-lighting");camera.fieldOfView=oldFov;
        var sample=commuter.LightingSamplePosition;
        Check(Mathf.Abs(sample.x)<=1.5f&&sample.z<=26&&sample.y>1,"Corner lighting stays inside baked probe coverage");
        UnityEngine.Rendering.SphericalHarmonicsL2 light;
        LightProbes.GetInterpolatedProbe(sample,null,out light);
        var colors=new Color[6];light.Evaluate(new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back},colors);
        float brightness=0;foreach(var color in colors)brightness+=color.grayscale/6;
        Check(brightness>.005f,"Corner lighting is nonblack; irradiance="+brightness);
        yield return new WaitForSeconds(1.9f);Capture("02-corner-entry");
        Check(commuter.DistanceWalked>4,"Walk starts without waiting for player");
        Check(commuter.transform.position.x<1.4f,"Commuter clears right corner");
        float maxGripError=0;
        for(int i=0;i<100;i++){yield return new WaitForEndOfFrame();maxGripError=Mathf.Max(maxGripError,commuter.GripError);}
        Check(maxGripError<.003f,"Handle remains in palm throughout animation; max error="+maxGripError);
        var hand=commuter.GripPosition;
        camera.transform.position=hand+new Vector3(-1.1f,.38f,-.75f);camera.transform.LookAt(hand+Vector3.down*.08f);
        Capture("03-carrying-hand");
        camera.transform.position=commuter.transform.position+new Vector3(-2.2f,1.4f,-1.8f);camera.transform.LookAt(commuter.transform.position+Vector3.up*.95f);
        Capture("04-commuter");
        camera.transform.localPosition=localPosition;camera.transform.localRotation=localRotation;
        loop.player.Teleport(new Vector3(0,.04f,12),0,true);
        loop.SetAnomaly(10);
        yield return new WaitForSeconds(.4f);
        Check(!loop.horror.FloodVisible,"Flood remains hidden during warning");
        bool middle=false,near=false;
        float deadline=Time.time+10;
        while(loop.ActiveAnomaly==10&&Time.time<deadline)
        {
            if(loop.horror.FloodVisible)
            {
                float front=loop.horror.water.GetComponent<Renderer>().sharedMaterial.GetFloat("_FrontZ");
                if(!middle&&front<21){Capture("05-surge-far");middle=true;}
                if(!near&&front<16){Capture("06-surge-near");Check(!commuter.Walking,"Flood overtakes commuter without visible walking through water");near=true;}
            }
            yield return null;
        }
        Check(middle&&near,"Surge travels continuously toward player");
        Check(loop.ActiveAnomaly==-1&&!loop.horror.FloodVisible&&!loop.horror.FloodTriggered,"Flood catch resets encounter");
        loop.ResetRun();Check(commuter.Walking,"Commuter restarts after flood reset");
        yield return CheckWalkingAudio(2.65f,4,"walking");
        yield return CheckWalkingAudio(4.6f,3,"running");
        loop.ResetRun();loop.player.enabled=true;
        File.WriteAllText(folder+"/encounter-validation.txt",report.ToString());Debug.Log(report.ToString());
        Destroy(gameObject);
    }
}
#endif
