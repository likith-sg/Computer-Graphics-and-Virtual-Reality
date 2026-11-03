#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor menu helper for Experiment 10.
/// Menu: CGVR Labs -> Lab 10 — Run 3D Viewing Pipeline
/// </summary>
public static class Lab10SceneMenu
{
    [MenuItem("CGVR Labs/Lab 10 — Run 3D Viewing Pipeline")]
    public static void RunLab10()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.isPlaying = true;
    }

    [MenuItem("CGVR Labs/Lab 10 — Setup Viewing Pipeline Scene")]
    public static void SetupScene()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);

        var runner = new GameObject("Lab10_ViewingPipeline_Runner");
        runner.AddComponent<Lab10ViewingPipelineDemo>();

        EditorUtility.DisplayDialog("Lab 10: Scene Ready",
            "3D Viewing Pipeline Demo Scene has been set up!\n\n" +
            "Student: Likith S G | USN: 1RVU23CSE236\n\n" +
            "Press Play (▶) to view the Orthographic vs Perspective dual-viewport comparison.", "OK");
    }
}
#endif

