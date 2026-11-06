using UnityEngine;

namespace InteriorStudio.Interaction
{
    public class ScreenToWorldRaycaster : MonoBehaviour
    {
        [Header("Camera Reference")]
        public Camera targetCamera;

        [Header("Layer Masks")]
        public LayerMask furnitureLayerMask = ~0;
        public LayerMask floorLayerMask = ~0;

        [Header("Live Transformation Telemetry")]
        public Vector2 lastScreenPos;
        public Vector2 lastViewportPos;
        public Vector3 lastNdcPos;
        public Vector3 lastRayOrigin;
        public Vector3 lastRayDirection;
        public Vector3 lastFloorHitPoint;
        public bool hasFloorHit;

        private void Awake()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) targetCamera = GetComponent<Camera>();
        }

        /// <summary>
        /// Explicit implementation of Window-to-Viewport Transformation (Question 3):
        /// Maps screen window pixel coordinates (x_w, y_w) in [0, Screen.width] x [0, Screen.height]
        /// into normalized viewport coordinates (x_v, y_v) in [0, 1] x [0, 1].
        /// Formula: x_v = x_vmin + ((x_w - x_wmin) / (x_wmax - x_wmin)) * (x_vmax - x_vmin)
        /// </summary>
        public Vector2 ComputeWindowToViewport(Vector2 screenCoord, Rect windowRect, Rect viewportRect)
        {
            float xw = screenCoord.x;
            float yw = screenCoord.y;

            float xv = viewportRect.xMin + ((xw - windowRect.xMin) / (windowRect.xMax - windowRect.xMin)) * (viewportRect.xMax - viewportRect.xMin);
            float yv = viewportRect.yMin + ((yw - windowRect.yMin) / (windowRect.yMax - windowRect.yMin)) * (viewportRect.yMax - viewportRect.yMin);

            return new Vector2(xv, yv);
        }

        public Ray GenerateWorldRayFromScreen(Vector2 screenPos)
        {
            if (targetCamera == null) targetCamera = Camera.main;

            lastScreenPos = screenPos;

            // 1. Window to Viewport mapping
            Rect windowRect = new Rect(0, 0, Screen.width, Screen.height);
            Rect viewportRect = new Rect(0, 0, 1, 1);
            lastViewportPos = ComputeWindowToViewport(screenPos, windowRect, viewportRect);

            // 2. Normalized Device Coordinates (NDC) in [-1, 1]
            lastNdcPos = new Vector3(lastViewportPos.x * 2.0f - 1.0f, lastViewportPos.y * 2.0f - 1.0f, -1.0f);

            // 3. Camera Unprojection: Ray Origin O and Direction D
            Ray ray = targetCamera.ScreenPointToRay(screenPos);
            lastRayOrigin = ray.origin;
            lastRayDirection = ray.direction;

            return ray;
        }

        public bool RaycastFloorPlane(Vector2 screenPos, out Vector3 worldHitPoint)
        {
            Ray ray = GenerateWorldRayFromScreen(screenPos);

            // Mathematical Ray-Plane intersection for Y = 0 (floor plane)
            // Plane: n dot (P - P0) = 0 where n = (0, 1, 0), P0 = (0, 0, 0)
            // Ray: P(t) = O + t * D
            // t = (n dot (P0 - O)) / (n dot D) = -O.y / D.y
            if (Mathf.Abs(ray.direction.y) > 0.0001f)
            {
                float t = -ray.origin.y / ray.direction.y;
                if (t > 0f)
                {
                    worldHitPoint = ray.origin + ray.direction * t;
                    lastFloorHitPoint = worldHitPoint;
                    hasFloorHit = true;
                    return true;
                }
            }

            // Fallback to physics raycast if available
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, floorLayerMask))
            {
                worldHitPoint = hit.point;
                lastFloorHitPoint = worldHitPoint;
                hasFloorHit = true;
                return true;
            }

            worldHitPoint = Vector3.zero;
            hasFloorHit = false;
            return false;
        }

        public bool RaycastFurniture(Vector2 screenPos, out GameObject hitObject, out Vector3 hitPoint)
        {
            Ray ray = GenerateWorldRayFromScreen(screenPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, furnitureLayerMask))
            {
                // Traverse up to find root furniture object
                Transform current = hit.transform;
                while (current.parent != null && current.parent.name != "Scandinavian_LivingRoom_Architecture" && current.parent.name != "StudioManager")
                {
                    current = current.parent;
                }
                hitObject = current.gameObject;
                hitPoint = hit.point;
                return true;
            }

            hitObject = null;
            hitPoint = Vector3.zero;
            return false;
        }

        public string GetTelemetrySummary()
        {
            return $"Window: ({lastScreenPos.x:F0}, {lastScreenPos.y:F0})px\n" +
                   $"Viewport: ({lastViewportPos.x:F3}, {lastViewportPos.y:F3})\n" +
                   $"NDC: ({lastNdcPos.x:F2}, {lastNdcPos.y:F2}, {lastNdcPos.z:F2})\n" +
                   $"Ray Origin: ({lastRayOrigin.x:F2}, {lastRayOrigin.y:F2}, {lastRayOrigin.z:F2})\n" +
                   $"Ray Dir: ({lastRayDirection.x:F2}, {lastRayDirection.y:F2}, {lastRayDirection.z:F2})\n" +
                   $"Floor Intersect: ({lastFloorHitPoint.x:F2}, {lastFloorHitPoint.y:F2}, {lastFloorHitPoint.z:F2})";
        }
    }
}
