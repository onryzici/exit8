using UnityEditor;
using UnityEngine;
public class CommuterImport:AssetPostprocessor {
 void OnPreprocessModel(){if(!assetPath.StartsWith("Assets/Commuter/"))return;var i=(ModelImporter)assetImporter;i.animationType=ModelImporterAnimationType.Legacy;i.importAnimation=true;i.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;i.isReadable=true;}
 void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Commuter/"))return;var i=(TextureImporter)assetImporter;i.maxTextureSize=2048;i.anisoLevel=8;if(assetPath.Contains("normal")){i.textureType=TextureImporterType.NormalMap;i.convertToNormalmap=false;}}
}
