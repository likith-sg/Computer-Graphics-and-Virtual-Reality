using UnityEngine;
using InteriorStudio.Core;

namespace InteriorStudio.Interaction
{
    public class CameraNavigation : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 4.0f;
        public float lookSensitivity = 2.5f;
        public float zoomSensitivity = 5.0f;

        [Header("Room Collision Clamping")]
        public float minX = -3.6f;
        public float maxX = 3.6f;
        public float minZ = -2.6f;
        public float maxZ = 2.6f;
        public float minY = 0.8f;
        public float maxY = 2.4f;

        private float pitch = 15f;
        private float yaw = -25f;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;

            Vector3 angles = transform.eulerAngles;
            pitch = angles.x;
            yaw = angles.y;
        }

        private void Update()
        {
            // Right-click drag to rotate camera view
            if (Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftShift))
            {
                float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

                yaw += mouseX;
                pitch -= mouseY;
                pitch = Mathf.Clamp(pitch, -45f, 75f);

                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }

            // Keyboard WASD / Arrow movement
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                Vector3 forward = transform.forward;
                forward.y = 0f;
                forward.Normalize();

                Vector3 right = transform.right;
                right.y = 0f;
                right.Normalize();

                Vector3 moveDir = forward * v + right * h;
                Vector3 newPos = transform.position + moveDir * moveSpeed * Time.deltaTime;

                newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
                newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
                newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);

                transform.position = newPos;
            }

            // Focus on selected furniture on F key
            if (Input.GetKeyDown(KeyCode.F))
            {
                FocusOnSelected();
            }

            // Scroll wheel zoom (adjusts FOV or dollies)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f && cam != null)
            {
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll * zoomSensitivity, 30f, 85f);
            }
        }

        public void FocusOnSelected()
        {
            if (StudioManager.Instance != null && StudioManager.Instance.selectedFurnitureObject != null)
            {
                Transform target = StudioManager.Instance.selectedFurnitureObject.transform;
                Vector3 focusPoint = target.position + Vector3.up * 0.4f;
                transform.LookAt(focusPoint);
                Vector3 angles = transform.eulerAngles;
                pitch = angles.x;
                yaw = angles.y;
                Debug.Log($"[CameraNavigation] Focused camera on: {target.name}");
            }
        }

        public void SetCameraPose(Vector3 position, Vector3 lookTarget)
        {
            transform.position = position;
            transform.LookAt(lookTarget);
            Vector3 angles = transform.eulerAngles;
            pitch = angles.x;
            yaw = angles.y;
        }
    }
}
