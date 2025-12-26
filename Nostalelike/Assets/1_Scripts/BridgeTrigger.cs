using UnityEngine;

using UnityEngine;

public class BridgeTrigger : MonoBehaviour
{
    //Trigger betreten
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Überprüfung Player betreten
        if (other.CompareTag("Player"))
        {
            
            Debug.Log("Spieler betritt die Brücke. Kollision mit Wasser wird ignoriert.");

            
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Water"), true);
        }
    }

    //Trigger verlassen
    private void OnTriggerExit2D(Collider2D other)
    {
        //Überprüfung Player verlassen 
        if (other.CompareTag("Player"))
        {
            
            Debug.Log("Spieler verlässt die Brücke. Kollision mit Wasser ist wieder aktiv.");

            
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Water"), false);
        }
    }
}