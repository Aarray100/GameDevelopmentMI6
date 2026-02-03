using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    [Header("UI Referenzen")]
    public GameObject dialogUI;        // Das Dialog-Fenster (Zuerst reden)
    public GameObject shopSystem;      // Das eigentliche Shop-Menü
    public GameObject interactionUI;   // Das [E] Interaktions-Pop-up

    [Header("Status (Nur zur Ansicht)")]
    public bool isPlayerInRange = false;

    void Update()
    {
        // 1. Prüfen, ob Spieler in Reichweite und E drückt
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Wenn der Shop oder Dialog schon offen ist, nichts tun
            if ((shopSystem != null && shopSystem.activeSelf) || (dialogUI != null && dialogUI.activeSelf))
            {
                return;
            }

            OpenDialog();
        }
    }

    void OpenDialog()
    {
        if (dialogUI != null)
        {
            dialogUI.SetActive(true);
            
            // --- AUDIO FIX: Nutzt den globalen Manager von Robin ---
            // Wir suchen den AudioManager in der Szene, falls er nicht schon da ist
            if (GameObject.Find("AudioManager") != null) 
            {
                // Beispiel: Rufe die Play-Funktion eures Managers auf
                // Ersetze "Plopp" durch den exakten Namen eures Sounds
                // AudioManager.instance.PlaySound("Plopp"); 
            }

            // [E] Hinweistext ausblenden, während man redet
            if (interactionUI != null) interactionUI.SetActive(false);
            
            Debug.Log("Dialog geöffnet.");
        }
    }

    // Diese Funktion beim Button "Kaufen" im Dialog-Fenster verknüpfen
    public void OpenShopMenu()
    {
        if (dialogUI != null) dialogUI.SetActive(false);
        if (shopSystem != null) shopSystem.SetActive(true);
        
        Debug.Log("Shop-Menü geöffnet.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            
            // Zeige das [E] Pop-up nur, wenn noch nichts offen ist
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
            
            // Alles schließen, wenn der Spieler weggeht (Sicherheit)
            if (interactionUI != null) interactionUI.SetActive(false);
            if (dialogUI != null) dialogUI.SetActive(false);
            if (shopSystem != null) shopSystem.SetActive(false);
        }
    }
}