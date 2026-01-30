using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    [Header("UI Referenzen")]
    public GameObject dialogUI;        // Das Dialog_Canvas aus der Hierarchy
    public GameObject shopSystem;      // Das ShopPanel (das eigentliche Fenster)
    public GameObject interactionUI;   // Der "[E] Reden" Hinweistext (optional)

    [Header("Audio")]
    public AudioSource audioSource;    // Ziehe hier deinen SoundManager rein
    public AudioClip dialogPlopp;     // Dein in Bfxr erstellter Plopp-Sound

    [Header("Status (Nur zum schauen)")]
    public bool isPlayerInRange = false;

    void Update()
    {
        // Prüft, ob der Player im Kreis steht, E drückt UND der Shop nicht schon offen ist
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Verhindert, dass der Dialog aufploppt, wenn man im Shop-Menü E drückt
            if (shopSystem != null && shopSystem.activeSelf) 
            {
                return; 
            }

            if (dialogUI != null)
            {
                dialogUI.SetActive(true); // Dialog einschalten
                
                // --- NEU: Dialog-Sound abspielen ---
                if (audioSource != null && dialogPlopp != null)
                {
                    audioSource.PlayOneShot(dialogPlopp);
                }
                
                // Hinweistext ausblenden, wenn der Dialog startet
                if (interactionUI != null) interactionUI.SetActive(false);
                
                Debug.Log("E wurde gedrückt - Dialog mit Sound geöffnet!");
            }
            else
            {
                Debug.LogError("Fehler: Dialog UI ist nicht im Inspector zugewiesen!");
            }
        }
    }

    // Diese Funktion beim Button "Kaufen" im Dialog-Fenster verknüpfen
    public void OpenShopMenu()
    {
        Debug.Log("Button geklickt - Shop wird geöffnet!");
        
        if (dialogUI != null) dialogUI.SetActive(false); // Dialog schließen
        
        if (shopSystem != null) 
        {
            shopSystem.SetActive(true); // Shop öffnen
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Erfolg: Player erkannt!");
            
            // Hinweistext nur einblenden, wenn Dialog und Shop zu sind
            if (interactionUI != null && !dialogUI.activeSelf && !shopSystem.activeSelf) 
            {
                interactionUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            
            // Alles ausblenden, wenn der Player weggeht
            if (interactionUI != null) interactionUI.SetActive(false);
            if (dialogUI != null) dialogUI.SetActive(false);
            if (shopSystem != null) shopSystem.SetActive(false);
            
            Debug.Log("Player hat Bereich verlassen - Alles geschlossen.");
        }
    }
}