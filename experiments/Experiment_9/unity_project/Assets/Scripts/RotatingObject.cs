using UnityEngine;

/// <summary>
/// Continuous multi-axis rotation component for obstacles and decorative elements.
/// Matches Python obstacle rotation.
/// </summary>
public class RotatingObject : MonoBehaviour
{
    public Vector3 rotationAxis = new Vector3(0.5f, 1f, 0.3f);
    public float rotationSpeed = 35f;

    void Update()
    {
        transform.Rotate(rotationAxis.normalized, rotationSpeed * Time.deltaTime, Space.Self);
    }
}

