using UnityEngine;

public class GoldMuenze : MonoBehaviour
{
    public int wert = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Prüfen, ob das Objekt den Tag "Player" hat
        if (other.CompareTag("Player"))
        {
            // Da gold "static" ist, schreiben wir einfach: SkriptName.Variable
            PlayerMovement2D.gold += wert; 
            PlayerMovement2D.GoldSpeichern();
            Debug.Log("Gold gesammelt! Kontostand: " + PlayerMovement2D.gold);
            Destroy(gameObject); // Münze verschwindet
        }
    }
}