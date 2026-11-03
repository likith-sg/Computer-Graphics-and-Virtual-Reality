#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor menu helpers for Case Study 2:
/// Industrial Design and Architecture - VR Automotive Collaborative Review.
/// Menu: CGVR Case Study -> ...
/// 
/// Student: Likith S G   USN: 1RVU23CSE236
/// </summary>
public static class CaseStudySceneMenu
{
    [MenuItem("CGVR Case Study/Case Study 2 - Setup Design Studio Scene")]
    public static void SetupScene()
    {
        // Clear existing scene items except Camera
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.GetComponent<Camera>() != null) continue;
            Object.DestroyImmediate(go);
        }

        CaseStudyBootstrap.EnsureSceneSetup();

        EditorUtility.DisplayDialog("Case Study 2: VR Automotive Studio Ready",
            "1:1 Scale Automotive VR Collaborative Studio has been constructed!\n\n" +
            "Student: Likith S G | USN: 1RVU23CSE236\n\n" +
            "Features Included:\n" +
            "  - 5 Distinct 1:1 Scale Car Models (Sports, Supercar, Hypercar, Muscle, SUV)\n" +
            "  - Full Forza Horizon-Style Customization (Paint, Wheels, Body, Performance)\n" +
            "  - Wheel Rim Matrix Transformation Inspector (Q2 Matrix Demo)\n" +
            "  - Studio Environment with Reflective Floor & Softbox Lighting\n" +
            "  - VR 90 FPS Performance & Culling/Clipping Telemetry (Q3)\n\n" +
            "Press Play to enter interactive VR design review.", "OK");
    }

    [MenuItem("CGVR Case Study/Case Study 2 - Run VR Design Review")]
    public static void RunScene()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.isPlaying = true;
    }
}
#endif
