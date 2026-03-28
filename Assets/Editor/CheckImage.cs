using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CheckImage
{
    public static void Execute()
    {
        var pageMelee = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer/Page_Melee");
        if (pageMelee != null)
        {
            var img = pageMelee.GetComponent<Image>();
            Debug.Log($"Page_Melee Image: {img != null}, showMaskGraphic: {(img != null ? pageMelee.GetComponent<Mask>().showMaskGraphic.ToString() : "N/A")}");
        }
    }
}