using UnityEditor;
using UnityEngine;

public class SetupIcons
{
    [MenuItem("Tools/Setup Icons")]
    public static void Execute()
    {
        string[] paths = new string[] 
        {
            "Assets/Sprites/Icons/Units/Human/Archer.png",
            "Assets/Sprites/Icons/Units/Human/Knight.png"
        };

        foreach (string path in paths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                Debug.Log("Icon setup complete for: " + path);
            }
            else
            {
                Debug.LogError("Could not find texture at " + path);
            }
        }
    }
}