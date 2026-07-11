using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

public static class CreateBallCustomScene
{
    [MenuItem("Tools/Create BallCustom Scene")]
    public static void Create()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Create BallCustom Scene",
                "Please exit Play Mode first.", "OK");
            return;
        }

        var scenePath = "Assets/Scenes/BallCustom.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera", typeof(Camera));
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0f, 0f, -3f);
        camGO.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        camGO.GetComponent<Camera>().backgroundColor = new Color(0.1f, 0.1f, 0.15f);

        // Light
        var lightGO = new GameObject("Directional Light", typeof(Light));
        lightGO.GetComponent<Light>().type = LightType.Directional;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem", typeof(EventSystem));
            var inputType = System.Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputType != null)
            {
                esGO.AddComponent(inputType);
            }
            else
            {
                Debug.LogWarning("InputSystemUIInputModule not found.");
            }
        }

        // BallCustomManager
        var managerGO = new GameObject("BallCustomManager");
        managerGO.AddComponent<BallCustomManager>();

        EditorSceneManager.SaveScene(scene, scenePath);

        // Build Settingsに追加
        var existing = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);
        if (!existing.Exists(s => s.path == scenePath))
            existing.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = existing.ToArray();

        Debug.Log("BallCustom scene created: " + scenePath);
    }
}
