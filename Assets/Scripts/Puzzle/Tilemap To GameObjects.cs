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
        
        // Static references for phase change system to access
        public static GameObject blockParentPrefabReference;
        public static GameObject interactionButtonReference;
        public static float colliderShrinkReference = 0.05f;
        public static Color glowColorReference = new Color(1f, 1f, 1f, 0.5f);
        public static float glowScaleReference = 1.15f;
        public static Color outlineColorReference = Color.white;
        public static float outlineThicknessReference = 0.1f;
        public static int sortingLayerReference = 0;
        public static int sortingOrderReference = 0;

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
        public LayerMask sortingLayerID = 0;
        public int sortingOrder = 0;
        
        [Space(10)]
        [Header("Collider Settings")]
        [SerializeField] private bool isGround = true;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] [Range(0f, 0.5f)] [Tooltip("Shrink colliders by this percentage (0 = no shrink, 0.5 = 50% smaller)")]
        private float colliderShrinkPercentage = 0.05f;
        
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
            // Set static references for phase change system
            if (blockParentPrefab != null)
            {
                blockParentPrefabReference = blockParentPrefab;
                interactionButtonReference = interactionButton;
                colliderShrinkReference = colliderShrinkPercentage;
                glowColorReference = glowColor;
                glowScaleReference = glowScale;
                outlineColorReference = outlineColor;
                outlineThicknessReference = outlineThickness;
                sortingLayerReference = sortingLayerID;
                sortingOrderReference = sortingOrder;
            }
            
            int clusterCount = 0;
            
            foreach (var cell in tilemap.cellBounds.allPositionsWithin)
            {
                if (!tilemap.HasTile(cell) || visited.Contains(cell))
                    continue;

                List<Vector3Int> cluster = FloodFill(cell);
                CreateBlockFromCluster(cluster);
                clusterCount++;
            }
            
            if (interactionButton != null)
                interactionButton.SetActive(false);

            Debug.Log($"TilemapToGameObjects: Spawned {clusterCount} matter block clusters from {tilemap.gameObject.name}.");
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
                child.layer = isGround ? LayerMask.NameToLayer("Ground") : LayerMask.NameToLayer("Default");
                child.transform.SetParent(block.transform);
                child.transform.position = worldPos;

                var sr = child.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerID = sortingLayerID;
                sr.sortingOrder = sortingOrder;

                var poly = child.AddComponent<PolygonCollider2D>();
                poly.usedByComposite = true;
                
                // Shrink the polygon collider if needed
                if (colliderShrinkPercentage > 0f)
                {
                    for (int pathIndex = 0; pathIndex < poly.pathCount; pathIndex++)
                    {
                        Vector2[] points = poly.GetPath(pathIndex);
                        
                        // Calculate centroid of the polygon
                        Vector2 centroid = Vector2.zero;
                        foreach (var point in points)
                        {
                            centroid += point;
                        }
                        centroid /= points.Length;
                        
                        // Scale points toward centroid
                        float scale = 1f - colliderShrinkPercentage;
                        for (int i = 0; i < points.Length; i++)
                        {
                            points[i] = centroid + (points[i] - centroid) * scale;
                        }
                        
                        poly.SetPath(pathIndex, points);
                    }
                }
                
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
        
        /// <summary>
        /// Rebuilds a matter block from scratch using new sprite while preserving structure
        /// </summary>
        /// <param name="tilePositions">World positions of all tiles</param>
        /// <param name="newSprite">The sprite to use for the new state</param>
        /// <param name="blockParentPrefab">Parent prefab with Rigidbody2D and colliders</param>
        /// <param name="material">Material data</param>
        /// <param name="interactionButton">Interaction button reference</param>
        /// <param name="container">Optional parent transform</param>
        /// <param name="settings">Block settings (collider, highlight, etc.)</param>
        /// <returns>The newly created matter block</returns>
        public static GameObject RebuildMatterBlock(
            List<Vector3> tilePositions,
            Sprite newSprite,
            GameObject blockParentPrefab,
            ParticleLatticeMaterial material,
            GameObject interactionButton,
            Transform container,
            MatterBlockSettings settings)
        {
            if (tilePositions == null || tilePositions.Count == 0)
            {
                Debug.LogError("RebuildMatterBlock: No tile positions provided!");
                return null;
            }
            
            if (newSprite == null)
            {
                Debug.LogError("RebuildMatterBlock: No sprite provided!");
                return null;
            }
            
            // Calculate center position
            Vector3 centerPos = Vector3.zero;
            foreach (var pos in tilePositions)
            {
                centerPos += pos;
            }
            centerPos /= tilePositions.Count;
            
            // Create new block
            GameObject block = Instantiate(blockParentPrefab, centerPos, Quaternion.identity, container);
            
            // Create highlight layers
            GameObject glowLayer = new GameObject("GlowLayer");
            glowLayer.transform.SetParent(block.transform);
            glowLayer.transform.localPosition = Vector3.zero;
            glowLayer.SetActive(false);
            
            GameObject outlineLayer = new GameObject("OutlineLayer");
            outlineLayer.transform.SetParent(block.transform);
            outlineLayer.transform.localPosition = Vector3.zero;
            outlineLayer.SetActive(false);
            
            // Create tiles at each position
            foreach (var worldPos in tilePositions)
            {
                // Main sprite
                GameObject child = new GameObject("TileSprite");
                child.layer = settings.isGround ? LayerMask.NameToLayer("Ground") : LayerMask.NameToLayer("Default");
                child.transform.SetParent(block.transform);
                child.transform.position = worldPos;
                
                var sr = child.AddComponent<SpriteRenderer>();
                sr.sprite = newSprite;
                sr.sortingLayerID = settings.sortingLayerID;
                sr.sortingOrder = settings.sortingOrder;
                
                var poly = child.AddComponent<PolygonCollider2D>();
                poly.usedByComposite = true;
                
                // Shrink collider if needed
                if (settings.colliderShrinkPercentage > 0f)
                {
                    for (int pathIndex = 0; pathIndex < poly.pathCount; pathIndex++)
                    {
                        Vector2[] points = poly.GetPath(pathIndex);
                        Vector2 centroid = Vector2.zero;
                        foreach (var point in points)
                            centroid += point;
                        centroid /= points.Length;
                        
                        float scale = 1f - settings.colliderShrinkPercentage;
                        for (int i = 0; i < points.Length; i++)
                            points[i] = centroid + (points[i] - centroid) * scale;
                        
                        poly.SetPath(pathIndex, points);
                    }
                }
                
                // Glow sprite
                GameObject glowChild = new GameObject("GlowSprite");
                glowChild.transform.SetParent(glowLayer.transform);
                glowChild.transform.position = worldPos;
                glowChild.transform.localScale = Vector3.one * settings.glowScale;
                
                var glowSr = glowChild.AddComponent<SpriteRenderer>();
                glowSr.sprite = newSprite;
                glowSr.color = settings.glowColor;
                glowSr.sortingLayerID = settings.sortingLayerID;
                glowSr.sortingOrder = settings.sortingOrder - 1;
                
                // Outline sprites
                Vector3[] outlineOffsets = new Vector3[]
                {
                    new Vector3(settings.outlineThickness, 0, 0),
                    new Vector3(-settings.outlineThickness, 0, 0),
                    new Vector3(0, settings.outlineThickness, 0),
                    new Vector3(0, -settings.outlineThickness, 0)
                };
                
                foreach (var offset in outlineOffsets)
                {
                    GameObject outlineChild = new GameObject("OutlineSprite");
                    outlineChild.transform.SetParent(outlineLayer.transform);
                    outlineChild.transform.position = worldPos + offset;
                    
                    var outlineSr = outlineChild.AddComponent<SpriteRenderer>();
                    outlineSr.sprite = newSprite;
                    outlineSr.color = settings.outlineColor;
                    outlineSr.sortingLayerID = settings.sortingLayerID;
                    outlineSr.sortingOrder = settings.sortingOrder - 1;
                }
            }
            
            // Setup trigger collider
            PolygonCollider2D[] polygons = block.GetComponentsInChildren<PolygonCollider2D>();
            if (polygons.Length > 0)
            {
                Bounds compositeBounds = polygons[0].bounds;
                foreach (var poly in polygons)
                    compositeBounds.Encapsulate(poly.bounds);
                
                BoxCollider2D col = block.GetComponent<BoxCollider2D>();
                MatterBehaviour matterBehaviour = block.GetComponent<MatterBehaviour>();
                
                matterBehaviour.interactionButton = interactionButton;
                matterBehaviour.glowVisual = glowLayer;
                matterBehaviour.outlineVisual = outlineLayer;
                matterBehaviour.material = material;
                matterBehaviour.glowColor = settings.glowColor;
                matterBehaviour.glowScale = settings.glowScale;
                matterBehaviour.outlineColor = settings.outlineColor;
                matterBehaviour.outlineThickness = settings.outlineThickness;
                
                Vector2 size = compositeBounds.size * 1.2f;
                col.size = size;
                col.offset = block.transform.InverseTransformPoint(compositeBounds.center);
            }
            
            return block;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
    
    /// <summary>
    /// Settings bundle for rebuilding matter blocks
    /// </summary>
    [System.Serializable]
    public struct MatterBlockSettings
    {
        public bool isGround;
        public int sortingLayerID;
        public int sortingOrder;
        public float colliderShrinkPercentage;
        public Color glowColor;
        public float glowScale;
        public Color outlineColor;
        public float outlineThickness;
    }
}