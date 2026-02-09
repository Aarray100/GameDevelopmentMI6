using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Fügt automatisch Sound-Effekte zu einem Button hinzu.
/// Einfach auf einen Button ziehen - fertig!
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour, IPointerEnterHandler
{
    public enum ButtonSoundType
    {
        Confirm,    // Standard-Klick (New Game, Auswählen, OK)
        Decline,    // Abbrechen, Zurück
        Denied,     // Wenn etwas nicht erlaubt ist
        Hover       // Maus drüber
    }

    [Header("Sound Settings")]
    [Tooltip("Welcher Sound beim Klicken?")]
    public ButtonSoundType clickSound = ButtonSoundType.Confirm;
    
    [Tooltip("Hover-Sound abspielen?")]
    public bool playHoverSound = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance == null) return;

        switch (clickSound)
        {
            case ButtonSoundType.Confirm:
                AudioManager.Instance.PlayConfirmSFX();
                break;
            case ButtonSoundType.Decline:
                AudioManager.Instance.PlayDeclineSFX();
                break;
            case ButtonSoundType.Denied:
                AudioManager.Instance.PlayDeniedSFX();
                break;
        }
    }

    // Wird aufgerufen wenn Maus über Button kommt
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHoverSFX();
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }
}
