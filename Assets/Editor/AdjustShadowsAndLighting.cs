using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AdjustShadowsAndLighting
{
    public static void Execute()
    {
        // 1. Настройка Directional Light
        Light dirLight = GameObject.Find("Directional Light")?.GetComponent<Light>();
        if (dirLight != null)
        {
            dirLight.shadowStrength = 0.5f; // Делаем тени от солнца светлее (было 1.0)
            dirLight.color = new Color(1f, 0.95f, 0.9f); // Чуть теплее свет
            dirLight.intensity = 1.1f; // Чуть ярче
            Debug.Log("Directional Light shadows softened.");
        }

        // 2. Настройка Ambient Lighting (Окружающий свет)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.4f, 0.45f, 0.5f); // Делаем тени синеватыми и светлыми
        Debug.Log("Ambient Light adjusted.");

        // 3. Настройка SSAO в рендерерах (чтобы черные пятна в углах стали мягче)
        string[] rendererPaths = { "Assets/URP/PC_Renderer.asset", "Assets/URP/Mobile_Renderer.asset" };
        foreach (var path in rendererPaths)
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(path);
            if (rendererData != null)
            {
                foreach (var feature in rendererData.rendererFeatures)
                {
                    if (feature != null && feature.GetType().Name.Contains("ScreenSpaceAmbientOcclusion"))
                    {
                        // Используем рефлексию, так как настройки SSAO скрыты в URP
                        var settingsField = feature.GetType().GetField("m_Settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (settingsField != null)
                        {
                            var settings = settingsField.GetValue(feature);
                            
                            var intensityField = settings.GetType().GetField("Intensity");
                            if (intensityField != null) intensityField.SetValue(settings, 0.5f); // Уменьшаем интенсивность SSAO

                            var radiusField = settings.GetType().GetField("Radius");
                            if (radiusField != null) radiusField.SetValue(settings, 0.3f); // Уменьшаем радиус

                            settingsField.SetValue(feature, settings);
                            EditorUtility.SetDirty(rendererData);
                            Debug.Log("SSAO intensity reduced on " + path);
                        }
                    }
                }
            }
        }

        // 4. Убираем лишний контраст из Volume
        string profilePath = "Assets/URP/SampleSceneProfile.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (profile != null)
        {
            if (profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                colorAdjustments.contrast.Override(0f); // Убираем искусственный контраст
                colorAdjustments.postExposure.Override(0.1f); // Чуть светлее
            }
            EditorUtility.SetDirty(profile);
        }

        AssetDatabase.SaveAssets();
    }
}