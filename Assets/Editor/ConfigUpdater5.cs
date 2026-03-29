using UnityEngine;
using UnityEditor;
using SteelSurge.LevelEditor.Configs;

public static class ConfigUpdater5
{
    [MenuItem("Tools/Update Config Border Settings")]
    public static void Update()
    {
        string configPath = "Assets/System/Configs/LevelEditor/ForestArenaConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<MapGenerationConfig>(configPath);
        if (config != null)
        {
            var so = new SerializedObject(config);
            so.FindProperty("_maxBorderDepth").intValue = 3;
            so.FindProperty("_borderNoiseScale").floatValue = 0.2f;
            so.FindProperty("_borderNoiseThreshold").floatValue = 0.4f;
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("Config updated for jagged borders.");
        }
    }
}
