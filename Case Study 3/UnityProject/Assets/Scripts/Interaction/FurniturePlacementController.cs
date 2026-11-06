using UnityEngine;
using InteriorStudio.Core;

namespace InteriorStudio.Interaction
{
    public class FurniturePlacementController : MonoBehaviour
    {
        public ScreenToWorldRaycaster raycaster;
        public FurnitureSelector selector;

        [Header("Placement Settings")]
        public bool isPlacing = false;
        public FurnitureItem currentItemToPlace;
        public bool enableGridSnapping = false;
        public float gridSize = 0.25f;

        private GameObject previewInstance;
        private Material previewValidMaterial;
        private Material previewInvalidMaterial;

        private void Awake()
        {
            previewValidMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            previewValidMaterial.color = new Color(0.2f, 0.85f, 0.35f, 0.45f);

            previewInvalidMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            previewInvalidMaterial.color = new Color(0.95f, 0.25f, 0.25f, 0.45f);
        }

        private void Update()
        {
            if (!isPlacing || previewInstance == null) return;

            // Follow mouse raycast on floor
            if (raycaster != null && raycaster.RaycastFloorPlane(Input.mousePosition, out Vector3 floorHit))
            {
                Vector3 placePos = floorHit;
                if (enableGridSnapping)
                {
                    placePos.x = Mathf.Round(placePos.x / gridSize) * gridSize;
                    placePos.z = Mathf.Round(placePos.z / gridSize) * gridSize;
                }

                // Clamp inside living room boundaries
                placePos.x = Mathf.Clamp(placePos.x, -3.5f, 3.5f);
                placePos.y = 0f;
                placePos.z = Mathf.Clamp(placePos.z, -2.5f, 2.5f);
                previewInstance.transform.position = placePos;

                // Rotate preview with scroll wheel or R key
                if (Input.GetKey(KeyCode.R)) previewInstance.transform.Rotate(Vector3.up, 60f * Time.deltaTime);

                // Confirm placement on left click
                if (Input.GetMouseButtonDown(0))
                {
                    if (UnityEngine.EventSystems.EventSystem.current == null || 
                        !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    {
                        ConfirmPlacement(placePos, previewInstance.transform.rotation);
                    }
                }

                // Cancel placement on Escape or Right Click
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
                {
                    CancelPlacement();
                }
            }
        }

        public void StartPlacement(FurnitureItem item)
        {
            if (item == null || item.prefab == null) return;

            CancelPlacement();

            currentItemToPlace = item;
            isPlacing = true;

            // Create ghost preview
            previewInstance = Instantiate(item.prefab, Vector3.zero, Quaternion.identity);
            previewInstance.name = "Preview_" + item.displayName;

            // Apply semi-transparent ghost material to all preview meshes
            foreach (var r in previewInstance.GetComponentsInChildren<MeshRenderer>())
            {
                r.material = previewValidMaterial;
            }

            // Disable colliders on preview
            foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            Debug.Log($"[FurniturePlacementController] Started placement for: {item.displayName}");
        }

        public void ConfirmPlacement(Vector3 position, Quaternion rotation)
        {
            if (currentItemToPlace == null || currentItemToPlace.prefab == null) return;

            GameObject spawned = Instantiate(currentItemToPlace.prefab, position, rotation);
            spawned.name = currentItemToPlace.displayName;

            if (StudioManager.Instance != null)
            {
                StudioManager.Instance.RegisterPlacedObject(spawned);
                if (selector != null) selector.Select(spawned);
            }

            CancelPlacement();
            Debug.Log($"[FurniturePlacementController] Placed {spawned.name} at {position}");
        }

        public void CancelPlacement()
        {
            if (previewInstance != null)
            {
                Destroy(previewInstance);
                previewInstance = null;
            }
            isPlacing = false;
            currentItemToPlace = null;
        }
    }
}
