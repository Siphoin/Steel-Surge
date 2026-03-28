using UnityEditor;
using UnityEngine;

public class SetupFootmanIcon
{
    [MenuItem("Tools/Setup Footman Icon")]
    public static void Execute()
    {
        string path = "Assets/Sprites/Icons/Units/Human/Footman.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            Debug.Log("Footman icon setup complete.");
        }
        else
        {
            Debug.LogError("Could not find texture at " + path);
        }
    }
}