using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CheckMaskable
{
    public static void Execute()
    {
        var content = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer/Page_Melee/Content");
        if (content != null)
        {
            foreach (Transform child in content.transform)
            {
                var img = child.GetComponent<Image>();
                if (img != null)
                {
                    Debug.Log($"Card {child.name} Image maskable: {img.maskable}");
                }
                break;
            }
        }
    }
}