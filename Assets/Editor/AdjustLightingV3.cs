using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AdjustLightingV3
{
    public static void Execute()
    {
        // 1. Настройка Directional Light (Солнце)
        Light dirLight = GameObject.Find("Directional Light")?.GetComponent<Light>();
        if (dirLight != null)
        {
            dirLight.shadowStrength = 0.75f; // Возвращаем немного плотности теням (было 0.5)
            dirLight.color = new Color(1f, 0.98f, 0.95f); // Делаем свет чуть белее, чтобы цвета не искажались
            dirLight.intensity = 1.2f; // Делаем солнце ярче
            Debug.Log("Directional Light adjusted.");
        }

        // 2. Настройка Ambient Lighting (Окружающий свет)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.7f); // Делаем тени светлее, но не серыми
        Debug.Log("Ambient Light adjusted.");

        // 3. Настройка Volume Profile (Цвета и контраст)
        string profilePath = "Assets/URP/SampleSceneProfile.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (profile != null)
        {
            if (profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                colorAdjustments.contrast.Override(10f); // Возвращаем контраст, чтобы не было "мыла"
                colorAdjustments.saturation.Override(15f); // Делаем цвета сочнее
                colorAdjustments.postExposure.Override(0.2f); // Делаем картинку в целом светлее
            }
            
            if (profile.TryGet(out WhiteBalance whiteBalance))
            {
                whiteBalance.temperature.Override(10f); // Легкая теплота
            }
            
            EditorUtility.SetDirty(profile);
            Debug.Log("Volume Profile adjusted for better colors.");
        }

        AssetDatabase.SaveAssets();
    }
}