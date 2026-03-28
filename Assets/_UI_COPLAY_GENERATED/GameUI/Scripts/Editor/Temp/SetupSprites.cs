using UnityEditor;
using UnityEngine;

public class SetupSprites
{
    [MenuItem("Tools/Setup UI Sprites")]
    public static void Setup()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_UI_COPLAY_GENERATED/GameUI/Sprites" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;

                if (path.Contains("MapStatusPanel") || path.Contains("MatchControlPanel") || 
                    path.Contains("QuickActionPanel") || path.Contains("BottomScorePanel"))
                {
                    importer.spriteBorder = new Vector4(10, 10, 10, 10);
                }

                importer.SaveAndReimport();
            }
        }
        Debug.Log("Sprites setup complete.");
    }
}