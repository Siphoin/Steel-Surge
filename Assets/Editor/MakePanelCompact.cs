using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class MakePanelCompact
{
    public static void Execute()
    {
        GameObject panelObj = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel");
        if (panelObj == null) return;

        // 1. Уменьшаем главную подложку
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.sizeDelta = new Vector2(320, 120); // Сделали уже и ниже
        panelRt.anchoredPosition = new Vector2(10, 10); // Прижали к левому нижнему углу

        // 2. Настраиваем контейнер вкладок (Tabs)
        GameObject tabsContObj = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/TabsContainer");
        if (tabsContObj != null)
        {
            RectTransform tabsRt = tabsContObj.GetComponent<RectTransform>();
            tabsRt.sizeDelta = new Vector2(0, 30); // Уменьшили высоту строки вкладок
            tabsRt.offsetMin = new Vector2(5, 0); // Уменьшили отступ слева

            HorizontalLayoutGroup tabsLayout = tabsContObj.GetComponent<HorizontalLayoutGroup>();
            if (tabsLayout != null)
            {
                tabsLayout.spacing = 2; // Уменьшили расстояние между вкладками
            }

            // Уменьшаем сами вкладки
            foreach (Transform tab in tabsContObj.transform)
            {
                RectTransform tabRt = tab.GetComponent<RectTransform>();
                tabRt.sizeDelta = new Vector2(75, 30); // Сделали вкладки меньше
                
                // Уменьшаем шрифт во вкладках
                TMPro.TextMeshProUGUI text = tab.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null) text.fontSize = 12;
            }
        }

        // 3. Настраиваем контейнер страниц (с карточками)
        GameObject pagesContObj = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer");
        if (pagesContObj != null)
        {
            RectTransform pagesRt = pagesContObj.GetComponent<RectTransform>();
            pagesRt.offsetMin = new Vector2(5, 5); // Уменьшили отступы снизу и слева
            pagesRt.offsetMax = new Vector2(-5, -35); // Уменьшили отступ сверху (под вкладки)

            // Настраиваем LayoutGroup на страницах
            foreach (Transform page in pagesContObj.transform)
            {
                HorizontalLayoutGroup pageLayout = page.GetComponent<HorizontalLayoutGroup>();
                if (pageLayout != null)
                {
                    pageLayout.spacing = 5; // Уменьшили расстояние между карточками
                    pageLayout.padding = new RectOffset(0, 0, 5, 0); // Уменьшили отступ сверху
                }
            }
        }

        // 4. Уменьшаем размер самих карточек (префабов на сцене)
        GameObject pageMelee = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer/Page_Melee");
        if (pageMelee != null)
        {
            foreach (Transform card in pageMelee.transform)
            {
                RectTransform cardRt = card.GetComponent<RectTransform>();
                cardRt.sizeDelta = new Vector2(65, 80); // Сделали карточки меньше (было 80x100)
                
                // Корректируем иконку внутри карточки
                Transform icon = card.Find("UnitIcon");
                if (icon != null)
                {
                    RectTransform iconRt = icon.GetComponent<RectTransform>();
                    iconRt.sizeDelta = new Vector2(45, 45);
                    iconRt.anchoredPosition = new Vector2(0, 8);
                }

                // Корректируем текст названия
                Transform name = card.Find("UnitName");
                if (name != null)
                {
                    TMPro.TextMeshProUGUI nameText = name.GetComponent<TMPro.TextMeshProUGUI>();
                    if (nameText != null) nameText.fontSize = 10;
                    RectTransform nameRt = name.GetComponent<RectTransform>();
                    nameRt.anchoredPosition = new Vector2(0, 2);
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
}