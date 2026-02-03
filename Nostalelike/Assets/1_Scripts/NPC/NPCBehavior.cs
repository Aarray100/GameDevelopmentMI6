using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    [Header("Idle Animation")]
    [SerializeField] private bool playIdleAnimation = true;
    
    [Header("Wander Settings")]
    [SerializeField] private bool canWander = false;
    [SerializeField] private float wanderRadius = 2f;
    [SerializeField] private float wanderSpeed = 1f;
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;
    
    [Header("Interaction")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private string npcName = "NPC";
    [SerializeField] [TextArea] private string[] dialogueLines;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float waitTimer;
    private bool isWaiting = true;
    
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }
    
    void Update()
    {
        if (!canWander) return;
        
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                PickNewTarget();
                isWaiting = false;
            }
        }
        else
        {
            MoveToTarget();
        }
    }
    
    private void PickNewTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        targetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
    }
    
    private void MoveToTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * wanderSpeed * Time.deltaTime;
        
        // Flip sprite basierend auf Bewegungsrichtung
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
        
        // Animation setzen
        if (animator != null)
        {
            animator.SetFloat("moveX", direction.x);
            animator.SetFloat("moveY", direction.y);
            animator.SetBool("isMoving", true);
        }
        
        // Ziel erreicht?
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isWaiting = true;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
            
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }
    }
    
    // Für Interaktion mit dem Spieler
    public string GetName() => npcName;
    
    public string GetRandomDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
            return "...";
        
        return dialogueLines[Random.Range(0, dialogueLines.Length)];
    }
}