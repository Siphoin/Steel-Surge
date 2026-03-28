using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class InspectLayout
{
    public static void Execute()
    {
        StringBuilder sb = new StringBuilder();
        var panel = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel");
        if (panel != null)
        {
            sb.AppendLine($"Panel: {panel.GetComponent<RectTransform>().rect}");
            
            var pagesContainer = panel.transform.Find("PagesContainer");
            if (pagesContainer != null)
            {
                sb.AppendLine($"PagesContainer: {pagesContainer.GetComponent<RectTransform>().rect}");
                
                var pageMelee = pagesContainer.Find("Page_Melee");
                if (pageMelee != null)
                {
                    sb.AppendLine($"Page_Melee: {pageMelee.GetComponent<RectTransform>().rect}");
                    var scrollRect = pageMelee.GetComponent<ScrollRect>();
                    sb.AppendLine($"ScrollRect: {(scrollRect != null ? "Yes" : "No")}");
                    
                    var content = pageMelee.Find("Content");
                    if (content != null)
                    {
                        sb.AppendLine($"Content: {content.GetComponent<RectTransform>().rect}");
                        var hlg = content.GetComponent<HorizontalLayoutGroup>();
                        if (hlg != null)
                        {
                            sb.AppendLine($"HLG: spacing={hlg.spacing}, padding={hlg.padding}");
                        }
                        var glg = content.GetComponent<GridLayoutGroup>();
                        if (glg != null)
                        {
                            sb.AppendLine($"GLG: cellSize={glg.cellSize}, spacing={glg.spacing}");
                        }
                        
                        foreach (Transform child in content)
                        {
                            sb.AppendLine($"Card {child.name}: {child.GetComponent<RectTransform>().rect}");
                            break; // just one is enough
                        }
                    }
                }
            }
        }
        Debug.Log(sb.ToString());
    }
}