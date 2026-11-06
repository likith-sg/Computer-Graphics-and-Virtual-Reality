using UnityEngine;
using InteriorStudio.Core;

namespace InteriorStudio.Interaction
{
    public class FurnitureSelector : MonoBehaviour
    {
        public ScreenToWorldRaycaster raycaster;
        public GameObject selectionHighlightPrefab;

        private GameObject currentHighlightInstance;
        private GameObject selectedObject;

        private void Update()
        {
            // Click to select/deselect
            if (Input.GetMouseButtonDown(0))
            {
                // Check if pointer is over UI - in desktop or XR
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                if (raycaster != null)
                {
                    GameObject hitObj;
                    Vector3 hitPt;
                    if (raycaster.RaycastFurniture(Input.mousePosition, out hitObj, out hitPt))
                    {
                        Select(hitObj);
                    }
                }
            }

            // Update highlight bounding box position
            if (selectedObject != null && currentHighlightInstance != null)
            {
                currentHighlightInstance.transform.position = selectedObject.transform.position;
                currentHighlightInstance.transform.rotation = selectedObject.transform.rotation;
            }
        }

        public void Select(GameObject obj)
        {
            selectedObject = obj;
            if (StudioManager.Instance != null)
            {
                StudioManager.Instance.SelectFurniture(selectedObject);
            }

            UpdateHighlightVisual();
            Debug.Log($"[FurnitureSelector] Selected: {selectedObject?.name}");
        }

        public void Deselect()
        {
            selectedObject = null;
            if (StudioManager.Instance != null)
            {
                StudioManager.Instance.DeselectFurniture();
            }

            if (currentHighlightInstance != null)
            {
                Destroy(currentHighlightInstance);
                currentHighlightInstance = null;
            }
        }

        private void UpdateHighlightVisual()
        {
            if (currentHighlightInstance != null)
            {
                Destroy(currentHighlightInstance);
                currentHighlightInstance = null;
            }

            if (selectedObject == null) return;

            // Calculate bounding box
            Bounds bounds = CalculateObjectBounds(selectedObject);

            currentHighlightInstance = new GameObject("Selection_BoundingBox_Gizmo");
            currentHighlightInstance.transform.position = bounds.center;
            currentHighlightInstance.transform.rotation = selectedObject.transform.rotation;

            // Wireframe / corner markers
            CreateCornerMarkers(currentHighlightInstance, bounds.size);
        }

        private Bounds CalculateObjectBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one);
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        private void CreateCornerMarkers(GameObject parent, Vector3 size)
        {
            // Create a subtle cyan bounding halo indicator around the base of the selected furniture
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Selection_Halo_Ring";
            ring.transform.SetParent(parent.transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            ring.transform.localScale = new Vector3(Mathf.Max(size.x, size.z) * 1.25f, 0.005f, Mathf.Max(size.x, size.z) * 1.25f);

            var col = ring.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var r = ring.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = new Color(0.2f, 0.8f, 1.0f, 0.65f);
            r.material = mat;
        }
    }
}
