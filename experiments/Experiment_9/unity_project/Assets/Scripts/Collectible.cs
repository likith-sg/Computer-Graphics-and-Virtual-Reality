using UnityEngine;

/// <summary>
/// Collectible gem/coin object. Rotates and bobs up and down.
/// Triggers HUD score increase and collection sound/effect when touched.
/// </summary>
public class Collectible : MonoBehaviour
{
    public float bobSpeed     = 2.2f;
    public float bobHeight    = 0.25f;
    public float spinSpeed    = 90f;
    public int   pointValue   = 1;

    private Vector3 _basePosition;
    private bool    _collected = false;

    void Start()
    {
        _basePosition = transform.position;
    }

    void Update()
    {
        if (_collected) return;

        // Bob up & down + spin
        float newY = _basePosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _collected = true;
            GameHUD.Instance?.AddScore(pointValue);
            gameObject.SetActive(false);
        }
    }

    public void ResetState()
    {
        _collected = false;
        transform.position = _basePosition;
        gameObject.SetActive(true);
    }
}

