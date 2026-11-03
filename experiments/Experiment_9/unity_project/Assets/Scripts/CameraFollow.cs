using UnityEngine;

/// <summary>
/// Third-person camera following the player avatar.
/// Smoothly follows distance=5.2, height=2.8 behind the player.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 5.2f;
    public float height   = 2.8f;
    public float smoothSpeed = 8f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position - (target.forward * distance) + (Vector3.up * height);
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

        Vector3 lookTarget = target.position + (Vector3.up * (height * 0.45f));
        transform.LookAt(lookTarget);
    }
}

