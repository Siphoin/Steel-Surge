using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UpdateUIBar
{
    [MenuItem("Tools/Update UI Bar")]
    public static void Execute()
    {
        GameObject canvasGO = GameObject.Find("GameUI_Canvas");
        if (canvasGO == null)
        {
            Debug.LogError("GameUI_Canvas not found in scene.");
            return;
        }

        Transform mapStatusPanel = canvasGO.transform.Find("MapStatusPanel");
        if (mapStatusPanel != null)
        {
            Transform oldText = mapStatusPanel.Find("PointsControlledText");
            if (oldText != null)
            {
                Object.DestroyImmediate(oldText.gameObject);
            }

            Transform oldBg = mapStatusPanel.Find("ConfrontationBarBg");
            if (oldBg != null)
            {
                Object.DestroyImmediate(oldBg.gameObject);
            }

            // Create Background
            GameObject bgObj = new GameObject("ConfrontationBarBg");
            bgObj.transform.SetParent(mapStatusPanel, false);
            RectTransform bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0.5f, 0.5f);
            bgRt.anchorMax = new Vector2(0.5f, 0.5f);
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.anchoredPosition = new Vector2(0, -10);
            bgRt.sizeDelta = new Vector2(320, 15);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Create Blue Fill
            GameObject blueObj = new GameObject("BlueFill");
            blueObj.transform.SetParent(bgObj.transform, false);
            RectTransform blueRt = blueObj.AddComponent<RectTransform>();
            blueRt.anchorMin = new Vector2(0, 0);
            blueRt.anchorMax = new Vector2(1, 1);
            blueRt.offsetMin = Vector2.zero;
            blueRt.offsetMax = Vector2.zero;
            Image blueImg = blueObj.AddComponent<Image>();
            blueImg.color = new Color(0.2f, 0.6f, 1f, 1f);
            blueImg.type = Image.Type.Filled;
            blueImg.fillMethod = Image.FillMethod.Horizontal;
            blueImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            blueImg.fillAmount = 0.66f;

            // Create Red Fill
            GameObject redObj = new GameObject("RedFill");
            redObj.transform.SetParent(bgObj.transform, false);
            RectTransform redRt = redObj.AddComponent<RectTransform>();
            redRt.anchorMin = new Vector2(0, 0);
            redRt.anchorMax = new Vector2(1, 1);
            redRt.offsetMin = Vector2.zero;
            redRt.offsetMax = Vector2.zero;
            Image redImg = redObj.AddComponent<Image>();
            redImg.color = new Color(1f, 0.2f, 0.2f, 1f);
            redImg.type = Image.Type.Filled;
            redImg.fillMethod = Image.FillMethod.Horizontal;
            redImg.fillOrigin = (int)Image.OriginHorizontal.Right;
            redImg.fillAmount = 0.33f;

            // Apply to prefab
            string prefabPath = "Assets/_UI_COPLAY_GENERATED/GameUI/GameUI_Canvas.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, prefabPath, InteractionMode.UserAction);
            
            Debug.Log("UI Bar updated successfully.");
        }
        else
        {
            Debug.LogError("MapStatusPanel not found.");
        }
    }
}