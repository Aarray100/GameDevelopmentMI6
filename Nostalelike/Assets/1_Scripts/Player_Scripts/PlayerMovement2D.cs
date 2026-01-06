using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Geschwindigkeit")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.0f;
    [Range(0.1f, 1f)] public float mudSpeedMultiplier = 0.4f;

    [Header("Fuß-Sensor")]
    public Vector3 footOffset = new Vector3(0, -0.7f, 0); 
    public float sensorRadius = 0.15f;

    [Header("Audio")]
    [Range(0.2f, 0.6f)] public float walkStepInterval = 0.4f;
    [Range(0.15f, 0.4f)] public float runStepInterval = 0.25f;
    private float stepTimer;
    
    [Header("Ground Detection")]
    public GroundType currentGround = GroundType.Grass;

    [Header("Referenzen")]
    public Animator anim;
    public Transform visuals;

    public bool movementLocked = false;

    private Rigidbody2D rb;
    private float currentSpeed;
    private float initialScaleX;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; 
        rb.freezeRotation = true;

        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (visuals == null) visuals = transform;
        initialScaleX = Mathf.Abs(visuals.localScale.x);
    }

    void Update()
    {
        if (movementLocked)
        {
            rb.linearVelocity = Vector2.zero;
            if (anim != null) anim.SetBool("isMoving", false);
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y).normalized;

        bool isMoving = dir.magnitude > 0.1f;
        bool isRunning = (Input.GetKey(KeyCode.LeftShift)) && isMoving;

        bool onMud = CheckFootSensor();

        float baseSpeed = isRunning ? runSpeed : walkSpeed;
        currentSpeed = onMud ? baseSpeed * mudSpeedMultiplier : baseSpeed;

        rb.linearVelocity = dir * currentSpeed;

        HandleFootsteps(isMoving, isRunning);

        if (anim != null) {
            anim.SetBool("isMoving", isMoving);
            anim.SetBool("isRunning", isRunning);
            anim.SetFloat("horizontal", Mathf.Abs(x));
            anim.SetFloat("vertical", y);
        }

        if (x != 0) visuals.localScale = new Vector3(Mathf.Sign(x) * initialScaleX, visuals.localScale.y, visuals.localScale.z);
    }

    private void HandleFootsteps(bool isMoving, bool isRunning)
    {
        if (isMoving)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                // Nutze Ground-Type basierte Sounds
                AudioManager.Instance?.PlayFootstep(currentGround);
                
                stepTimer = isRunning ? runStepInterval : walkStepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private bool CheckFootSensor() {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + footOffset, sensorRadius);
        
        foreach (Collider2D hit in hits) {
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            if (hit.GetComponent<Mud>() != null) return true;
        }
        return false;
    }

    // Ground-Type Detection via Trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignoriere Items
        if (other.GetComponent<ItemPickup>() != null) return;
        
        if (other.CompareTag("Grass"))
            currentGround = GroundType.Grass;
        else if (other.CompareTag("Rock"))
            currentGround = GroundType.Rock;
        else if (other.CompareTag("Wood"))
            currentGround = GroundType.Wood;
        else if (other.CompareTag("Water"))
            currentGround = GroundType.Water;
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (IsGroundTag(other.tag))
            currentGround = GroundType.Grass;
    }
    
    private bool IsGroundTag(string tag)
    {
        return tag == "Grass" || tag == "Rock" || tag == "Wood" || tag == "Water";
    }

    public void FaceDirection(Vector2 direction) {
        if (Mathf.Abs(direction.x) > 0.1f) 
            visuals.localScale = new Vector3(Mathf.Sign(direction.x) * initialScaleX, visuals.localScale.y, visuals.localScale.z);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + footOffset, sensorRadius);
    }

    public void ForceStop()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public bool IsMoving()
    {
        return rb.linearVelocity.sqrMagnitude > 0.01f;
    }
}