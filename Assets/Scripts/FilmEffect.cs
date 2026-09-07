using UnityEngine;
[ExecuteAlways] public class FilmEffect:MonoBehaviour {
    public Material material;
    void OnRenderImage(RenderTexture source,RenderTexture destination){if(material)Graphics.Blit(source,destination,material);else Graphics.Blit(source,destination);}
}
