using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using System.IO;

/// <summary>
/// Editor-Tool zum automatischen Erstellen von Minimap-Screenshots für alle Szenen.
/// Öffne: Tools → Minimap Screenshot Tool
/// </summary>
public class MapScreenshotTool : EditorWindow
{
    private string outputFolder = "Assets/4_UI_Elements/MinimapImages";
    private float cameraHeight = 100f;
    private float orthographicSize = 50f;
    private Color backgroundColor = Color.black;
    private int screenshotWidth = 2048;
    private int screenshotHeight = 2048;
    private bool autoDetectBounds = true;
    private LayerMask cullingMask = -1; // Alle Layer

    private Vector2 scrollPosition;

    [MenuItem("Tools/Minimap Screenshot Tool")]
    public static void ShowWindow()
    {
        MapScreenshotTool window = GetWindow<MapScreenshotTool>("Minimap Tool");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Minimap Screenshot Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Dieses Tool erstellt automatisch Top-Down Screenshots für deine Minimaps.\n\n" +
            "1. Stelle die Einstellungen ein\n" +
            "2. Klicke auf 'Screenshot der aktuellen Szene'\n" +
            "3. Oder nutze 'Alle Szenen durchlaufen' für Batch-Processing",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        // Einstellungen
        EditorGUILayout.LabelField("Kamera-Einstellungen", EditorStyles.boldLabel);
        cameraHeight = EditorGUILayout.FloatField("Kamera Höhe (Y)", cameraHeight);
        orthographicSize = EditorGUILayout.FloatField("Orthographic Size", orthographicSize);
        backgroundColor = EditorGUILayout.ColorField("Hintergrundfarbe", backgroundColor);
        cullingMask = LayerMaskField("Culling Mask", cullingMask);

        EditorGUILayout.Space(5);
        autoDetectBounds = EditorGUILayout.Toggle("Auto-Detect Bounds", autoDetectBounds);
        if (!autoDetectBounds)
        {
            EditorGUILayout.HelpBox("Manuelle Einstellung: Die Kamera wird an (0, " + cameraHeight + ", 0) platziert.", MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox("Auto-Detect sucht nach einem MapSceneInfo BoxCollider in der Szene.", MessageType.None);
        }

        EditorGUILayout.Space(10);

        // Screenshot-Einstellungen
        EditorGUILayout.LabelField("Screenshot-Einstellungen", EditorStyles.boldLabel);
        screenshotWidth = EditorGUILayout.IntField("Breite", screenshotWidth);
        screenshotHeight = EditorGUILayout.IntField("Höhe", screenshotHeight);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Output Ordner:", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField(outputFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string path = EditorUtility.OpenFolderPanel("Output Ordner wählen", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                outputFolder = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Aktionen
        EditorGUILayout.LabelField("Aktionen", EditorStyles.boldLabel);

        if (GUILayout.Button("Screenshot der aktuellen Szene", GUILayout.Height(40)))
        {
            CaptureCurrentScene();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Alle Szenen im Build durchlaufen", GUILayout.Height(40)))
        {
            CaptureAllScenes();
        }

        EditorGUILayout.Space(10);

        // Hinweise
        EditorGUILayout.HelpBox(
            "Tipp: Stelle sicher, dass in jeder Szene ein MapSceneInfo mit BoxCollider existiert, " +
            "wenn du Auto-Detect nutzen möchtest.",
            MessageType.Warning
        );

        EditorGUILayout.EndScrollView();
    }

    private void CaptureCurrentScene()
    {
        string sceneName = EditorSceneManager.GetActiveScene().name;

        if (string.IsNullOrEmpty(sceneName))
        {
            EditorUtility.DisplayDialog("Fehler", "Keine Szene geladen!", "OK");
            return;
        }

        // Output-Ordner erstellen
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }

        // Screenshot machen
        CaptureScreenshot(sceneName);
    }

    private void CaptureAllScenes()
    {
        if (EditorUtility.DisplayDialog(
            "Alle Szenen durchlaufen?",
            "Dies wird alle Szenen im Build Settings durchlaufen und Screenshots erstellen.\n\n" +
            "Die aktuelle Szene wird gespeichert und danach wiederhergestellt.",
            "Fortfahren",
            "Abbrechen"))
        {
            // Aktuelle Szene speichern
            string currentScenePath = EditorSceneManager.GetActiveScene().path;

            // Alle Szenen im Build durchgehen
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog("Fehler", "Keine Szenen in den Build Settings gefunden!", "OK");
                return;
            }

            // Output-Ordner erstellen
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
                AssetDatabase.Refresh();
            }

            int successCount = 0;

            for (int i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled) continue;

                string scenePath = scenes[i].path;
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                // Progress Bar
                float progress = (float)i / scenes.Length;
                if (EditorUtility.DisplayCancelableProgressBar(
                    "Screenshots erstellen",
                    $"Szene {i + 1}/{scenes.Length}: {sceneName}",
                    progress))
                {
                    break; // Abgebrochen
                }

                // Szene laden
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // Screenshot machen
                if (CaptureScreenshot(sceneName))
                {
                    successCount++;
                }
            }

