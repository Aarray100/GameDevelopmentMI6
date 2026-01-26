using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JournalToast : MonoBehaviour
{
    public static JournalToast Instance;

    [SerializeField] CanvasGroup group;
    [SerializeField] TMP_Text text;
    [SerializeField] float showSeconds = 5f;

    readonly Queue<string> queue = new();
    bool running;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    public static void Enqueue(string msg)
    {
        if (Instance == null) return;
        Instance.queue.Enqueue(msg);
        if (!Instance.running) Instance.StartCoroutine(Instance.Run());
    }

    IEnumerator Run()
    {
        running = true;
        while (queue.Count > 0)
        {
            text.text = queue.Dequeue();
            group.alpha = 1f;
            yield return new WaitForSeconds(showSeconds);
            group.alpha = 0f;
            yield return new WaitForSeconds(0.1f);
        }
        running = false;
    }
}
