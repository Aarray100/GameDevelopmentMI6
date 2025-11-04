using UnityEngine;



public class NewEmptyCSharpScript : MonoBehaviour
{
    public float speed;
    private bool isChasing;
    private int facingDirection = 1;
    private EnemyState enemyState;


    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }



    void Update()
    {
        if (isChasing == true)
        {
            if (player.position.x < transform.position.x && facingDirection == -1
        || player.position.x > transform.position.x && facingDirection == 1)
            {
                Flip();
            }
            {
                if (player.position.x < transform.position.x && facingDirection == 1)
                {
                    facingDirection = -1;
                    Vector3 localScale = transform.localScale;
                    localScale.x *= -1;
                    transform.localScale = localScale;
                }
                else if (player.position.x > transform.position.x && facingDirection == -1)
                {
                    facingDirection = 1;
                    Vector3 localScale = transform.localScale;
                    localScale.x *= -1;
                    transform.localScale = localScale;
                }
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * speed;
            }
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
            isChasing = true;
    }
    }
    

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isChasing = false;
            rb.linearVelocity = Vector2.zero;
        }
    }
}


public enum EnemyState
{
    Idle,
    Chasing,
}