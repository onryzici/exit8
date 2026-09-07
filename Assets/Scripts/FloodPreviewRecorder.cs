#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEngine;
public class FloodPreviewRecorder:MonoBehaviour {
 IEnumerator Start(){yield return null;var loop=FindFirstObjectByType<CorridorLoop>();loop.player.enabled=false;loop.player.Teleport(new Vector3(0,.04f,12),0,true);var c=loop.player.view;c.transform.localRotation=Quaternion.Euler(0,0,0);loop.SetAnomaly(10);Time.captureFramerate=20;
  Directory.CreateDirectory("SourceAssets/FloodPreview");var rt=new RenderTexture(960,540,24,RenderTextureFormat.ARGBHalf);rt.Create();c.targetTexture=rt;var t=new Texture2D(960,540,TextureFormat.RGB24,false);
  for(int frame=0;frame<140;frame++){yield return null;if(loop.horror.water.activeSelf&&loop.horror.water.GetComponent<Renderer>().sharedMaterial.GetFloat("_FrontZ")<18)loop.player.GetComponent<CharacterController>().Move(Vector3.back*4.6f*Time.deltaTime);c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,960,540),0,0);var pixels=t.GetPixels();for(int i=0;i<pixels.Length;i++)pixels[i]=pixels[i].gamma;t.SetPixels(pixels);t.Apply();File.WriteAllBytes("SourceAssets/FloodPreview/"+frame.ToString("D4")+".png",t.EncodeToPNG());}
  c.targetTexture=null;RenderTexture.active=null;rt.Release();Destroy(rt);Destroy(t);Time.captureFramerate=0;UnityEditor.EditorApplication.Exit(0);
 }
}
#endif
