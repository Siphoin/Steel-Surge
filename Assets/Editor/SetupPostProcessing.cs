using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class SetupPostProcessing
{
    public static void Execute()
    {
        // 1. Setup Volume Profile
        string profilePath = "Assets/URP/SampleSceneProfile.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);

        if (profile != null)
        {
            // Bloom
            if (!profile.TryGet(out Bloom bloom)) bloom = profile.Add<Bloom>(false);
            bloom.active = true;
            bloom.intensity.Override(0.6f);
            bloom.threshold.Override(0.85f);
            bloom.scatter.Override(0.7f);

            // Color Adjustments
            if (!profile.TryGet(out ColorAdjustments colorAdjustments)) colorAdjustments = profile.Add<ColorAdjustments>(false);
            colorAdjustments.active = true;
            colorAdjustments.postExposure.Override(0.15f);
            colorAdjustments.contrast.Override(15f);
            colorAdjustments.saturation.Override(15f);

            // White Balance
            if (!profile.TryGet(out WhiteBalance whiteBalance)) whiteBalance = profile.Add<WhiteBalance>(false);
            whiteBalance.active = true;
            whiteBalance.temperature.Override(15f); // Warmer
            whiteBalance.tint.Override(2f);

            // Tonemapping
            if (!profile.TryGet(out Tonemapping tonemapping)) tonemapping = profile.Add<Tonemapping>(false);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);

            // Vignette
            if (!profile.TryGet(out Vignette vignette)) vignette = profile.Add<Vignette>(false);
            vignette.active = true;
            vignette.intensity.Override(0.25f);
            vignette.smoothness.Override(0.2f);

            EditorUtility.SetDirty(profile);
            Debug.Log("Volume Profile updated.");
        }
        else
        {
            Debug.LogError("Could not find VolumeProfile at " + profilePath);
        }

        // 2. Setup SSAO on Renderers
        string[] rendererPaths = { "Assets/URP/PC_Renderer.asset", "Assets/URP/Mobile_Renderer.asset" };
        foreach (var path in rendererPaths)
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(path);
            if (rendererData != null)
            {
                bool hasSSAO = rendererData.rendererFeatures.Any(f => f != null && f.GetType().Name.Contains("ScreenSpaceAmbientOcclusion"));
                
                if (!hasSSAO)
                {
                    var ssaoFeature = ScriptableObject.CreateInstance("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion") as ScriptableRendererFeature;
                    if (ssaoFeature != null)
                    {
                        ssaoFeature.name = "SSAO";
                        rendererData.rendererFeatures.Add(ssaoFeature);
                        AssetDatabase.AddObjectToAsset(ssaoFeature, rendererData);
                        Debug.Log("Added SSAO to " + path);
                    }
                    else
                    {
                        Debug.LogWarning("Could not create SSAO feature instance.");
                    }
                }
                else
                {
                    var ssaoFeature = rendererData.rendererFeatures.First(f => f != null && f.GetType().Name.Contains("ScreenSpaceAmbientOcclusion"));
                    ssaoFeature.SetActive(true);
                    Debug.Log("Enabled existing SSAO on " + path);
                }
                EditorUtility.SetDirty(rendererData);
            }
        }
        
        AssetDatabase.SaveAssets();
    }
}