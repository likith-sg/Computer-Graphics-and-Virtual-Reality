using UnityEngine;

/// <summary>
/// Experiment 9 — Simple Interactive 3D Scene
/// Player movement, rotation, jump physics — mirrors the Python/OpenGL implementation.
/// Controls: WASD / Arrow keys to move & turn, Space to jump, R to reset, Esc to quit.
///
/// Physics matches Python:
///   move speed  = 7 m/s  (MOVE_SPEED)
///   turn speed  = 120 °/s (ROT_SPEED)
///   jump speed  = 8.5 m/s (JUMP_IMPULSE)
///   gravity     = 24 m/s² (GRAVITY)
///
/// Student: Likith S G   USN: 1RVU23CSE236
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float turnSpeed = 120f;

    [Header("Jump / Gravity")]
    public float jumpSpeed = 8.5f;
    public float gravity   = 24f;

    [Header("Arena Bounds")]
    public float arenaBound = 13f;

    [Header("Reset")]
    public Vector3 spawnPosition = new Vector3(0f, 1f, 0f);

    private CharacterController _cc;
    private float _verticalVelocity = 0f;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = spawnPosition;
            transform.rotation = Quaternion.identity;
            _verticalVelocity  = 0f;
            GameHUD.Instance?.ResetCollectibles();
        }

        // Turn (A/D or Left/Right arrow)
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  turnInput -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) turnInput += 1f;
        transform.Rotate(Vector3.up, turnInput * turnSpeed * Time.deltaTime);

        // Forward / Backward (W/S or Up/Down arrow)
        float forwardInput = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))   forwardInput += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) forwardInput -= 1f;

        Vector3 move = transform.forward * (forwardInput * moveSpeed);

        // Gravity & Jump
        if (_cc.isGrounded)
        {
            if (_verticalVelocity < 0f) _verticalVelocity = -2f;
            if (Input.GetKeyDown(KeyCode.Space))
                _verticalVelocity = jumpSpeed;
        }
        else
        {
            _verticalVelocity -= gravity * Time.deltaTime;
        }

        move.y = _verticalVelocity;
        _cc.Move(move * Time.deltaTime);

        // Clamp to arena bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -arenaBound, arenaBound);
        pos.z = Mathf.Clamp(pos.z, -arenaBound, arenaBound);
        if (pos.y < -5f)
        {
            pos = spawnPosition;
            _verticalVelocity = 0f;
        }
        transform.position = pos;
    }
}

