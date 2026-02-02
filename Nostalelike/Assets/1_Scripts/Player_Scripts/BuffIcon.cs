using UnityEngine;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour
{
    public Image iconImage;
    public Image durationOverlay; // Das dunkle Bild, das kleiner wird (Fill Amount)

    private float maxDuration;
    private float currentDuration;

    public void Initialize(Sprite sprite, float duration)
    {
        iconImage.sprite = sprite;
        maxDuration = duration;
        currentDuration = duration;
        durationOverlay.fillAmount = 1f;
    }

    void Update()
    {
        if (currentDuration > 0)
        {
            currentDuration -= Time.deltaTime;
            // Berechnet den Kreis-Fortschritt (1.0 = voll, 0.0 = leer)
            durationOverlay.fillAmount = currentDuration / maxDuration;

            if (currentDuration <= 0)
            {
                Destroy(gameObject); // Icon löschen, wenn Zeit abgelaufen
            }
        }
    }
}