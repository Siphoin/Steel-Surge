using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class UpdateCardBgV3
{
    public static void Execute()
    {
        string prefabPath = "Assets/Prefabs/UI/UnitCard.prefab";
        string spritePath = "Assets/Sprites/UI/UnitCardBg_v3.png";
        
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (bgSprite == null)
        {
            Debug.LogError("New background sprite not found!");
            return;
        }

        // Обновляем префаб
        GameObject contentsRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        Image bgImage = contentsRoot.GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.sprite = bgSprite;
            bgImage.color = Color.white;
        }
        PrefabUtility.SaveAsPrefabAsset(contentsRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(contentsRoot);

        // Обновляем экземпляры на сцене
        GameObject pageMelee = GameObject.Find("GameUI_Canvas/UnitSummonTabsPanel/PagesContainer/Page_Melee");
        if (pageMelee != null)
        {
            foreach (Transform child in pageMelee.transform)
            {
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = bgSprite;
                    img.color = Color.white;
                }
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        Debug.Log("New background applied.");
    }
}