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

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Fields")]
        
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

            foreach (var cell in cluster)
            {
                Sprite sprite = tilemap.GetSprite(cell);
                if (!sprite) continue;

                GameObject child = new GameObject("TileSprite");
                child.transform.SetParent(block.transform);

                Vector3 worldPos = tilemap.CellToWorld(cell) + tilemap.cellSize / 2f;
                child.transform.position = worldPos;

                var sr = child.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerID = tilemap.GetComponent<TilemapRenderer>().sortingLayerID;
                sr.sortingOrder = tilemap.GetComponent<TilemapRenderer>().sortingOrder;

                var poly = child.AddComponent<PolygonCollider2D>();
                poly.usedByComposite = true;
            }

            // Resize collider to fit cluster
            BoxCollider2D col = block.GetComponent<BoxCollider2D>();
            Vector2 size = (Vector2)(Vector3)(max - min + Vector3Int.one);
            size = Vector2.Scale(size, tilemap.cellSize);
            col.size = size;
            col.offset = Vector2.zero;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}