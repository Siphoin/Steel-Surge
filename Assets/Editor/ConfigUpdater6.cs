using UnityEngine;
using UnityEditor;
using SteelSurge.LevelEditor.Configs;

public static class ConfigUpdater6
{
    [MenuItem("Tools/Update Config Rivers")]
    public static void Update()
    {
        string configPath = "Assets/System/Configs/LevelEditor/ForestArenaConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<MapGenerationConfig>(configPath);
        if (config != null)
        {
            var so = new SerializedObject(config);
            
            var straight = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetStore/Kaykat/fbx(unity)/tiles/rivers/hex_river_A.fbx");
            var curved = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetStore/Kaykat/fbx(unity)/tiles/rivers/hex_river_B.fbx");
            var crossing = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetStore/Kaykat/fbx(unity)/tiles/rivers/hex_river_crossing_A.fbx");
            
            so.FindProperty("_riverStraightPrefab").objectReferenceValue = straight;
            so.FindProperty("_riverCurvedPrefab").objectReferenceValue = curved;
            so.FindProperty("_riverCrossingPrefab").objectReferenceValue = crossing;
            
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("Config updated with river prefabs.");
        }
    }
}
