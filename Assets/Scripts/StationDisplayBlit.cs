using UnityEngine;
public class StationDisplayBlit:MonoBehaviour {
 public StationDisplay owner;
 void OnRenderImage(RenderTexture source,RenderTexture destination){if(owner)owner.Present(destination);else Graphics.Blit(source,destination);}
}
