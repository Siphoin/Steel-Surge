using UnityEditor;
using UnityEngine;
using TMPro;

public class FixFontSizesV2
{
    public static void Execute()
    {
        // 1. Map Status Panel
        var mapStatusTextObj = GameObject.Find("GameUI_Canvas/MapStatusPanel/Text ");
        if (mapStatusTextObj != null)
        {
            var tmp = mapStatusTextObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = 14;
                tmp.enableAutoSizing = false;
            }
        }

        // 2. Quick Action Panel Title
        var quickActionTitleObj = GameObject.Find("GameUI_Canvas/QuickActionPanel/QuickActionTitle");
        if (quickActionTitleObj != null)
        {
            var tmp = quickActionTitleObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = 14;
                tmp.enableAutoSizing = false;
            }
        }

        // 3. Resource Text
        var resourceTextObj = GameObject.Find("GameUI_Canvas/QuickActionPanel/ResourceBarBg/ResourceText");
        if (resourceTextObj != null)
        {
            var tmp = resourceTextObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = 10;
                tmp.enableAutoSizing = false;
            }
        }

        // 4. Energy Text
        var energyTextObj = GameObject.Find("GameUI_Canvas/EnergyCounterBg/EnergyText");
        if (energyTextObj == null) energyTextObj = GameObject.Find("GameUI_Canvas/QuickActionPanel/EnergyCounterBg/EnergyText");
        if (energyTextObj != null)
        {
            var tmp = energyTextObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = 16;
                tmp.enableAutoSizing = false;
            }
        }

        // 5. Tabs Text
        string[] tabNames = { "Tab_Melee", "Tab_Ranged", "Tab_Magic", "Tab_Siege" };
        foreach (var tabName in tabNames)
        {
            var tabObj = GameObject.Find($"GameUI_Canvas/UnitSummonTabsPanel/TabsContainer/{tabName}");
            if (tabObj != null)
            {
                var textObj = tabObj.transform.Find("Text") ?? tabObj.transform.Find("Text_1") ?? tabObj.transform.Find("Text ");
                if (textObj != null)
                {
                    var tmp = textObj.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.fontSize = 12;
                        tmp.enableAutoSizing = false;
                    }
                }
            }
        }

        // 6. Unit Cards Text
        var unitCards = GameObject.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var rt in unitCards)
        {
            if (rt.name == "UnitName" && rt.parent.name.StartsWith("UnitCard_"))
            {
                var tmp = rt.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize = 10;
                    tmp.enableAutoSizing = false;
                }
            }
            
            if (rt.name == "Text (TMP)" && rt.parent.name == "CostContainer")
            {
                var tmp = rt.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize = 10;
                    tmp.enableAutoSizing = false;
                }
            }
        }

        Debug.Log("Font sizes updated V2.");
    }
}