using UnityEditor;
using UnityEngine;

public class SetupNewKnightIcon
{
    [MenuItem("Tools/Setup New Knight Icon")]
    public static void Execute()
    {
        string path = "Assets/Sprites/Icons/Units/Human/Knight.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            Debug.Log("New Knight icon setup complete.");
        }
        else
        {
            Debug.LogError("Could not find texture at " + path);
        }
    }
}