using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class ButtonInteraction : MonoBehaviour
{
    [Header("Ziele & Action")]
    public Tilemap wallTilemap;
    public Vector3Int[] tilesToDelete;
    
    // --- HIER IST DAS NEUE FELD ---
    public GameObject objectToActivate;   // Hier kommt "BridgePath" rein (Brücke AN)
    public GameObject objectToDeactivate; // Hier kommt "Void_Bridge" rein (Tod AUS)
    // ------------------------------

    public Transform cameraTarget;      

    [Header("Sequenz-Einstellungen")]
    public float waitTimeAtTarget = 1.5f;
    public float slowMotionFactor = 0.05f;
    public float cameraSpeed = 5f; 

    [Header("UI & Grafik")]
    public Sprite pressedSprite;
    public GameObject uiPopup; 

    private bool isActivated = false;
    private bool isPlayerInRange = false;
    private Camera mainCam; 

    void Start()
    {
        mainCam = Camera.main;

        if(uiPopup != null) uiPopup.SetActive(false);

        // START-ZUSTAND:
        // 1. Die Brücke ist UNSICHTBAR
        if(objectToActivate != null) objectToActivate.SetActive(false);
        
        // 2. Das Void (die Barriere) ist SICHTBAR/AKTIV (damit man nicht reinfällt)
        if(objectToDeactivate != null) objectToDeactivate.SetActive(true);
    }

    void Update()
    {
        if (isPlayerInRange && !isActivated && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(CutsceneRoutine());
        }
    }

    IEnumerator CutsceneRoutine()
    {
        isActivated = true;

        if(uiPopup != null) uiPopup.SetActive(false);
        if (pressedSprite != null) GetComponent<SpriteRenderer>().sprite = pressedSprite;

        // Hier passiert der Tausch: Brücke her, Void weg!
        ExecuteAction(); 

        Time.timeScale = slowMotionFactor;

        // --- KAMERA FAHRT ---
        Transform originalParent = mainCam.transform.parent;
        Vector3 originalLocalPos = mainCam.transform.localPosition;

        mainCam.transform.parent = null; 

        if (cameraTarget != null)
        {
            Vector3 endPos = new Vector3(cameraTarget.position.x, cameraTarget.position.y, mainCam.transform.position.z);

            while (Vector3.Distance(mainCam.transform.position, endPos) > 0.1f)
            {
                mainCam.transform.position = Vector3.MoveTowards(mainCam.transform.position, endPos, cameraSpeed * Time.unscaledDeltaTime);
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(waitTimeAtTarget);

        mainCam.transform.parent = originalParent;
        mainCam.transform.localPosition = originalLocalPos;
        Time.timeScale = 1.0f;
    }

    void ExecuteAction()
    {
        // Alte Logik für einzelne Tiles (falls du die noch brauchst)
        if (wallTilemap != null)
        {
            foreach (Vector3Int pos in tilesToDelete) wallTilemap.SetTile(pos, null);
        }

        // --- HIER IST DIE WICHTIGE ÄNDERUNG ---
        if (objectToActivate != null) objectToActivate.SetActive(true);    // Brücke erscheint
        if (objectToDeactivate != null) objectToDeactivate.SetActive(false); // Void verschwindet
    }

    // --- TRIGGER LOGIK ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isPlayerInRange = true;
            if(uiPopup != null) uiPopup.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if(uiPopup != null) uiPopup.SetActive(false);
        }
    }
}