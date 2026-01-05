using UnityEngine;

public class JournalOverlay : MonoBehaviour
{
    [SerializeField] GameObject panel;          // dein JournalRoot / Panel
    [SerializeField] PlayerMovement2D movement; // optional

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.J)) return; // KeyDown gehört in Update [web:144]

        if (panel == null)
        {
            Debug.LogError("JournalOverlay: panel ist nicht zugewiesen!");
            return;
        }

        bool open = !panel.activeSelf;
        panel.SetActive(open);

        if (movement != null)
        {
            movement.movementLocked = open;
            movement.ForceStop();
        }
    }
}
