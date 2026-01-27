using UnityEngine;

public class JournalOverlay : MonoBehaviour
{
    [Header("Prefab Spawn (Option B)")]
    [SerializeField] GameObject journalCanvasPrefab; // JournalCanvas.prefab hier reinziehen
    [SerializeField] string panelName = "JournalRoot"; // muss exakt so heißen im Prefab

    [Header("Optional")]
    [SerializeField] PlayerMovement2D movement;

    static GameObject uiInstance;  // verhindert doppelte Instanz
    GameObject panel;
    
    // Static property um zu prüfen ob Journal offen ist
    public static bool IsOpen { get; private set; }

    void Awake()
    {
        if (uiInstance == null)
        {
            if (journalCanvasPrefab == null)
            {
                Debug.LogError("JournalOverlay: journalCanvasPrefab ist nicht zugewiesen!");
                return;
            }

            uiInstance = Instantiate(journalCanvasPrefab);
            DontDestroyOnLoad(uiInstance);
        }

        panel = uiInstance.transform.Find(panelName)?.gameObject;

        if (panel == null)
            Debug.LogError($"JournalOverlay: Panel '{panelName}' nicht gefunden (Name prüfen).");
        else
            panel.SetActive(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.J)) return; // in Update abfragen [web:329]
        if (panel == null) return;

        bool open = !panel.activeSelf;
        panel.SetActive(open);
        IsOpen = open;

        // Movement bleibt erlaubt, nur Angriffe werden blockiert (via PlayerCombat)
        // if (movement != null)
        // {
        //     movement.movementLocked = open;
        //     movement.ForceStop();
        // }
    }
}
