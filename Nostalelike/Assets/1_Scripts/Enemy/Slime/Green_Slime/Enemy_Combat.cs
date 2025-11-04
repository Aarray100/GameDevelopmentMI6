using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    public int damage = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // Assuming the player has a script with a method 'TakeDamage(int amount)'
            collision.gameObject.GetComponent<PlayerHealth>().ChangeHealth(-damage);
        }
    }    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
