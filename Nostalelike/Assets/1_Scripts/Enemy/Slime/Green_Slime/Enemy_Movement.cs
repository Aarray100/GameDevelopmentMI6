using UnityEngine;
using UnityEngine.XR;



public class NewEmptyCSharpScript : MonoBehaviour
{
    public float speed;
    private int facingDirection = 1;
    private EnemyState enemyState;


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
        if (enemyState == EnemyState.Chasing)
        {
            if(player.position.x < transform.position.x && facingDirection == -1||
                player.position.x > transform.position.x && facingDirection == 1)
            {
                Flip();
            }

            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
        else if (enemyState == EnemyState.Idle)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (player == null)
            {
                player = collision.transform;
            }
            ChangeState(EnemyState.Chasing);
    }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            ChangeState(EnemyState.Idle);
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

    }
}


public enum EnemyState
{
    Idle,
    Chasing,
}