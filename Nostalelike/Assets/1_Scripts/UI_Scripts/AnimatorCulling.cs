using UnityEngine;

/// <summary>
/// Pausiert den Animator wenn das Objekt außerhalb der Kamera ist.
/// Lege dieses Script auf alle NPCs und Enemies die einen Animator haben.
/// 
/// Das spart enorm CPU - jeder aktive Animator kostet ~0.5-1ms pro Frame.
/// Bei 33 Animatoren = ca. 16-33ms gespart!
/// </summary>
[RequireComponent(typeof(Animator))]
public class AnimatorCulling : MonoBehaviour
{
    private Animator anim;
    private bool isVisible = true;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        
        // Nutze Unity's eingebautes Culling
        // CullCompletely = Animator stoppt komplett wenn unsichtbar
        anim.cullingMode = AnimatorCullingMode.CullCompletely;
    }

    // OnBecameVisible/Invisible wird automatisch von Unity aufgerufen
    // wenn ein Renderer sichtbar/unsichtbar für JEDE Kamera wird
    private void OnBecameInvisible()
    {
        isVisible = false;
        
        // Animator komplett stoppen wenn off-screen
        if (anim != null)
            anim.enabled = false;
    }

    private void OnBecameVisible()
    {
        isVisible = true;
        
        // Animator wieder aktivieren wenn on-screen
        if (anim != null)
            anim.enabled = true;
    }
}
