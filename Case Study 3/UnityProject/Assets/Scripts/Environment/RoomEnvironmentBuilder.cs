using UnityEngine;

namespace InteriorStudio.Environment
{
    [ExecuteInEditMode]
    public class RoomEnvironmentBuilder : MonoBehaviour
    {
        [Header("Room Dimensions")]
        public float roomWidth = 8.0f;   // X-axis (-4 to +4)
        public float roomLength = 6.0f;  // Z-axis (-3 to +3)
        public float roomHeight = 3.2f;  // Y-axis (0 to 3.2)

        [Header("Materials")]
        public Material floorMaterial;
        public Material wallMaterial;
        public Material ceilingMaterial;
        public Material windowFrameMaterial;
        public Material glassMaterial;
        public Material rugMaterial;
        public Material wallArtMaterial;
        public Material baseboardMaterial;

        [Header("Generated References")]
        public GameObject roomRoot;

        public void BuildLivingRoom()
        {
            if (roomRoot != null)
            {
                if (Application.isPlaying) Destroy(roomRoot);
                else DestroyImmediate(roomRoot);
            }

            roomRoot = new GameObject("Scandinavian_LivingRoom_Architecture");
            roomRoot.transform.SetParent(transform, false);

            BuildFloor();
            BuildCeiling();
            BuildWalls();
            BuildWindow();
            BuildAreaRug();
            BuildWallArt();
            BuildBaseboards();

            Debug.Log("[RoomEnvironmentBuilder] Scandinavian living room architecture generated.");
        }

        private void BuildFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Architectural_Floor_Parquet";
            floor.transform.SetParent(roomRoot.transform, false);
            // Unity default plane is 10m x 10m
            floor.transform.localScale = new Vector3(roomWidth / 10f, 1f, roomLength / 10f);
            floor.transform.position = Vector3.zero;

            var r = floor.GetComponent<MeshRenderer>();
            if (floorMaterial != null) r.sharedMaterial = floorMaterial;

            // Ensure collider is configured for floor raycasts
            var col = floor.GetComponent<MeshCollider>();
            if (col == null) col = floor.AddComponent<MeshCollider>();
            floor.layer = LayerMask.NameToLayer("Default");
        }

        private void BuildCeiling()
        {
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ceiling.name = "Architectural_Ceiling";
            ceiling.transform.SetParent(roomRoot.transform, false);
            ceiling.transform.localScale = new Vector3(roomWidth / 10f, 1f, roomLength / 10f);
            ceiling.transform.position = new Vector3(0f, roomHeight, 0f);
            ceiling.transform.rotation = Quaternion.Euler(180f, 0f, 0f); // Face downwards

            var r = ceiling.GetComponent<MeshRenderer>();
            if (ceilingMaterial != null) r.sharedMaterial = ceilingMaterial;
            else if (wallMaterial != null) r.sharedMaterial = wallMaterial;
        }

        private void BuildWalls()
        {
            // North (Back) Wall
            CreateWall("Wall_North_Back", 
                new Vector3(0f, roomHeight * 0.5f, roomLength * 0.5f), 
                new Vector3(roomWidth, roomHeight, 0.15f));

            // West (Left) Wall
            CreateWall("Wall_West_Left", 
                new Vector3(-roomWidth * 0.5f, roomHeight * 0.5f, 0f), 
                new Vector3(0.15f, roomHeight, roomLength));

            // East (Right) Wall
            CreateWall("Wall_East_Right", 
                new Vector3(roomWidth * 0.5f, roomHeight * 0.5f, 0f), 
                new Vector3(0.15f, roomHeight, roomLength));

            // South (Front) Wall Piers - surrounds large window aperture
            float windowWidth = 4.8f;
            float sidePierWidth = (roomWidth - windowWidth) * 0.5f;

            // Left pier
            CreateWall("Wall_South_LeftPier", 
                new Vector3(-roomWidth * 0.5f + sidePierWidth * 0.5f, roomHeight * 0.5f, -roomLength * 0.5f), 
                new Vector3(sidePierWidth, roomHeight, 0.15f));

            // Right pier
            CreateWall("Wall_South_RightPier", 
                new Vector3(roomWidth * 0.5f - sidePierWidth * 0.5f, roomHeight * 0.5f, -roomLength * 0.5f), 
                new Vector3(sidePierWidth, roomHeight, 0.15f));

            // Header lintel above window
            CreateWall("Wall_South_WindowLintel", 
                new Vector3(0f, roomHeight - 0.2f, -roomLength * 0.5f), 
                new Vector3(windowWidth, 0.4f, 0.15f));
        }

        private void CreateWall(string name, Vector3 pos, Vector3 size)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(roomRoot.transform, false);
            wall.transform.position = pos;
            wall.transform.localScale = size;

