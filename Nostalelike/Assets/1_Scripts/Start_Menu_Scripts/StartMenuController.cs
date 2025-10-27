using UnityEngine;

public class StartMenuController : MonoBehaviour
{

    public void OnStartClicked()
    {
        // Lädt die nächste Szene im Build-Index (normalerweise das Hauptspiel)
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OnExitClicked()
    {
       #if UNITY_EDITOR
        // Beendet den Play-Modus im Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
       #endif
        
        // Beendet die Anwendung
        Application.Quit();
    }

}
