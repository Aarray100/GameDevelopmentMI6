using UnityEngine;
using System.Collections; // Wichtig für Coroutines

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

    // --- NEU: Variable für Speed Bonus ---
    private float activeSpeedMultiplier = 1.0f; 
    // ------------------------------------
    
    // GC-freier Buffer für Physics Queries
    private static readonly Collider2D[] hitBuffer = new Collider2D[8];

   
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

        // --- HIER WIRD DER TRANK EINGERECHNET ---
        currentSpeed *= activeSpeedMultiplier;
        // ----------------------------------------

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

    // --- NEU: FUNKTION FÜR SPEED TRANK ---
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        activeSpeedMultiplier = multiplier; // Z.B. 1.3f für 30% mehr
        Debug.Log($"Speed Boost aktiviert! (x{multiplier})");
        
        yield return new WaitForSeconds(duration);
        
        activeSpeedMultiplier = 1.0f; // Zurücksetzen
        Debug.Log("Speed Boost abgelaufen.");
    }
    // -------------------------------------

    private void HandleFootsteps(bool isMoving, bool isRunning)
    {
        if (isMoving)
        {
            stepTimer -= Time.deltaTime;
            // Wenn wir schneller laufen (Trank), spielen wir Sounds auch schneller ab
            float speedFactor = (activeSpeedMultiplier > 1f) ? 0.7f : 1f;

            if (stepTimer <= 0f)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayFootstep(currentGround);
                stepTimer = (isRunning ? runStepInterval : walkStepInterval) * speedFactor;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private bool CheckFootSensor() {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position + footOffset, sensorRadius, hitBuffer);
        for (int i = 0; i < hitCount; i++) {
            Collider2D hit = hitBuffer[i];
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            if (hit.GetComponent<Mud>() != null) return true;
        }
        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<ItemPickup>() != null) return;
        
        if (other.CompareTag("Grass")) currentGround = GroundType.Grass;
        else if (other.CompareTag("Rock")) currentGround = GroundType.Rock;
        else if (other.CompareTag("Wood")) currentGround = GroundType.Wood;
        else if (other.CompareTag("Water")) currentGround = GroundType.Water;
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (IsGroundTag(other.tag)) currentGround = GroundType.Grass;
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