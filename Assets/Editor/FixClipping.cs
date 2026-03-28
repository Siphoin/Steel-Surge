using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FixClipping
{
    public static void Execute()
    {
        var pagesContainer = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer");
        if (pagesContainer != null)
        {
            var rt = pagesContainer.GetComponent<RectTransform>();
            // Increase height to prevent clipping
            rt.offsetMin = new Vector2(rt.offsetMin.x, 5); // Bottom offset
            rt.offsetMax = new Vector2(rt.offsetMax.x, -25); // Top offset (leave space for tabs)
            
            Debug.Log($"PagesContainer new rect: {rt.rect}");
        }

        var pageMelee = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer/Page_Melee");
        if (pageMelee != null)
        {
            var rt = pageMelee.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            
            Debug.Log($"Page_Melee new rect: {rt.rect}");
        }
        
        var content = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer/Page_Melee/Content");
        if (content != null)
        {
            var rt = content.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            
            var hlg = content.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.padding = new RectOffset(5, 5, 5, 5);
                hlg.childAlignment = TextAnchor.MiddleCenter;
            }
            
            Debug.Log($"Content new rect: {rt.rect}");
        }
    }
}