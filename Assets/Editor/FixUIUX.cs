using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FixUIUX
{
    public static void Execute()
    {
        // 1. Fix MapStatusPanel text
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

        // 2. Fix QuickActionPanel text
        var quickActionTitleObj = GameObject.Find("GameUI_Canvas/QuickActionPanel/QuickActionTitle");
        if (quickActionTitleObj != null)
        {
            var tmp = quickActionTitleObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = 14;
                tmp.enableAutoSizing = false;
                // Add outline or shadow for better readability
                tmp.fontStyle = FontStyles.Bold;
                // We can add a shadow component
                var shadow = quickActionTitleObj.GetComponent<Shadow>();
                if (shadow == null) shadow = quickActionTitleObj.AddComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.8f);
                shadow.effectDistance = new Vector2(1, -1);
            }
        }

        // 3. Fix Unit names visibility and size
        var unitCards = GameObject.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var rt in unitCards)
        {
            if (rt.name == "UnitName" && rt.parent.name.StartsWith("UnitCard_"))
            {
                var tmp = rt.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.enabled = true;
                    tmp.fontSize = 10;
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMin = 8;
                    tmp.fontSizeMax = 12;
                    tmp.alignment = TextAlignmentOptions.Center;
                    // Adjust position to be at the bottom of the card
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 0);
                    rt.pivot = new Vector2(0.5f, 0);
                    rt.anchoredPosition = new Vector2(0, 2);
                    rt.sizeDelta = new Vector2(0, 20);
                }
            }
            
            // 4. Fix Cost text size
            if (rt.name == "Text (TMP)" && rt.parent.name == "CostContainer")
            {
                var tmp = rt.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize = 12;
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMin = 10;
                    tmp.fontSizeMax = 14;
                }
            }
        }

        // 5. Fix EnergyCounterBg position and anchor
        var energyCounterBgObj = GameObject.Find("GameUI_Canvas/QuickActionPanel/EnergyCounterBg");
        if (energyCounterBgObj != null)
        {
            var rt = energyCounterBgObj.GetComponent<RectTransform>();
            // Move it to GameUI_Canvas
            var canvas = GameObject.Find("GameUI_Canvas").transform;
            rt.SetParent(canvas, true);
            
            // Anchor to top left
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(20, -20);
            
            // Fix EnergyText size
            var energyTextObj = rt.Find("EnergyText");
            if (energyTextObj != null)
            {
                var tmp = energyTextObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize = 18;
                    tmp.enableAutoSizing = false;
                }
            }
        }

        // 6. Fix Tabs text size
        string[] tabNames = { "Tab_Melee", "Tab_Ranged", "Tab_Magic", "Tab_Siege" };
        foreach (var tabName in tabNames)
        {
            var tabObj = GameObject.Find($"GameUI_Canvas/UnitSummonTabsPanel/TabsContainer/{tabName}");
            if (tabObj != null)
            {
                var textObj = tabObj.transform.GetChild(0); // Assuming Text is the first child
                if (textObj != null)
                {
                    var tmp = textObj.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.fontSize = 12;
                        tmp.enableAutoSizing = true;
                        tmp.fontSizeMin = 10;
                        tmp.fontSizeMax = 14;
                    }
                }
            }
        }

        // 7. Fix QuickActionPanel anchor to bottom right
        var quickActionPanelObj = GameObject.Find("GameUI_Canvas/QuickActionPanel");
        if (quickActionPanelObj != null)
        {
            var rt = quickActionPanelObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
            rt.anchoredPosition = new Vector2(-20, 20);
        }

        // 8. Fix MapStatusPanel anchor to top center
        var mapStatusPanelObj = GameObject.Find("GameUI_Canvas/MapStatusPanel");
        if (mapStatusPanelObj != null)
        {
            var rt = mapStatusPanelObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -20);
        }

        Debug.Log("UI UX fixed.");
    }
}