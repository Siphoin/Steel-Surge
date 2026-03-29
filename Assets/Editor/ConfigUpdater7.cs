using UnityEngine;
using UnityEditor;
using SteelSurge.LevelEditor.Configs;

public static class ConfigUpdater7
{
    [MenuItem("Tools/Update Config Water")]
    public static void Update()
    {
        string configPath = "Assets/System/Configs/LevelEditor/ForestArenaConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<MapGenerationConfig>(configPath);
        if (config != null)
        {
            var so = new SerializedObject(config);
            
            // Use a basic hex for water, we will apply a blue material to it
            var waterHex = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetStore/Kaykat/fbx(unity)/tiles/hex_grass.fbx");
            
            // Create a simple blue material for water if it doesn't exist
            string matPath = "Assets/System/Materials/WaterMaterial.mat";
            Material waterMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (waterMat == null)
            {
                waterMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                waterMat.color = new Color(0.2f, 0.5f, 0.8f, 0.8f); // Transparent blue
                
                // Set URP material to transparent
                waterMat.SetFloat("_Surface", 1); // 1 = Transparent
                waterMat.SetFloat("_Blend", 0); // 0 = Alpha
                waterMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                waterMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                waterMat.SetInt("_ZWrite", 0);
                waterMat.DisableKeyword("_ALPHATEST_ON");
                waterMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                waterMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                
                if (!System.IO.Directory.Exists("Assets/System/Materials"))
                {
                    System.IO.Directory.CreateDirectory("Assets/System/Materials");
                }
                AssetDatabase.CreateAsset(waterMat, matPath);
            }
            
            so.FindProperty("_waterHexPrefab").objectReferenceValue = waterHex;
            so.FindProperty("_waterMaterial").objectReferenceValue = waterMat;
            
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("Config updated with water settings.");
        }
    }
}
