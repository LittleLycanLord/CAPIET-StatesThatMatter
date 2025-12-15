using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    public class ScreenContainerGenerator : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Fields")]
        [SerializeField] private Camera orthographicCamera;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake() { }

        void Start() 
        {
            CreateViewportBoundaries();
        }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        private void CreateViewportBoundaries()
        {
            Assert.IsNotNull(orthographicCamera, "Orthographic camera is not assigned!");
            Assert.IsTrue(orthographicCamera.orthographic, "Camera must be orthographic!");

            float cameraHeight = orthographicCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * orthographicCamera.aspect;
            float depth = orthographicCamera.nearClipPlane + 1f;
            float wallThickness = 1f;

            Vector3 cameraPosition = orthographicCamera.transform.position;

            // Top wall
            CreateInvisibleWall("TopWall", 
                new Vector3(cameraPosition.x, cameraPosition.y + cameraHeight / 2f + wallThickness / 2f, depth),
                new Vector3(cameraWidth, wallThickness, wallThickness));

            // Bottom wall
            CreateInvisibleWall("BottomWall", 
                new Vector3(cameraPosition.x, cameraPosition.y - cameraHeight / 2f - wallThickness / 2f, depth),
                new Vector3(cameraWidth, wallThickness, wallThickness));

            // Left wall
            CreateInvisibleWall("LeftWall", 
                new Vector3(cameraPosition.x - cameraWidth / 2f - wallThickness / 2f, cameraPosition.y, depth),
                new Vector3(wallThickness, cameraHeight, wallThickness));

            // Right wall
            CreateInvisibleWall("RightWall", 
                new Vector3(cameraPosition.x + cameraWidth / 2f + wallThickness / 2f, cameraPosition.y, depth),
                new Vector3(wallThickness, cameraHeight, wallThickness));
        }

        private void CreateInvisibleWall(string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.transform.parent = transform;

            // Make invisible by removing the renderer
            Destroy(wall.GetComponent<Renderer>());
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}