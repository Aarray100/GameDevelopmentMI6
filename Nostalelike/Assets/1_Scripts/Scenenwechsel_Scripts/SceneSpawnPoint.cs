using UnityEngine;

public class SceneSpawnPoint : MonoBehaviour
{
   [Tooltip("Name des Spawn-Punkts, der von einem Szenenwechsel-Trigger angesprochen wird")]
    public string spawnPointID;

    private void Start()
    {
        // Debug-Ausgabe um zu sehen welche Spawn-Points in der Szene sind
        Debug.Log($"SceneSpawnPoint: '{spawnPointID}' initialisiert an Position: {transform.position}");
    }
    
    // Visualisiere den Spawn-Point im Editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Zeige den Namen an
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, spawnPointID);
        #endif
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
