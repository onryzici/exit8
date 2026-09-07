using System.Collections;
using System.IO;
using UnityEngine;

// Created only by the editor QA command; never part of the saved playable scene.
public class StationRuntimeCheck:MonoBehaviour {
    IEnumerator Start(){
        yield return null;
        var fps=FindFirstObjectByType<FirstPerson>();fps.enabled=false;
        var cc=fps.GetComponent<CharacterController>();
        yield return new WaitForSeconds(.1f);
        bool ok=true;string report="Station runtime collision and feedback checks\n";
        cc.enabled=false;cc.transform.position=new Vector3(0,.12f,1);cc.enabled=true;
        for(int i=0;i<45;i++){cc.Move(Vector3.down*.035f);yield return null;}
        bool floor=cc.isGrounded && cc.transform.position.y>-.1f;ok&=floor;report+="Ground contact: "+floor+"\n";
        for(int i=0;i<50;i++){cc.Move(new Vector3(.07f,-.03f,0));yield return null;}
        float eastLimit=GameObject.Find("East structural wall").GetComponent<Renderer>().bounds.min.x;
        bool wall=cc.transform.position.x<eastLimit-.10f;ok&=wall;report+="East wall blocks player: "+wall+"\n";
        var feet=fps.GetComponent<StationFootsteps>();
        bool steps=feet && feet.StepsPlayed>0;ok&=steps;report+="Walking triggers footsteps: "+steps+"\n";
        int before=feet?feet.StepsPlayed:0;
        for(int i=0;i<30;i++){cc.Move(new Vector3(.07f,-.03f,0));yield return null;}
        bool quiet=feet && feet.StepsPlayed==before;ok&=quiet;report+="Blocked movement is silent: "+quiet+"\n";
        var profile=FindFirstObjectByType<UnityEngine.Rendering.PostProcessing.PostProcessVolume>().profile;
        var blur=profile.GetSetting<UnityEngine.Rendering.PostProcessing.MotionBlur>();
        var rotation=fps.view.transform.rotation;
        for(int i=0;i<30;i++){fps.view.transform.Rotate(0,3,0);yield return null;}
        bool turning=blur && blur.shutterAngle.value>0 && blur.shutterAngle.value<=fps.view.GetComponent<TurningMotionBlur>().maximumShutterAngle+.1f;ok&=turning;report+="Turning applies configured motion blur: "+turning+"\n";
        yield return new WaitForSeconds(.8f);
        bool still=blur && blur.shutterAngle.value<1;ok&=still;report+="Stationary view clears motion blur: "+still+"\n";
        fps.view.transform.rotation=rotation;
        cc.enabled=false;cc.transform.position=new Vector3(0,.04f,23);cc.enabled=true;
        for(int i=0;i<65;i++){cc.Move(new Vector3(0,-.03f,.05f));yield return null;}
        bool approach=cc.transform.position.z>26.1f;ok&=approach;report+="Tactile paving and drain traversable: "+approach+"\n";
        for(int i=0;i<320;i++){cc.Move(new Vector3(.05f,-.025f,0));yield return null;}
        bool stairs=cc.transform.position.x>9.0f && cc.transform.position.y>3.35f;ok&=stairs;report+="Stair ascent to landing: "+stairs+"; position="+cc.transform.position+"\n";
        report+="Lightmaps: "+LightmapSettings.lightmaps.Length+"\n";ok&=LightmapSettings.lightmaps.Length>0;
        report+="RESULT: "+(ok?"PASS":"FAIL");File.WriteAllText("runtime-validation.txt",report);Debug.Log(report);
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(ok?0:1);
        #endif
    }
}
