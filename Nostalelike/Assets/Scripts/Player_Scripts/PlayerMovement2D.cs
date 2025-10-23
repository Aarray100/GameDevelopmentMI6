using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    public float speed = 5.0f;
    private Rigidbody2D rb;
    public Animator anim;
    public Transform visualsTransform; // Das Transform des visuellen Teils des Charakters (für das Flippen)

    // Speichert die letzte normalisierte Bewegungsrichtung (für den 1D Blend Tree)
    private Vector2 lastMoveDirection = new Vector2(0, -1); 

    // Speichert die initiale Skalierung (zum korrekten Flippen)
    private float initialFacingDirection = 1f;
    
    // Speichert die letzte STABILE horizontale Blickrichtung (+1 oder -1)
    private float lastStableHorizontal = 1f; 


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        if (visualsTransform == null)
        {
            Debug.LogError("Visuals Transform is not assigned in PlayerMovement2D script.");
            visualsTransform = transform.GetComponentInChildren<Animator>().transform;
        }

        initialFacingDirection = Mathf.Abs(visualsTransform.localScale.x);
        lastStableHorizontal = initialFacingDirection;
    }


    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        
        float inputThreshold = 0.05f; 

        // 1. INPUT AN DEN ANIMATOR SENDEN (für den Movement-Zustand)
        anim.SetFloat("horizontal", moveHorizontal);
        anim.SetFloat("vertical", moveVertical);

        // 2. BEWEGUNG AUSFÜHREN
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }
        rb.linearVelocity = movement * speed;

        // 3. LOGIK FÜR LAST MOVE DIRECTION & STABILE BLICKRICHTUNG
        if (movement.magnitude > inputThreshold) 
        {
            // A) LastMoveDirection für den Idle Blend Tree
            lastMoveDirection = movement.normalized;
            
            // B) Update der STABILEN horizontalen Blickrichtung 
            // Nur aktualisieren, wenn horizontal aktiv gedrückt wird
            if (Mathf.Abs(moveHorizontal) > inputThreshold)
            {
                // Speichert nur -1 oder 1
                lastStableHorizontal = Mathf.Sign(moveHorizontal);
                
                // WICHTIG: Wenn wir horizontal gehen, soll Y im Blend Tree 0 sein
                lastMoveDirection.y = 0; 
            }
        }
        
        // 4. ANWENDEN DES FLIP AUF DAS GAMEOBJECT
        // IMMER basierend auf der stabilen Blickrichtung aufrufen
        Flip(lastStableHorizontal);

        // 5. SENDEN DER LETZTEN RICHTUNG AN DEN ANIMATOR (für den 1D Blend Tree)
        anim.SetFloat("LastMoveX", lastMoveDirection.x);
        anim.SetFloat("LastMoveY", lastMoveDirection.y);
    }
    
    
    // Die NEU KORRIGIERTE Flip-Methode
 // Die Methode zum Spiegeln (flippen) des Sprites
void Flip(float horizontalDirection)
{
    // Die gewünschte X-Skala (entweder initialFacingDirection oder -initialFacingDirection)
    float targetScaleX = transform.localScale.x; 

    // Rechts (positive Richtung)
    if (horizontalDirection > 0)
    {
        // Setzt targetScaleX auf den positiven Betrag
        targetScaleX = Mathf.Abs(initialFacingDirection);
    }
    // Links (negative Richtung)
    else if (horizontalDirection < 0)
    {
        // Setzt targetScaleX auf den negativen Betrag
        targetScaleX = -Mathf.Abs(initialFacingDirection);
    }

    // WICHTIG: Ändere die Skala NUR, wenn die aktuelle Skala NICHT der Zielskala entspricht.
    // Dadurch wird die Zuweisung (und das Flackern) unterbunden, wenn keine Änderung notwendig ist.
    if (visualsTransform.localScale.x != targetScaleX)
    {
        // Setzt die neue Skala, behält Y und Z bei.
        visualsTransform.localScale = new Vector3(targetScaleX, visualsTransform.localScale.y, visualsTransform.localScale.z);
    }
    // Wenn horizontalDirection == 0, wird die Skala NICHT geändert (bleibt stabil).
}
}