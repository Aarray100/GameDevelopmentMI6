using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{

    public static SceneTransitionManager instance;
    public string targetSpawnPointID;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Stelle sicher, dass das GameObject ein Root-Objekt ist
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


}
