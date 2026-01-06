using UnityEngine;
using TMPro;

public class NotificationItem : MonoBehaviour
{
    [SerializeField] TMP_Text textMesh;
    [SerializeField] float duration = 3f;

    public void SetText(string message)
    {
        if (textMesh != null) textMesh.text = message;
        // Zerstört das Objekt nach X Sekunden automatisch
        Destroy(gameObject, duration);
    }
}