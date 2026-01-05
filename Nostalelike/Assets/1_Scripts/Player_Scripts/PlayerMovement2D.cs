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

    [Header("Referenzen")]
    public Animator anim;
    public Transform visuals;

    private Rigidbody2D rb;
    private float currentSpeed;
    private float initialScaleX;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        // Top-Down Einstellungen erzwingen
        rb.gravityScale = 0; 
        rb.freezeRotation = true;

        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (visuals == null) visuals = transform;
        initialScaleX = Mathf.Abs(visuals.localScale.x);
    }

    void Update() {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y).normalized;

        bool isMoving = dir.magnitude > 0.1f;
        bool isRunning = (Input.GetKey(KeyCode.LeftShift)) && isMoving;

        // MATSCH-LOGIK: Prüfen, ob das Objekt unter den Füßen die "Mud"-Komponente hat
        bool onMud = CheckFootSensor();

        float baseSpeed = isRunning ? runSpeed : walkSpeed;
        currentSpeed = onMud ? baseSpeed * mudSpeedMultiplier : baseSpeed;

        rb.linearVelocity = dir * currentSpeed;

        // Animationen & Flip
        if (anim != null) {
            anim.SetBool("isMoving", isMoving);
            anim.SetFloat("horizontal", Mathf.Abs(x));
            anim.SetFloat("vertical", y);
        }
        if (x != 0) visuals.localScale = new Vector3(Mathf.Sign(x) * initialScaleX, visuals.localScale.y, visuals.localScale.z);
    }

    private bool CheckFootSensor() {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + footOffset, sensorRadius);
        
        Debug.Log($"Sensor findet {hits.Length} Collider"); // DEBUG
        
        foreach (Collider2D hit in hits) {
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            
            Debug.Log($"Gefunden: {hit.gameObject.name}, Hat Mud: {hit.GetComponent<Mud>() != null}"); // DEBUG
            
            if (hit.GetComponent<Mud>() != null) return true;
        }
        return false;
    }

    // Damit dein Combat-Script nicht meckert
    public void FaceDirection(Vector2 direction) {
        if (Mathf.Abs(direction.x) > 0.1f) 
            visuals.localScale = new Vector3(Mathf.Sign(direction.x) * initialScaleX, visuals.localScale.y, visuals.localScale.z);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + footOffset, sensorRadius);
    }
}