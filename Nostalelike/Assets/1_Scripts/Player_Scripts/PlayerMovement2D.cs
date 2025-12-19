using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.5f;
    [Range(0.1f, 1f)] public float mudSpeedMultiplier = 0.5f; 

    [Header("Detection")]
    public Tilemap groundTilemap; 
    public string mudTileNamePart = "Matsch"; 

    [Header("References")]
    public Animator anim;
    public Transform visualsTransform;

    private Rigidbody2D rb;
    private float currentSpeed;
    private Vector2 lastMoveDirection = new Vector2(0, -1);
    private float initialFacingDirection = 1f;
    private float lastStableHorizontal = 1f;
    private Vector2 movementVector = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        if (visualsTransform == null)
        {
            visualsTransform = transform.GetComponentInChildren<Animator>().transform;
        }

        // --- AUTOMATISCHE SUCHE FÜR MULTI-SCENES ---
        // Wenn im Inspector nichts zugewiesen ist, suchen wir zuerst nach einem Objekt namens "Mud"
        if (groundTilemap == null)
        {
            GameObject mudObj = GameObject.Find("Mud");
            if (mudObj != null) groundTilemap = mudObj.GetComponent<Tilemap>();
            
            // Falls es kein Objekt "Mud" gibt, nehmen wir die erste Tilemap, die wir finden
            if (groundTilemap == null) groundTilemap = Object.FindFirstObjectByType<Tilemap>();
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

        bool isShiftPressed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        bool isMoving = (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0);

        // --- MATSCH LOGIK ---
        float targetBaseSpeed = (isShiftPressed && isMoving) ? runSpeed : walkSpeed;
        
        if (IsOnMud())
        {
            currentSpeed = targetBaseSpeed * mudSpeedMultiplier;
        }
        else
        {
            currentSpeed = targetBaseSpeed;
        }

        anim.SetBool("isRunning", isShiftPressed && isMoving);

        if (isMoving)
        {
            if (Mathf.Abs(moveHorizontal) > Mathf.Abs(moveVertical))
            {
                lastStableHorizontal = Mathf.Sign(moveHorizontal);
                lastMoveDirection.y = 0;
            }
            else
            {
                lastMoveDirection.y = Mathf.Sign(moveVertical);
            }
        }

        Flip(lastStableHorizontal);
        anim.SetFloat("horizontal", Mathf.Abs(moveHorizontal));
        anim.SetFloat("vertical", moveVertical);
        anim.SetFloat("LastMoveY", lastMoveDirection.y);
        anim.SetBool("isMoving", isMoving);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movementVector * currentSpeed;
    }

    // --- MATSCH PRÜFUNG ---
    private bool IsOnMud()
    {
        if (groundTilemap == null) return false;

        Vector3Int cellPosition = groundTilemap.WorldToCell(transform.position);
        TileBase currentTile = groundTilemap.GetTile(cellPosition);

        return currentTile != null && currentTile.name.Contains(mudTileNamePart);
    }

    // --- DIESE FUNKTION IST WICHTIG FÜR PlayerCombat ---
    public void FaceDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            lastStableHorizontal = Mathf.Sign(direction.x);
            Flip(lastStableHorizontal);
        }
    }

    void Flip(float horizontalDirection)
    {
        if (visualsTransform == null) return;

        float targetScaleX = visualsTransform.localScale.x;

        if (horizontalDirection > 0) targetScaleX = Mathf.Abs(initialFacingDirection);
        else if (horizontalDirection < 0) targetScaleX = -Mathf.Abs(initialFacingDirection);

        if (visualsTransform.localScale.x != targetScaleX)
        {
            visualsTransform.localScale = new Vector3(targetScaleX, visualsTransform.localScale.y, visualsTransform.localScale.z);
        }
    }
}