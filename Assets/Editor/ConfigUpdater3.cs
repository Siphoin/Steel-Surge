using UnityEngine;
using UnityEditor;
using SteelSurge.LevelEditor.Configs;

public static class ConfigUpdater3
{
    [MenuItem("Tools/Update Config Tree Settings 3")]
    public static void Update()
    {
        string configPath = "Assets/System/Configs/LevelEditor/ForestArenaConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<MapGenerationConfig>(configPath);
        if (config != null)
        {
            var so = new SerializedObject(config);
            so.FindProperty("_treeDensity").floatValue = 0.15f; // Number of clusters
            so.FindProperty("_treeNoiseThreshold").floatValue = 0.6f; // Controls cluster radius (1 - 0.6 = 0.4 * 10 = radius 4)
            so.FindProperty("_treeClusterDensity").floatValue = 0.95f; // How dense the trees are inside the cluster
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("Config updated for Warcraft 3 style tree clusters.");
        }
    }
}
