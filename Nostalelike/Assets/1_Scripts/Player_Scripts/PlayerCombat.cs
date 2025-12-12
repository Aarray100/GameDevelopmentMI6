using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement2D playerMovement;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement2D>();
    }

    public void MeleeAttack()
    {
        Debug.Log("PlayerCombat: MeleeAttack called");

        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>(); // Fallback: Suche irgendeine Kamera
            if (cam == null)
            {
                Debug.LogError("PlayerCombat: KEINE KAMERA GEFUNDEN! Bitte tagge deine Kamera als 'MainCamera'.");
                return;
            }
        }

        // 1. Mausposition in Weltkoordinaten holen
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // Z auf 0 setzen für 2D

        // 2. Richtung vom Spieler zur Maus berechnen
        Vector2 direction = (mousePos - transform.position).normalized;

        // 3. Charakter in die Richtung drehen
        if (playerMovement != null)
        {
            playerMovement.FaceDirection(direction);
        }
        else
        {
            Debug.LogWarning("PlayerCombat: PlayerMovement is null");
        }

        // 4. Animator Parameter setzen
        if (anim != null)
        {
            Debug.Log($"PlayerCombat: Setting Trigger 'Attack'. Dir: {direction}");
            anim.SetFloat("AttackX", direction.x);
            anim.SetFloat("AttackY", direction.y);
            anim.SetTrigger("Attack");
        }
        else
        {
            Debug.LogError("PlayerCombat: Animator is null!");
        }
    }
}
