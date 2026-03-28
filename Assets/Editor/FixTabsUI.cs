using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class FixTabsUI
{
    public static void Execute()
    {
        GameObject panelObj = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel");
        if (panelObj == null) { Debug.LogError("UnitSummonTabsPanel not found"); return; }

        // 1. Сдвигаем панель выше, чтобы не перекрывала центральный счетчик
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchoredPosition = new Vector2(20, 100); // Подняли с 20 до 100 по Y

        // 2. Исправляем выравнивание кнопок на страницах
        GameObject pagesContObj = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer");
        if (pagesContObj != null)
        {
            foreach (Transform page in pagesContObj.transform)
            {
                HorizontalLayoutGroup layout = page.GetComponent<HorizontalLayoutGroup>();
                if (layout != null)
                {
                    layout.childAlignment = TextAnchor.UpperLeft; // Выравнивание по верхнему левому краю
                    layout.childControlWidth = false;
                    layout.childControlHeight = false;
                    layout.childForceExpandWidth = false; // Отключаем растягивание
                    layout.childForceExpandHeight = false;
                    layout.spacing = 10;
                }
            }
        }

        // 3. Исправляем выравнивание вкладок (Tabs)
        GameObject tabsContObj = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/TabsContainer");
        if (tabsContObj != null)
        {
            HorizontalLayoutGroup tabsLayout = tabsContObj.GetComponent<HorizontalLayoutGroup>();
            if (tabsLayout != null)
            {
                tabsLayout.childAlignment = TextAnchor.LowerLeft;
                tabsLayout.childForceExpandWidth = false;
                tabsLayout.childForceExpandHeight = false;
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
}