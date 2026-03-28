using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FixMask
{
    public static void Execute()
    {
        var pagesContainer = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer");
        if (pagesContainer != null)
        {
            var rectMask = pagesContainer.GetComponent<RectMask2D>();
            if (rectMask == null)
            {
                pagesContainer.AddComponent<RectMask2D>();
                Debug.Log("Added RectMask2D to PagesContainer");
            }
        }
    }
}