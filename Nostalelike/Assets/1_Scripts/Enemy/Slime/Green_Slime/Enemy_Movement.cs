using UnityEngine;
using UnityEngine.XR;



public class NewEmptyCSharpScript : MonoBehaviour
{
    public float speed;
    public float attacCoolddown = 2;
    private float attackCooldownTimer;
    private int facingDirection = 1;
    private EnemyState enemyState;
    public float playerDetectRange = 5;
    public Transform detectionPoint;
    public LayerMask playerLayer;

   

    public float attackRange = 1.2f;


    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;


    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        ChangeState(EnemyState.Idle);
    }



    void Update()
    {

        CheckForPlayer();
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (enemyState == EnemyState.Chasing)
        {
            Chase();
        }
        else if (enemyState == EnemyState.Attacking)
        {
            //Do attack behavior
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detectionPoint.position, playerDetectRange);
        
    }

    void Chase()
    {
        if (player.position.x < transform.position.x && facingDirection == -1 ||
               player.position.x > transform.position.x && facingDirection == 1)
        {
            Flip();
        }

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    private void CheckForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectRange, playerLayer);
        if(hits.Length > 0)
        {
            player = hits[0].transform;

            //if the player is in attack range AND cooldown is ready
            if (Vector2.Distance(transform.position, player.position) <= attackRange && attackCooldownTimer <= 0)
            {
                attackCooldownTimer = attacCoolddown;
                ChangeState(EnemyState.Attacking);
            }

            else if (Vector2.Distance(transform.position, player.position) > attackRange)
            {
                ChangeState(EnemyState.Chasing);
            }

            else
            {

                rb.linearVelocity = Vector2.zero;
                ChangeState(EnemyState.Idle);
            }
      

        }
    }




    void ChangeState(EnemyState newState)
    {
        //Exit the current animation state
        if (enemyState == EnemyState.Idle)
        {
            anim.SetBool("isIdle", false);
        }
        else if (enemyState == EnemyState.Chasing)
        {
            anim.SetBool("isChasing", false);
        }
        else if (enemyState == EnemyState.Attacking)
        {
            anim.SetBool("isAttacking", false);
        }

        //Update our current state
        enemyState = newState;

        //Enter the new animation state)
        if (enemyState == EnemyState.Idle)
        {
            anim.SetBool("isIdle", true);
        }
        else if (enemyState == EnemyState.Chasing)
        {
            anim.SetBool("isChasing", true);
        }
        else if (enemyState == EnemyState.Attacking)
        {
            anim.SetBool("isAttacking", true);

        }
    }


    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking
    }

    public void Attack()
    {
        
    }
}