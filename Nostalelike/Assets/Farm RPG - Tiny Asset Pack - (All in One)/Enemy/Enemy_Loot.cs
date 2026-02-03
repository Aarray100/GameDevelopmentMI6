using UnityEngine;
using TMPro; 

public class EnemyLoot : MonoBehaviour
{
    [Header("Loot Einstellungen")]
    public int minGoldDroprate = 5;
    public int maxGoldDroprate = 20;

    [Header("Referenzen")]
    public GameObject popupTextPrefab; 
    public ItemData goldItemAsset;     

    public void DropLoot()
    {
        int randomGold = Random.Range(minGoldDroprate, maxGoldDroprate + 1);

        // 1. Gold zum Manager
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.GoldHinzufuegen(randomGold);
            Debug.Log(gameObject.name + " hat " + randomGold + " Gold gedroppt.");
        }

        // 2. Text anzeigen
        if (popupTextPrefab != null)
        {
            GameObject popup = Instantiate(popupTextPrefab, transform.position, Quaternion.identity);

            TextMeshPro textMesh = popup.GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = "+" + randomGold + " Gold";
                
                // --- WICHTIG: Hier machen wir ihn GOLD/GELB! ---
                textMesh.color = Color.yellow; 
            }
        }
    }
}