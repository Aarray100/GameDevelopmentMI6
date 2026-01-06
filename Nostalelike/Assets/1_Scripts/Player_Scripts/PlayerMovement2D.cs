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
    public AudioClip walkSFX;
    public AudioClip runSFX;
    [Range(0.2f, 0.6f)] public float walkStepInterval = 0.4f;
    [Range(0.15f, 0.4f)] public float runStepInterval = 0.25f;
    private float stepTimer;

    [Header("Referenzen")]
    public Animator anim;
    public Transform visuals;

    private Rigidbody2D rb;
    private float currentSpeed;
    private float initialScaleX;
    public bool movementLocked;

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

        // Audio abspielen
        HandleFootsteps(isMoving, isRunning);

        if (anim != null) {
            anim.SetBool("isMoving", isMoving);
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
                AudioClip clipToPlay = isRunning ? runSFX : walkSFX;
                float interval = isRunning ? runStepInterval : walkStepInterval;
                
                if (clipToPlay != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(clipToPlay);
                }
                
                stepTimer = interval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    // ...existing code...
    private bool CheckFootSensor() {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + footOffset, sensorRadius);
        
        Debug.Log($"Sensor findet {hits.Length} Collider");
        
        foreach (Collider2D hit in hits) {
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            
            Debug.Log($"Gefunden: {hit.gameObject.name}, Hat Mud: {hit.GetComponent<Mud>() != null}");
            
            if (hit.GetComponent<Mud>() != null) return true;
        }
        return false;
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
}