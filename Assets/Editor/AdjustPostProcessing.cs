using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AdjustPostProcessing
{
    public static void Execute()
    {
        string profilePath = "Assets/URP/SampleSceneProfile.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);

        if (profile != null)
        {
            // Color Adjustments - убираем кислотность
            if (profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                colorAdjustments.postExposure.Override(0.05f); // Чуть светлее, но не пересвечено
                colorAdjustments.contrast.Override(5f); // Меньше контраста
                colorAdjustments.saturation.Override(5f); // Сильно убавляем насыщенность
            }

            // White Balance - делаем чуть теплее, но не желтым
            if (profile.TryGet(out WhiteBalance whiteBalance))
            {
                whiteBalance.temperature.Override(5f); // Легкая теплота
                whiteBalance.tint.Override(0f); // Убираем мадженту/зелень
            }

            // Bloom - делаем мягче
            if (profile.TryGet(out Bloom bloom))
            {
                bloom.intensity.Override(0.2f); // Меньше свечения
                bloom.threshold.Override(1.0f); // Светится только самое яркое
            }

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Debug.Log("Volume Profile adjusted to be less acidic.");
        }
        else
        {
            Debug.LogError("Could not find VolumeProfile at " + profilePath);
        }
    }
}