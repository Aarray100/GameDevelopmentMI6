using UnityEngine;
using System.Collections;

public class VoidFall : MonoBehaviour
{
    [Header("Fall-Einstellungen")]
    public float fallDuration = 0.8f; // Wie lange der Fall dauert
    public float targetScalePercent = 0.6f; // 0.6 bedeutet 40% kleiner als das Original
    
    [Header("Respawn")]
    public string spawnPointName = "[SceneSpawnPoint]";

    private bool isFalling = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Wir prüfen, ob der Spieler (Tag "Player") hineinfällt
        if (other.CompareTag("Player") && !isFalling)
        {
            StartCoroutine(FallIntoVoidRoutine(other.gameObject));
        }
    }

    IEnumerator FallIntoVoidRoutine(GameObject player)
    {
        isFalling = true;

        // 1. Komponenten finden
        var movement = player.GetComponent<PlayerMovement2D>();
        var stats = player.GetComponent<PlayerStats>();
        var rb = player.GetComponent<Rigidbody2D>();

        // 2. Steuerung & Physik stoppen
        if (movement != null) movement.enabled = false;
        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; 
        }

        // 3. Animation: Nur Schrumpfen auf 60% Größe (ohne Drehen)
        Vector3 startScale = player.transform.localScale;
        Vector3 endScale = startScale * targetScalePercent; // Ziel-Größe berechnen
        float elapsed = 0;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / fallDuration;
            
            // Spieler schrumpft auf 60% seiner ursprünglichen Größe
            player.transform.localScale = Vector3.Lerp(startScale, endScale, percent);
            
            // Die Zeile für Rotate wurde entfernt!
            
            yield return null;
        }

        // 4. Schaden/Tod
        if (stats != null)
        {
            stats.currentHealth = 0; 
        }

        yield return new WaitForSeconds(0.4f); // Kurze Pause unten im "Loch"

        // 5. Respawn am SpawnPoint
        RespawnPlayer(player, startScale, movement, rb);
    }

    void RespawnPlayer(GameObject player, Vector3 originalScale, PlayerMovement2D movement, Rigidbody2D rb)
    {
        GameObject spawnPoint = GameObject.Find(spawnPointName);
        
        if (spawnPoint != null)
        {
            player.transform.position = spawnPoint.transform.position;
        }

        // Alles zurücksetzen
        player.transform.localScale = originalScale;
        player.transform.rotation = Quaternion.identity;
        
        if (rb != null) rb.simulated = true;
        if (movement != null) movement.enabled = true;
        
        isFalling = false;
    }
}