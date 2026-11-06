using UnityEngine;
using InteriorStudio.Core;

namespace InteriorStudio.Interaction
{
    public class FurnitureManipulator : MonoBehaviour
    {
        public ScreenToWorldRaycaster raycaster;
        public FurnitureSelector selector;

        [Header("Room Boundaries for Manipulation")]
        public float minX = -3.5f;
        public float maxX = 3.5f;
        public float minZ = -2.5f;
        public float maxZ = 2.5f;

        private bool isDraggingMove = false;
        private Vector3 dragOffset;

        private void Update()
        {
            if (StudioManager.Instance == null || StudioManager.Instance.selectedFurnitureObject == null) return;
            var target = StudioManager.Instance.selectedFurnitureObject;

            // Middle mouse drag or Shift+Left Click to drag move
            if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftShift))
            {
                if (raycaster != null && raycaster.RaycastFloorPlane(Input.mousePosition, out Vector3 floorHit))
                {
                    isDraggingMove = true;
                    dragOffset = target.transform.position - floorHit;
                }
            }

            if (isDraggingMove && Input.GetMouseButton(1))
            {
                if (raycaster != null && raycaster.RaycastFloorPlane(Input.mousePosition, out Vector3 floorHit))
                {
                    Vector3 newPos = floorHit + dragOffset;
                    newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
                    newPos.y = 0f; // Keep on floor plane
                    newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
                    target.transform.position = newPos;
                }
            }

            if (Input.GetMouseButtonUp(1))
            {
                isDraggingMove = false;
            }

            // Keyboard shortcut rotations: Q / E to rotate
            if (Input.GetKey(KeyCode.Q)) RotateSelected(-45f * Time.deltaTime);
            if (Input.GetKey(KeyCode.E)) RotateSelected(45f * Time.deltaTime);
        }

        public void MoveSelected(Vector3 delta)
        {
            if (StudioManager.Instance == null || StudioManager.Instance.selectedFurnitureObject == null) return;
            var target = StudioManager.Instance.selectedFurnitureObject;
            Vector3 pos = target.transform.position + delta;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = 0f;
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
            target.transform.position = pos;
        }

        public void SetSelectedPosition(Vector3 newPos)
        {
            if (StudioManager.Instance == null || StudioManager.Instance.selectedFurnitureObject == null) return;
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = 0f;
            newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
            StudioManager.Instance.selectedFurnitureObject.transform.position = newPos;
        }

        public void RotateSelected(float angleDegrees)
        {
            if (StudioManager.Instance == null || StudioManager.Instance.selectedFurnitureObject == null) return;
            var target = StudioManager.Instance.selectedFurnitureObject;
            target.transform.Rotate(Vector3.up, angleDegrees, Space.World);
        }

        public void SetSelectedRotationY(float angleDegrees)
        {
            if (StudioManager.Instance == null || StudioManager.Instance.selectedFurnitureObject == null) return;
            var target = StudioManager.Instance.selectedFurnitureObject;
            target.transform.rotation = Quaternion.Euler(0f, angleDegrees, 0f);
        }

        public void SetSelectedScale(float uniformScale)
        {
            if (StudioManager.Instance == null || StudioManager.Instance.selectedFurnitureObject == null) return;
            var target = StudioManager.Instance.selectedFurnitureObject;
            float s = Mathf.Clamp(uniformScale, 0.5f, 2.0f);
            target.transform.localScale = Vector3.one * s;
        }

        public void ResetSelectedTransform()
        {
            if (StudioManager.Instance == null || StudioManager.Instance.selectedFurnitureObject == null) return;
            var target = StudioManager.Instance.selectedFurnitureObject;
            target.transform.position = new Vector3(0f, 0f, 0f);
            target.transform.rotation = Quaternion.identity;
            target.transform.localScale = Vector3.one;
            Debug.Log($"[FurnitureManipulator] Transform reset for {target.name}");
        }
    }
}
