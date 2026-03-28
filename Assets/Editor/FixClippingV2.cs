using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FixClippingV2
{
    public static void Execute()
    {
        var pagesContainer = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer");
        if (pagesContainer != null)
        {
            var rt = pagesContainer.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(5, 5); // Left, Bottom
            rt.offsetMax = new Vector2(-5, -30); // Right, Top (leave space for tabs)
        }

        var pageMelee = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer/Page_Melee");
        if (pageMelee != null)
        {
            var rt = pageMelee.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
        }
        
        var content = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer/Page_Melee/Content");
        if (content != null)
        {
            var rt = content.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 1); // Anchor to left
            rt.pivot = new Vector2(0, 0.5f);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(rt.sizeDelta.x, 0); // Keep width
            
            var hlg = content.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.padding = new RectOffset(5, 5, 5, 5);
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlHeight = true;
                hlg.childControlWidth = false;
                hlg.childForceExpandHeight = true;
                hlg.childForceExpandWidth = false;
            }
        }
        
        // Fix card sizes
        var unitCards = GameObject.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var rt in unitCards)
        {
            if (rt.name.StartsWith("UnitCard_") && rt.parent != null && rt.parent.name == "Content")
            {
                var le = rt.GetComponent<LayoutElement>();
                if (le == null) le = rt.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = 65;
                le.preferredHeight = 80;
                le.minWidth = 65;
                le.minHeight = 80;
            }
        }
    }
}