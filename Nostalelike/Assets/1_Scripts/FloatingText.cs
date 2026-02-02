using UnityEngine;
using TMPro; // Wichtig für TextMeshPro

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float destroyTime = 1.5f;
    public Vector3 offset = new Vector3(0, 0.5f, 0); // Startet etwas über dem Gegner

    void Start()
    {
        // Text etwas nach oben schieben, damit er nicht im Boden steckt
        transform.position += offset;
        
        // Zerstört das Objekt automatisch nach X Sekunden
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // Bewegt den Text langsam nach oben
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }
}