#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor menu helper to build or reset the Lab 9 scene in one click.
/// Menu: CGVR Labs -> Lab 9 — Setup Interactive 3D Scene
/// </summary>
public static class LabSceneAutoSetup
{
    [MenuItem("CGVR Labs/Lab 9 — Setup Interactive 3D Scene")]
    public static void SetupScene()
    {
        // Clear old scene objects
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.GetComponent<Camera>() != null) continue;
            Object.DestroyImmediate(go);
        }

        Lab9Bootstrap.EnsureSceneSetup();

        EditorUtility.DisplayDialog("Lab 9: Scene Ready",
            "Interactive 3D Scene has been generated successfully!\n\n" +
            "Student: Likith S G | USN: 1RVU23CSE236\n\n" +
            "Press Play (▶) to move with WASD/Arrows and collect all 5 gems.", "OK");
    }
}
#endif

