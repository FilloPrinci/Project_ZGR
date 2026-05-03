using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIListManager : MonoBehaviour
{
    public Transform contentParent;
    public GameObject rowPrefab;

    public Color defaultBgColor;
    public Color accentBgColor;


    public void AddRow(List<string> columns, Color? bgColor = null)
    {
        if (bgColor == null)
        {
            bgColor = defaultBgColor;
        }

        GameObject row = Instantiate(rowPrefab, contentParent);

        Image bgImage = row.GetComponent<Image>();

        bgImage.color = bgColor.Value;

        TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();

        int count = Mathf.Min(columns.Count, texts.Length);

        Debug.Log($"Adding row with {count} columns.");

        for (int i = 0; i < count; i++)
        {
            texts[i].text = columns[i];
        }
    }
}