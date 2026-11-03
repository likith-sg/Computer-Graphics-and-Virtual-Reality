using UnityEngine;

/// <summary>
/// Case Study 2 — VR Rendering Performance Telemetry
///
/// Q3: Frame budget, HSR pipeline, clipping, VR stereo data.
/// Student: Likith S G   USN: 1RVU23CSE236
/// </summary>
public class VROptimizationTelemetry : MonoBehaviour
{
    public float fps                    = 90.0f;
    public float frameTimeMs            = 11.1f;
    public float motionToPhotonMs       = 14.2f;

    public bool singlePassStereoEnabled = true;
    public bool backFaceCullingEnabled  = true;
    public bool earlyZBufferEnabled     = true;
    public bool occlusionCullingEnabled = true;
    public bool foveatedRenderingEnabled = true;

    private float _smooth = 0f;

    void Update()
    {
        _smooth      += (Time.unscaledDeltaTime - _smooth) * 0.10f;
        fps           = 1f / Mathf.Max(0.0001f, _smooth);
        frameTimeMs   = _smooth * 1000f;
        motionToPhotonMs = frameTimeMs + 3.2f;
    }
}
