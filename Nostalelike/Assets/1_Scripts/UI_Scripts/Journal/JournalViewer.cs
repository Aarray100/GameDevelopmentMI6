using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JournalViewer : MonoBehaviour
{
    [SerializeField] TMP_Text leftText;
    [SerializeField] TMP_Text rightText;

    [TextArea(3, 10)] public List<string> pages = new(); // dummy content

    int leftPageIndex = 0;

    void OnEnable() => Refresh();

    public void Next()
    {
        leftPageIndex = Mathf.Min(leftPageIndex + 2, Mathf.Max(0, pages.Count - 1));
        Refresh();
    }

    public void Prev()
    {
        leftPageIndex = Mathf.Max(0, leftPageIndex - 2);
        Refresh();
    }

    public void Refresh()
    {
        leftText.text  = GetPageOrEmpty(leftPageIndex);
        rightText.text = GetPageOrEmpty(leftPageIndex + 1);
    }

    string GetPageOrEmpty(int index)
    {
        if (index < 0 || index >= pages.Count) return "";   // “leer wenn nicht vorhanden”
        return pages[index];
    }
}
