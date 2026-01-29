using System.Collections;
using UnityEngine;

//Somethings wrong with the patrol script, the npc walks in a downward diagonal line instead of between the set points.

public class NPC_Patrol : MonoBehaviour
{
    public Vector2[] patrolPoints; // Array of points to patrol between
     public float speed = 2f; // Speed of movement

     public float pauseDuration = 1.5f;


    private bool isPaused;
    private int currentPatrolIndex;
    private Vector2 target;

   

    private Rigidbody2D rb;
    private Animator anim;

    void OnEnable()
    {
        transform.position = patrolPoints[0];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        currentPatrolIndex = 0;
        target = patrolPoints[currentPatrolIndex];
        transform.position = patrolPoints[currentPatrolIndex]; // Ensure correct starting position
        StartCoroutine(SetPatrolPoint());

    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 direction = ((Vector3)target - transform.position).normalized;
        if (direction.x < 0 && transform.localScale.x > 0 || direction.x > 0 && transform.localScale.x < 0)
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        

        rb.linearVelocity = direction * speed;

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            StartCoroutine(SetPatrolPoint());
        }
    }

    IEnumerator SetPatrolPoint()
    {
        isPaused = true;
        
        yield return new WaitForSeconds(pauseDuration);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        target = patrolPoints[currentPatrolIndex];
        isPaused = false;
    }

}