            var r = wall.GetComponent<MeshRenderer>();
            if (wallMaterial != null) r.sharedMaterial = wallMaterial;
        }

        private void BuildWindow()
        {
            GameObject windowRoot = new GameObject("Panoramic_Balcony_Window");
            windowRoot.transform.SetParent(roomRoot.transform, false);
            windowRoot.transform.position = new Vector3(0f, 1.4f, -roomLength * 0.5f);

            // Large glass pane
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "Window_Glass";
            glass.transform.SetParent(windowRoot.transform, false);
            glass.transform.localScale = new Vector3(4.7f, 2.6f, 0.03f);

            var r = glass.GetComponent<MeshRenderer>();
            if (glassMaterial != null) r.sharedMaterial = glassMaterial;

            // Black architectural frame mullions
            CreateFrameMullion(windowRoot, new Vector3(0f, 1.3f, 0f), new Vector3(4.8f, 0.08f, 0.08f));  // Top
            CreateFrameMullion(windowRoot, new Vector3(0f, -1.3f, 0f), new Vector3(4.8f, 0.08f, 0.08f)); // Bottom sill
            CreateFrameMullion(windowRoot, new Vector3(-2.35f, 0f, 0f), new Vector3(0.08f, 2.6f, 0.08f)); // Left
            CreateFrameMullion(windowRoot, new Vector3(2.35f, 0f, 0f), new Vector3(0.08f, 2.6f, 0.08f));  // Right
            CreateFrameMullion(windowRoot, new Vector3(-0.8f, 0f, 0f), new Vector3(0.05f, 2.6f, 0.07f));  // Vertical mullion 1
            CreateFrameMullion(windowRoot, new Vector3(0.8f, 0f, 0f), new Vector3(0.05f, 2.6f, 0.07f));   // Vertical mullion 2
        }

        private void CreateFrameMullion(GameObject parent, Vector3 localPos, Vector3 size)
        {
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame_Mullion";
            frame.transform.SetParent(parent.transform, false);
            frame.transform.localPosition = localPos;
            frame.transform.localScale = size;

            var r = frame.GetComponent<MeshRenderer>();
            if (windowFrameMaterial != null) r.sharedMaterial = windowFrameMaterial;
        }

        private void BuildAreaRug()
        {
            GameObject rug = GameObject.CreatePrimitive(PrimitiveType.Plane);
            rug.name = "Scandinavian_AreaRug_Diamond";
            rug.transform.SetParent(roomRoot.transform, false);
            // 3.2m x 2.4m rug (0.32 x 0.24 plane scale)
            rug.transform.localScale = new Vector3(0.32f, 1f, 0.24f);
            rug.transform.position = new Vector3(0f, 0.005f, 0.2f); // Just above floor to prevent z-fighting

            var r = rug.GetComponent<MeshRenderer>();
            if (rugMaterial != null) r.sharedMaterial = rugMaterial;

            // Remove collider so clicks hit the floor plane underneath
            var col = rug.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
        }

        private void BuildWallArt()
        {
            GameObject artRoot = new GameObject("Scandinavian_Framed_WallArt");
            artRoot.transform.SetParent(roomRoot.transform, false);
            artRoot.transform.position = new Vector3(0f, 2.05f, roomLength * 0.5f - 0.06f);

            // Frame (light oak or black metal)
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Art_Frame";
            frame.transform.SetParent(artRoot.transform, false);
            frame.transform.localScale = new Vector3(1.65f, 1.15f, 0.04f);
            var rf = frame.GetComponent<MeshRenderer>();
            if (windowFrameMaterial != null) rf.sharedMaterial = windowFrameMaterial;

            // Canvas print
            GameObject canvas = GameObject.CreatePrimitive(PrimitiveType.Quad);
            canvas.name = "Art_Canvas";
            canvas.transform.SetParent(artRoot.transform, false);
            canvas.transform.localPosition = new Vector3(0f, 0f, -0.025f);
            canvas.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            canvas.transform.localScale = new Vector3(1.5f, 1.0f, 1f);
            var rc = canvas.GetComponent<MeshRenderer>();
            if (wallArtMaterial != null) rc.sharedMaterial = wallArtMaterial;
        }

        private void BuildBaseboards()
        {
            // Subtle 8cm architectural baseboard skirting
            CreateBaseboard("Baseboard_North", new Vector3(0f, 0.04f, roomLength * 0.5f - 0.04f), new Vector3(roomWidth, 0.08f, 0.04f));
            CreateBaseboard("Baseboard_West", new Vector3(-roomWidth * 0.5f + 0.04f, 0.04f, 0f), new Vector3(0.04f, 0.08f, roomLength));
            CreateBaseboard("Baseboard_East", new Vector3(roomWidth * 0.5f - 0.04f, 0.04f, 0f), new Vector3(0.04f, 0.08f, roomLength));
        }

        private void CreateBaseboard(string name, Vector3 pos, Vector3 size)
        {
            GameObject bb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bb.name = name;
            bb.transform.SetParent(roomRoot.transform, false);
            bb.transform.position = pos;
            bb.transform.localScale = size;
            var r = bb.GetComponent<MeshRenderer>();
            if (baseboardMaterial != null) r.sharedMaterial = baseboardMaterial;
            else if (wallMaterial != null) r.sharedMaterial = wallMaterial;
        }
    }
}
