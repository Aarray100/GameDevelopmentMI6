using UnityEngine;
using UnityEngine.UI;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance;

    [Header("UI References")]
    public Transform buffContainer; // Das Eltern-Objekt (z.B. rechts am Rand)
    public GameObject buffIconPrefab; // Das Prefab von Schritt 1

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddBuff(Sprite icon, float duration)
    {
        if (buffContainer == null || buffIconPrefab == null) return;

        GameObject newBuff = Instantiate(buffIconPrefab, buffContainer);
        BuffIcon script = newBuff.GetComponent<BuffIcon>();
        
        if (script != null)
        {
            script.Initialize(icon, duration);
        }
    }
}