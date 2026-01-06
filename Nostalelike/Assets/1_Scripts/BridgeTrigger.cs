using UnityEngine;

public class BridgeTrigger : MonoBehaviour
{
    //Trigger betreten
    private void OnTriggerEnter2D(Collider2D other)
    {
        //�berpr�fung Player betreten
        if (other.CompareTag("Player"))
        {
            
            Debug.Log("Spieler betritt die Br�cke. Kollision mit Wasser wird ignoriert.");

            
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Water"), true);
        }
    }

    //Trigger verlassen
    private void OnTriggerExit2D(Collider2D other)
    {
        //�berpr�fung Player verlassen 
        if (other.CompareTag("Player"))
        {
            
            Debug.Log("Spieler verl�sst die Br�cke. Kollision mit Wasser ist wieder aktiv.");

            
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Water"), false);
        }
    }
}