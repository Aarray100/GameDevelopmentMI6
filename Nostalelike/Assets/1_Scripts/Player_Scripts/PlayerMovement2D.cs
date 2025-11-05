using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.5f;
    private float currentSpeed;
    private Rigidbody2D rb;
    public Animator anim;
    public Transform visualsTransform; // Das Transform des visuellen Teils des Charakters (für das Flippen)

    // Speichert die letzte normalisierte Bewegungsrichtung (für den 1D Blend Tree)
    private Vector2 lastMoveDirection = new Vector2(0, -1); 

    // Speichert die initiale Skalierung (zum korrekten Flippen)
    private float initialFacingDirection = 1f;

    // Speichert die letzte STABILE horizontale Blickrichtung (+1 oder -1)
    private float lastStableHorizontal = 1f;
    private Vector2 movementVector = Vector2.zero;

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
        currentSpeed = walkSpeed;
    }
    
    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        movementVector = new Vector2(moveHorizontal, moveVertical);
        if (movementVector.magnitude > 1)
        {
            movementVector = movementVector.normalized;
        }
        
        // Geschwindigkeit basierend auf Shift-Taste setzen
        bool isShiftPressed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        bool isMoving = (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0);



        if (isShiftPressed && isMoving)
        {
            currentSpeed = runSpeed;
            anim.SetBool("isRunning", true);
        }
        else
        {
            currentSpeed = walkSpeed;
            anim.SetBool("isRunning", false);
        }

       // --- 3. IDLE-RICHTUNGS-LOGIK (KORRIGIERT) ---
        // Wir verwenden 'isMoving', das du oben schon berechnet hast.
        if (isMoving) 
        {
            // Prüfen, ob die Bewegung STÄRKER horizontal als vertikal ist
            if (Mathf.Abs(moveHorizontal) > Mathf.Abs(moveVertical))
            {
                // Horizontale Bewegung dominiert:
                lastStableHorizontal = Mathf.Sign(moveHorizontal);
                lastMoveDirection.y = 0; // Für 1D Idle Tree (Idle Rechts/Links)
            }
            else
            {
                // Vertikale Bewegung dominiert (oder ist gleich stark):
                // Setzt Y auf 1 (Oben) oder -1 (Unten).
                lastMoveDirection.y = Mathf.Sign(moveVertical);
            }
        }
        // Wenn isMoving = false, behält lastMoveDirection.y seinen letzten Wert,
        // was genau das ist, was wir für Idle wollen.
        Flip(lastStableHorizontal);
        anim.SetFloat("horizontal", Mathf.Abs(moveHorizontal));
        anim.SetFloat("vertical", (moveVertical));
        anim.SetFloat("LastMoveY", lastMoveDirection.y);
        anim.SetBool("isMoving", isMoving);

    }


    private void FixedUpdate()
    {    
        rb.linearVelocity = movementVector * currentSpeed;
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