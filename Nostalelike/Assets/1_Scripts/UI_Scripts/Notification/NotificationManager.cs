using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [SerializeField] GameObject notificationPrefab;
    [SerializeField] Transform container; // Der Ort mit der Vertical Layout Group

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowNotification(string message)
    {
        if (notificationPrefab == null || container == null) return;

        GameObject go = Instantiate(notificationPrefab, container);
        NotificationItem item = go.GetComponent<NotificationItem>();
        if (item != null) item.SetText(message);
    }
}