            EditorUtility.ClearProgressBar();

            // Ursprüngliche Szene wiederherstellen
            if (!string.IsNullOrEmpty(currentScenePath))
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }

            EditorUtility.DisplayDialog(
                "Fertig!",
                $"{successCount} Screenshots erfolgreich erstellt!\n\nOrdner: {outputFolder}",
                "OK"
            );

            AssetDatabase.Refresh();
        }
    }

    private bool CaptureScreenshot(string sceneName)
    {
        // Temporäre Kamera erstellen
        GameObject camObj = new GameObject("TempScreenshotCamera");
        Camera cam = camObj.AddComponent<Camera>();

        // Kamera-Position und -Einstellungen
        Vector3 cameraPosition = new Vector3(0, cameraHeight, 0);

        // Auto-Detect: Finde MapSceneInfo und zentriere Kamera über den Bounds
        if (autoDetectBounds)
        {
            MapSceneInfo mapInfo = FindObjectOfType<MapSceneInfo>();
            if (mapInfo != null && mapInfo.boundaryReference != null)
            {
                Bounds bounds = mapInfo.WorldBounds;
                cameraPosition = new Vector3(bounds.center.x, cameraHeight, bounds.center.z);

                // Orthographic Size anpassen basierend auf den Bounds
                float maxSize = Mathf.Max(bounds.size.x, bounds.size.z) / 2f;
                orthographicSize = maxSize + 5f; // +5 für etwas Padding

                Debug.Log($"Auto-Detect: Kamera zentriert bei {cameraPosition}, Size: {orthographicSize}");
            }
            else
            {
                Debug.LogWarning($"Keine MapSceneInfo in Szene '{sceneName}' gefunden. Nutze manuelle Einstellungen.");
            }
        }

        camObj.transform.position = cameraPosition;
        camObj.transform.rotation = Quaternion.Euler(90, 0, 0); // Schaut nach unten

        cam.orthographic = true;
        cam.orthographicSize = orthographicSize;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = backgroundColor;
        cam.cullingMask = cullingMask;

        // RenderTexture erstellen
        RenderTexture rt = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        cam.targetTexture = rt;

        // Rendern
        cam.Render();

        // Texture2D erstellen und speichern
        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
        screenshot.Apply();

        // Als PNG speichern
        byte[] bytes = screenshot.EncodeToPNG();
        string filePath = Path.Combine(outputFolder, $"Map_{sceneName}.png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Screenshot gespeichert: {filePath}");

        // Aufräumen
        RenderTexture.active = null;
        cam.targetTexture = null;
        DestroyImmediate(rt);
        DestroyImmediate(screenshot);
        DestroyImmediate(camObj);

        return true;
    }

    // Helper für LayerMask Field
    private LayerMask LayerMaskField(string label, LayerMask layerMask)
    {
        var tempMask = EditorGUILayout.MaskField(label, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), InternalEditorUtility.layers);
        return InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
    }
}
