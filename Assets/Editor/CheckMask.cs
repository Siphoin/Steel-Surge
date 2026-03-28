using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CheckMask
{
    public static void Execute()
    {
        var pageMelee = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer/Page_Melee");
        if (pageMelee != null)
        {
            var mask = pageMelee.GetComponent<Mask>();
            var rectMask = pageMelee.GetComponent<RectMask2D>();
            Debug.Log($"Page_Melee Mask: {mask != null}, RectMask2D: {rectMask != null}");
        }
        
        var pagesContainer = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer");
        if (pagesContainer != null)
        {
            var mask = pagesContainer.GetComponent<Mask>();
            var rectMask = pagesContainer.GetComponent<RectMask2D>();
            Debug.Log($"PagesContainer Mask: {mask != null}, RectMask2D: {rectMask != null}");
        }
    }
}