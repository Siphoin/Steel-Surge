using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;

public class CreateUI
{
    [MenuItem("Tools/Create Game UI")]
    public static void Execute()
    {
        string jsonPath = "Assets/_UI_COPLAY_GENERATED/GameUI/config.json";
        if (!File.Exists(jsonPath))
        {
            Debug.LogError("JSON file not found at " + jsonPath);
            return;
        }

        string jsonContent = File.ReadAllText(jsonPath);
        JObject root = JObject.Parse(jsonContent);

        // Find or create Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        GameObject canvasGO;
        if (canvas == null)
        {
            canvasGO = new GameObject("GameUI_Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1024, 559);
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasGO = canvas.gameObject;
            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1024, 559);
            }
        }

        // Clear existing children
        for (int i = canvasGO.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(canvasGO.transform.GetChild(i).gameObject);
        }

        // Create root element
        CreateElement(root, canvasGO.transform, Vector2.zero);

        // Save as prefab
        string prefabPath = "Assets/_UI_COPLAY_GENERATED/GameUI/GameUI_Canvas.prefab";
        Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));
        PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, prefabPath, InteractionMode.UserAction);

        Debug.Log("UI created successfully.");
    }

    private static void CreateElement(JToken token, Transform parent, Vector2 parentAbsolutePosition)
    {
        string name = token["name"]?.ToString() ?? "Element";
        
        // Skip creating a new GameObject for the root Canvas itself, just process its children
        if (name == "Canvas" && parent.GetComponent<Canvas>() != null)
        {
            JArray rootChildren = token["children"] as JArray;
            if (rootChildren != null)
            {
                foreach (JToken child in rootChildren)
                {
                    CreateElement(child, parent, Vector2.zero);
                }
            }
            return;
        }

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        
        // Set anchor to center
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 absolutePosition = Vector2.zero;
        if (token["position"] != null)
        {
            float x = token["position"]["x"].Value<float>();
            float y = token["position"]["y"].Value<float>();
            absolutePosition = new Vector2(x, y);
            rt.anchoredPosition = absolutePosition - parentAbsolutePosition;
        }

        if (token["size"] != null)
        {
            float width = token["size"]["width"].Value<float>();
            float height = token["size"]["height"].Value<float>();
            rt.sizeDelta = new Vector2(width, height);
        }

        string type = token["type"]?.ToString();
        if (type == "Image" || type == "Button")
        {
            Image img = go.AddComponent<Image>();
            string bgType = token["backgroundType"]?.ToString();
            
            if (bgType != "None")
            {
                string spritePath = $"Assets/_UI_COPLAY_GENERATED/GameUI/Sprites/{name}.png";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprite != null)
                {
                    img.sprite = sprite;
                    if (bgType == "Sliced")
                    {
                        img.type = Image.Type.Sliced;
                    }
                    else
                    {
                        img.type = Image.Type.Simple;
                    }
                }
                else
                {
                    // If sprite not found, set a default color
                    img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                }
            }
            else
            {
                img.color = new Color(0, 0, 0, 0); // Transparent
            }

            if (type == "Button")
            {
                go.AddComponent<Button>();
            }
        }
        else if (type == "Text")
        {
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = token["text"]?.ToString() ?? "";
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.fontSize = 14;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = 24;
            text.fontStyle = FontStyles.Bold;
        }

        // Process children
        JArray children = token["children"] as JArray;
        if (children != null)
        {
            foreach (JToken child in children)
            {
                CreateElement(child, go.transform, absolutePosition);
            }
        }
    }
}