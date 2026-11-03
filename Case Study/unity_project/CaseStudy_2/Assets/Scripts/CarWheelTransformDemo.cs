using UnityEngine;

/// <summary>
/// Case Study 2 — Wheel Rim Homogeneous Transformation Demo
///
/// Q2: Scale=1.5, Rotate=45°, Translate=50mm forward.
/// M = T(50mm) · R(45°) · S(1.5)   [Column-vector convention, local-to-world]
///
/// Student: Likith S G   USN: 1RVU23CSE236
/// Course:  Computer Graphics and Virtual Reality Laboratory
/// </summary>
public class CarWheelTransformDemo : MonoBehaviour
{
    [Header("Q2 Target Parameters")]
    public float targetScale          = 1.5f;
    public float targetRotationDeg    = 45.0f;
    public float targetTranslationMm  = 50.0f;

    [Header("Interactive Controls")]
    [Range(0.8f, 2.2f)] public float scaleFactor    = 1.0f;
    [Range(0f, 360f)]   public float rotationAngle  = 0.0f;
    [Range(-20f, 150f)] public float translationMm  = 0.0f;

    public bool animateLoop          = false;
    public bool showCoordinateAxes   = true;

    public enum RotationAxis { Z_Rolling, Y_Steering }
    public RotationAxis selectedAxis = RotationAxis.Z_Rolling;

    // ── private state ──────────────────────────────────────────────────────
    private Vector3    _origPos;
    private Quaternion _origRot;
    private Vector3    _origScale;

    private Matrix4x4  _S, _R, _T, _M;

    private LineRenderer _axX, _axY, _axZ;

    // ── lifecycle ──────────────────────────────────────────────────────────
    void Start()
    {
        _origPos   = transform.localPosition;
        _origRot   = transform.localRotation;
        _origScale = transform.localScale;

        BuildAxes();
        Recompute();
    }

    void Update()
    {
        if (animateLoop)
        {
            float t = (Mathf.Sin(Time.time * 2f) + 1f) * 0.5f;
            scaleFactor   = Mathf.Lerp(1.0f, targetScale,         t);
            rotationAngle = Mathf.Lerp(0f,   targetRotationDeg,   t);
            translationMm = Mathf.Lerp(0f,   targetTranslationMm, t);
        }

        Recompute();
        ApplyTransform();
        RefreshAxes();
    }

    // ── public API ─────────────────────────────────────────────────────────
    public void ApplyPresetQ2Transformation()
    {
        animateLoop   = false;
        scaleFactor   = targetScale;
        rotationAngle = targetRotationDeg;
        translationMm = targetTranslationMm;
    }

    public void ResetTransformation()
    {
        animateLoop   = false;
        scaleFactor   = 1.0f;
        rotationAngle = 0.0f;
        translationMm = 0.0f;
    }

    /// <summary>Returns composite M with translation in mm (for on-screen display).</summary>
    public Matrix4x4 GetCompositeMatrixMm()
    {
        Matrix4x4 tMm = Matrix4x4.Translate(new Vector3(0, 0, translationMm));
        return tMm * _R * _S;
    }

    // ── private ────────────────────────────────────────────────────────────
    private void Recompute()
    {
        _S = Matrix4x4.Scale(Vector3.one * scaleFactor);
        _R = selectedAxis == RotationAxis.Z_Rolling
            ? Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotationAngle))
            : Matrix4x4.Rotate(Quaternion.Euler(0, rotationAngle, 0));
        float tz = translationMm / 1000f;
        _T = Matrix4x4.Translate(new Vector3(0, 0, tz));
        _M = _T * _R * _S;
    }

    private void ApplyTransform()
    {
        float tz = translationMm / 1000f;
        transform.localPosition = _origPos + new Vector3(0, 0, tz);
        transform.localRotation = selectedAxis == RotationAxis.Z_Rolling
            ? _origRot * Quaternion.Euler(0, 0, rotationAngle)
            : _origRot * Quaternion.Euler(0, rotationAngle, 0);
        transform.localScale = Vector3.Scale(_origScale, Vector3.one * scaleFactor);
    }

    // ── coordinate axes ────────────────────────────────────────────────────
    private void BuildAxes()
    {
        GameObject axRoot = new GameObject("CoordTripod");
        axRoot.transform.SetParent(transform, false);
        _axX = MakeAxisLine(axRoot, "Ax_X", Color.red);
        _axY = MakeAxisLine(axRoot, "Ax_Y", Color.green);
        _axZ = MakeAxisLine(axRoot, "Ax_Z", new Color(0.2f, 0.6f, 1f));
    }

    private LineRenderer MakeAxisLine(GameObject parent, string name, Color col)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        Shader s = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        Material m = new Material(s);
        m.color = col;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
        lr.material = m;
        lr.startWidth = 0.010f; lr.endWidth = 0.003f;
        lr.startColor = col; lr.endColor = col;
        lr.positionCount = 2;
        lr.useWorldSpace = false;
        return lr;
    }

    private void RefreshAxes()
    {
        bool show = showCoordinateAxes;
        if (_axX) _axX.gameObject.SetActive(show);
        if (_axY) _axY.gameObject.SetActive(show);
        if (_axZ) _axZ.gameObject.SetActive(show);
        if (!show) return;
        float len = 0.35f * scaleFactor;
        _axX.SetPosition(0, Vector3.zero); _axX.SetPosition(1, new Vector3(len, 0, 0));
        _axY.SetPosition(0, Vector3.zero); _axY.SetPosition(1, new Vector3(0, len, 0));
        _axZ.SetPosition(0, Vector3.zero); _axZ.SetPosition(1, new Vector3(0, 0, len));
    }
}
