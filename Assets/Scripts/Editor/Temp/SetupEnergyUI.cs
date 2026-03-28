using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetupEnergyUI
{
    [MenuItem("Tools/Setup Energy UI")]
    public static void Execute()
    {
        // 1. Setup Sprite
        string path = "Assets/Sprites/Icons/UI/EnergyLightning.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        // 2. Update UI
        GameObject canvasGO = GameObject.Find("GameUI_Canvas");
        if (canvasGO == null)
        {
            Debug.LogError("GameUI_Canvas not found in scene.");
            return;
        }

        Transform quickActionPanel = canvasGO.transform.Find("QuickActionPanel");
        if (quickActionPanel != null)
        {
            // Create Energy Counter Background
            GameObject energyBgObj = new GameObject("EnergyCounterBg");
            energyBgObj.transform.SetParent(quickActionPanel, false);
            RectTransform bgRt = energyBgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0);
            bgRt.anchorMax = new Vector2(0, 0);
            bgRt.pivot = new Vector2(0, 0);
            bgRt.anchoredPosition = new Vector2(-20, -20); // Position relative to bottom-left of QuickActionPanel
            bgRt.sizeDelta = new Vector2(80, 30);
            Image bgImg = energyBgObj.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent background

            // Create Lightning Icon
            GameObject iconObj = new GameObject("LightningIcon");
            iconObj.transform.SetParent(energyBgObj.transform, false);
            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0.5f);
            iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.pivot = new Vector2(0, 0.5f);
            iconRt.anchoredPosition = new Vector2(5, 0);
            iconRt.sizeDelta = new Vector2(20, 20);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            // Create Energy Text
            GameObject textObj = new GameObject("EnergyText");
            textObj.transform.SetParent(energyBgObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0);
            textRt.anchorMax = new Vector2(1, 1);
            textRt.offsetMin = new Vector2(30, 0); // Offset to make room for icon
            textRt.offsetMax = new Vector2(-5, 0);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "10"; // Default value
            text.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
            text.color = Color.white;
            text.fontSize = 16;
            text.fontStyle = FontStyles.Bold;

            // Apply to prefab
            string prefabPath = "Assets/_UI_COPLAY_GENERATED/GameUI/GameUI_Canvas.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, prefabPath, InteractionMode.UserAction);
            
            Debug.Log("Energy UI updated successfully.");
        }
        else
        {
            Debug.LogError("QuickActionPanel not found.");
        }
    }
}