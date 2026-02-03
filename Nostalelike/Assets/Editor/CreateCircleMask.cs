using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Erstellt ein rundes Mask-Sprite für die Minimap
/// </summary>
public class CreateCircleMask : EditorWindow
{
    [MenuItem("Tools/Create Circle Mask Sprite")]
    public static void CreateMask()
    {
        int size = 512;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        // Jeden Pixel durchgehen
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);

                // Weißer Kreis mit Anti-Aliasing
                if (distance <= radius - 2)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else if (distance <= radius)
                {
                    // Soft edge
                    float alpha = (radius - distance) / 2f;
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();

        // Speichern
        string path = "Assets/4_UI_Elements/MinimapCircleMask.png";
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        AssetDatabase.Refresh();

        // Import Settings
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Debug.Log("Circle Mask erstellt: " + path);
        EditorUtility.DisplayDialog("Fertig!", "Circle Mask Sprite wurde erstellt:\n" + path, "OK");
    }
}
