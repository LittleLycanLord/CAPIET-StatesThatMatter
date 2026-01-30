using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace LilLycanLord_Official
{
    public class TilemapToGameObjects : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        public Tilemap tilemap;
        public GameObject blockParentPrefab; // Empty prefab with Rigidbody2D + Collider
        public Transform container; // Optional parent
        public GameObject interactionButton;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Fields")]
        public ParticleLatticeMaterial material;
        
        [Space(10)]
        [Header("Highlight Settings")]
        [SerializeField] private Color glowColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float glowScale = 1.15f;
        [SerializeField] private Color outlineColor = Color.white;
        [SerializeField] private float outlineThickness = 0.1f;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        private HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        static readonly Vector3Int[] Directions =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Start()
        {
            foreach (var cell in tilemap.cellBounds.allPositionsWithin)
            {
                if (!tilemap.HasTile(cell) || visited.Contains(cell))
                    continue;

                List<Vector3Int> cluster = FloodFill(cell);
                CreateBlockFromCluster(cluster);
            }

            tilemap.gameObject.SetActive(false);
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        List<Vector3Int> FloodFill(Vector3Int start)
        {
            List<Vector3Int> cluster = new List<Vector3Int>();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                cluster.Add(current);

                foreach (var dir in Directions)
                {
                    var next = current + dir;
                    if (!visited.Contains(next) && tilemap.HasTile(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return cluster;
        }

        void CreateBlockFromCluster(List<Vector3Int> cluster)
        {
            // Compute bounds
            Vector3Int min = cluster[0];
            Vector3Int max = cluster[0];

            foreach (var cell in cluster)
            {
                min = Vector3Int.Min(min, cell);
                max = Vector3Int.Max(max, cell);
            }

            Vector3 centerWorld =
                tilemap.CellToWorld(min) +
                Vector3.Scale(tilemap.cellSize, (Vector3)(max - min + Vector3Int.one)) / 2f;

            GameObject block = Instantiate(blockParentPrefab, centerWorld, Quaternion.identity, container);

            // Create containers for highlight effects
            GameObject glowLayer = new GameObject("GlowLayer");
            glowLayer.transform.SetParent(block.transform);
            glowLayer.transform.localPosition = Vector3.zero;
            glowLayer.SetActive(false);
            
            GameObject outlineLayer = new GameObject("OutlineLayer");
            outlineLayer.transform.SetParent(block.transform);
            outlineLayer.transform.localPosition = Vector3.zero;
            outlineLayer.SetActive(false);

            foreach (var cell in cluster)
            {
                Sprite sprite = tilemap.GetSprite(cell);
                if (!sprite) continue;

                Vector3 worldPos = tilemap.CellToWorld(cell) + tilemap.cellSize / 2f;

                // Create main sprite
                GameObject child = new GameObject("TileSprite");
                child.transform.SetParent(block.transform);
                child.transform.position = worldPos;

                var sr = child.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerID = tilemap.GetComponent<TilemapRenderer>().sortingLayerID;
                sr.sortingOrder = tilemap.GetComponent<TilemapRenderer>().sortingOrder;

                var poly = child.AddComponent<PolygonCollider2D>();
                poly.usedByComposite = true;
                
                // Create glow sprite (scaled, behind, tinted)
                GameObject glowChild = new GameObject("GlowSprite");
                glowChild.transform.SetParent(glowLayer.transform);
                glowChild.transform.position = worldPos;
                glowChild.transform.localScale = Vector3.one * glowScale;
                
                var glowSr = glowChild.AddComponent<SpriteRenderer>();
                glowSr.sprite = sprite;
                glowSr.color = glowColor;
                glowSr.sortingLayerID = sr.sortingLayerID;
                glowSr.sortingOrder = sr.sortingOrder - 1;
                
                // Create outline sprites (4 directional offsets)
                Vector3[] outlineOffsets = new Vector3[]
                {
                    new Vector3(outlineThickness, 0, 0),
                    new Vector3(-outlineThickness, 0, 0),
                    new Vector3(0, outlineThickness, 0),
                    new Vector3(0, -outlineThickness, 0)
                };
                
                foreach (var offset in outlineOffsets)
                {
                    GameObject outlineChild = new GameObject("OutlineSprite");
                    outlineChild.transform.SetParent(outlineLayer.transform);
                    outlineChild.transform.position = worldPos + offset;
                    
                    var outlineSr = outlineChild.AddComponent<SpriteRenderer>();
                    outlineSr.sprite = sprite;
                    outlineSr.color = outlineColor;
                    outlineSr.sortingLayerID = sr.sortingLayerID;
                    outlineSr.sortingOrder = sr.sortingOrder - 1;
                }
            }

            // Calculate actual bounds from all polygon colliders
            PolygonCollider2D[] polygons = block.GetComponentsInChildren<PolygonCollider2D>();
            if (polygons.Length > 0)
            {
                Bounds compositeBounds = polygons[0].bounds;
                foreach (var poly in polygons)
                {
                    compositeBounds.Encapsulate(poly.bounds);
                }

                // Resize trigger collider to be 1.2x larger than the composite shape
                BoxCollider2D col = block.GetComponent<BoxCollider2D>();
                MatterBehaviour matterBehaviour = block.GetComponent<MatterBehaviour>();
                matterBehaviour.interactionButton = interactionButton;
                matterBehaviour.glowVisual = glowLayer;
                matterBehaviour.outlineVisual = outlineLayer;
                
                // Assign material to the matter block
                matterBehaviour.material = material;
                
                // Pass highlight settings from TilemapToGameObjects to MatterBehaviour
                matterBehaviour.glowColor = glowColor;
                matterBehaviour.glowScale = glowScale;
                matterBehaviour.outlineColor = outlineColor;
                matterBehaviour.outlineThickness = outlineThickness;
                
                Vector2 size = compositeBounds.size * 1.2f;
                col.size = size;
                col.offset = block.transform.InverseTransformPoint(compositeBounds.center);
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}