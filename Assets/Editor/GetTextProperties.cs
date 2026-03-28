using UnityEditor;
using UnityEngine;
using TMPro;
using System.Text;

public class GetTextProperties
{
    public static void Execute()
    {
        StringBuilder sb = new StringBuilder();
        var texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in texts)
        {
            sb.AppendLine($"{t.name} (in {t.transform.parent.name}): fontSize={t.fontSize}, autoSize={t.enableAutoSizing}, min={t.fontSizeMin}, max={t.fontSizeMax}, alignment={t.alignment}, rect={t.rectTransform.rect}");
        }
        Debug.Log(sb.ToString());
    }
}