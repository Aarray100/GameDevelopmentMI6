using UnityEngine;
using UnityEditor;

public class TextureFixer : EditorWindow
{
    [MenuItem("Tools/Fix All Textures")]
    public static void FixTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
        Debug.Log("Alle Texturen sind jetzt scharf!");
    }
}
