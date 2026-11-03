using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// On-screen HUD displaying score, collection progress, controls, and student info.
/// Uses Unity OnGUI for zero-dependency, crash-proof rendering.
/// Student: Likith S G   USN: 1RVU23CSE236
/// </summary>
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    public int totalGems = 5;
    private int _score = 0;
    private List<Collectible> _collectibles = new List<Collectible>();

    private GUIStyle _boxStyle, _titleStyle, _infoStyle, _winStyle;
    private bool _stylesInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        _collectibles.AddRange(FindObjectsByType<Collectible>(FindObjectsSortMode.None));
        totalGems = _collectibles.Count > 0 ? _collectibles.Count : 5;
    }

    public void AddScore(int pts)
    {
        _score += pts;
    }

    public void ResetCollectibles()
    {
        _score = 0;
        foreach (var c in _collectibles)
            if (c != null) c.ResetState();
    }

    void InitStyles()
    {
        if (_stylesInitialized) return;

        Texture2D darkBg = new Texture2D(1, 1);
        darkBg.SetPixel(0, 0, new Color(0.08f, 0.10f, 0.14f, 0.85f));
        darkBg.Apply();

        _boxStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = darkBg, textColor = Color.white },
            padding = new RectOffset(12, 12, 10, 10)
        };

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.35f, 0.80f, 1.0f) }
        };

        _infoStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = new Color(0.90f, 0.90f, 0.90f) }
        };

        _winStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.25f, 1.0f, 0.40f) }
        };

        _stylesInitialized = true;
    }

    void OnGUI()
    {
        InitStyles();

        // Top-left HUD card
        GUILayout.BeginArea(new Rect(16, 16, 310, 190), _boxStyle);
        GUILayout.Label("CGVR Lab 9: Interactive 3D Scene", _titleStyle);
        GUILayout.Label("Student: Likith S G | USN: 1RVU23CSE236", _infoStyle);
        GUILayout.Space(6);

        GUILayout.Label(string.Format("Gems Collected: {0} / {1}", _score, totalGems), _infoStyle);

        GUILayout.Space(6);
        GUILayout.Label("Controls:", _titleStyle);
        GUILayout.Label("  W / S or Up / Down  : Move forward / back", _infoStyle);
        GUILayout.Label("  A / D or Left / Right: Turn left / right", _infoStyle);
        GUILayout.Label("  Space: Jump   |   R: Reset scene", _infoStyle);
        GUILayout.EndArea();

        // Win banner when all collected
        if (_score >= totalGems && totalGems > 0)
        {
            float w = 420; float h = 70;
            Rect centerRect = new Rect((Screen.width - w) * 0.5f, 20, w, h);
            GUILayout.BeginArea(centerRect, _boxStyle);
            GUILayout.Label("ALL GEMS COLLECTED! EXCELLENT!", _winStyle);
            GUILayout.Label("Press 'R' to play again", _infoStyle);
            GUILayout.EndArea();
        }
    }
}

