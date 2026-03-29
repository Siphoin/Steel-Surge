using UnityEngine;
using UnityEditor;
using SteelSurge.LevelEditor.Configs;

public static class ConfigUpdater8
{
    [MenuItem("Tools/Update Config Water Prefabs")]
    public static void Update()
    {
        string configPath = "Assets/System/Configs/LevelEditor/ForestArenaConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<MapGenerationConfig>(configPath);
        if (config != null)
        {
            var so = new SerializedObject(config);
            
            var waterSmall = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/water_small.prefab");
            var waterBig = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/water_big.prefab");
            
            so.FindProperty("_waterSmallPrefab").objectReferenceValue = waterSmall;
            so.FindProperty("_waterBigPrefab").objectReferenceValue = waterBig;
            
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("Config updated with water_small and water_big prefabs.");
        }
    }
}
