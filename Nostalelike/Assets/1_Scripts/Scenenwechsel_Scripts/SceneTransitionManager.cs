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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


}
