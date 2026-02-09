using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;


public class StartMenuController : MonoBehaviour
{
    [Header("Load Game Button")]
    [Tooltip("Der Load Game Button – wird automatisch deaktiviert wenn kein Save existiert")]
    [SerializeField] private UnityEngine.UI.Button loadGameButton;

    private string SavePath => Path.Combine(Application.persistentDataPath, "Saves");

    private void Start()
    {
        // Load-Button nur anklickbar machen, wenn ein Savefile existiert
        if (loadGameButton != null)
        {
            bool hasSave = HasAnySaveFile();
            loadGameButton.interactable = hasSave;
            Debug.Log(hasSave
                ? "<color=green>StartMenu: Spielstand gefunden – Load Game verfügbar</color>"
                : "<color=yellow>StartMenu: Kein Spielstand gefunden – Load Game deaktiviert</color>");
        }
    }

    /// <summary>
    /// Prüft ob mindestens ein .json Savefile im Saves-Ordner existiert.
    /// </summary>
    private bool HasAnySaveFile()
    {
        if (!Directory.Exists(SavePath)) return false;
        return Directory.GetFiles(SavePath, "*.json").Length > 0;
    }

    /// <summary>
    /// "NEW GAME" – geht zur Character Creation (nächste Szene im Build-Index).
    /// </summary>
    public void OnStartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// <summary>
    /// "LOAD GAME" – lädt den letzten Spielstand und springt direkt in die gespeicherte Szene.
    /// </summary>
    public void OnLoadGameClicked()
    {
        string saveName = PlayerPrefs.GetString("LastSaveName", "quicksave");
        string filePath = Path.Combine(SavePath, saveName + ".json");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Save-Datei nicht gefunden: {filePath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            Debug.Log($"<color=green>StartMenu: Lade Spielstand '{data.saveName}' – Scene: {data.currentSceneName}</color>");

            // Save-Daten für die Ziel-Scene bereitstellen
            SaveDataHolder.PendingLoadData = data;

            // Direkt in die gespeicherte Scene laden
            if (!string.IsNullOrEmpty(data.currentSceneName))
            {
                SceneManager.LoadScene(data.currentSceneName);
            }
            else
            {
                Debug.LogError("Save-Datei enthält keinen Scene-Namen!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fehler beim Laden: {e.Message}");
        }
    }

    public void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
}
