using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Abstract base class for defining phase change behaviors for different materials
    /// </summary>
    public abstract class ParticleLatticeMaterialPhaseChangeEvents : ScriptableObject
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
        [Header("State Prefabs")]
        [Tooltip("Prefab or sprite for the solid state of this material")]
        public GameObject solidStatePrefab;
        
        [Tooltip("Prefab or sprite for the liquid state of this material")]
        public GameObject liquidStatePrefab;

        [Tooltip("Prefab or sprite for the gas state of this material")]
        public GameObject gasStatePrefab;
        public LayerMask sortingLayerID = 0;
        public int sortingOrder = 0;
        

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Called when a solid melts into a liquid
        /// </summary>
        public abstract void OnMelt(GameObject matterBlock);
        
        /// <summary>
        /// Called when a liquid freezes into a solid
        /// </summary>
        public abstract void OnFreeze(GameObject matterBlock);
        
        /// <summary>
        /// Called when a liquid evaporates into a gas
        /// </summary>
        public abstract void OnEvaporate(GameObject matterBlock);
        
        /// <summary>
        /// Called when a gas condenses into a liquid
        /// </summary>
        public abstract void OnCondense(GameObject matterBlock);
        
        /// <summary>
        /// Called when a solid sublimates into a gas
        /// </summary>
        public abstract void OnSublimate(GameObject matterBlock);
        
        /// <summary>
        /// Called when a gas deposits into a solid
        /// </summary>
        public abstract void OnDeposition(GameObject matterBlock);
        
        /// <summary>
        /// Helper method to transform a matter block to a new state while preserving its structure
        /// </summary>
        /// <param name="matterBlock">The matter block to transform</param>
        /// <param name="newStatePrefab">The prefab to instantiate for the new state</param>
        /// <param name="spriteTilemap">The sprite reference tilemap to look up sprites from (from TilemapStateSynchronizer)</param>
        protected void TransformMatterBlock(GameObject matterBlock, GameObject newStatePrefab, Tilemap spriteTilemap)
        {
            if (newStatePrefab == null)
            {
                Debug.LogWarning("TransformMatterBlock: newStatePrefab is null!");
                return;
            }
            
            if (spriteTilemap == null)
            {
                Debug.LogWarning("TransformMatterBlock: spriteTilemap is null!");
                return;
            }
            
            // 1. Extract tile positions and their corresponding sprites from current block
            List<Vector3> tilePositions = new List<Vector3>();
            List<Sprite> tileSprites = new List<Sprite>();
            SpriteRenderer[] spriteRenderers = matterBlock.GetComponentsInChildren<SpriteRenderer>();
            
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                // Skip glow and outline layers
                if (sr.transform.parent.name == "GlowLayer" || sr.transform.parent.name == "OutlineLayer")
                    continue;
                    
                // Collect tile positions
                if (sr.gameObject.name == "TileSprite" || sr.transform.parent == matterBlock.transform)
                {
                    Vector3 worldPos = sr.transform.position;
                    tilePositions.Add(worldPos);
                    
                    // Look up sprite from the sprite reference tilemap
                    Vector3Int cellPos = spriteTilemap.WorldToCell(worldPos);
                    Sprite newSprite = spriteTilemap.GetSprite(cellPos);
                    
                    if (newSprite == null)
                    {
                        Debug.LogWarning($"TransformMatterBlock: No sprite found at position {cellPos} in sprite tilemap. Using prefab sprite as fallback.");
                        // Fallback: try to get sprite from prefab
                        SpriteRenderer prefabRenderer = newStatePrefab.GetComponentInChildren<SpriteRenderer>();
                        if (prefabRenderer != null)
                        {
                            newSprite = prefabRenderer.sprite;
                        }
                    }
                    
                    tileSprites.Add(newSprite);
                }
            }
            
            if (tilePositions.Count == 0)
            {
                Debug.LogError("TransformMatterBlock: No tile positions found in matter block!");
                return;
            }
            
            // 2. Calculate center position
            Vector3 centerPosition = Vector3.zero;
            foreach (Vector3 pos in tilePositions)
            {
                centerPosition += pos;
            }
            centerPosition /= tilePositions.Count;
            
            // 3. Store parent container
            Transform container = matterBlock.transform.parent;
            
            // 4. Destroy old block completely
            Object.Destroy(matterBlock);
            
            // 5. Instantiate ONE new prefab at center position
            GameObject newBlock = Object.Instantiate(newStatePrefab, centerPosition, Quaternion.identity, container);
            
            // 6. Create GlowLayer
            GameObject glowLayer = new GameObject("GlowLayer");
            glowLayer.transform.SetParent(newBlock.transform);
            glowLayer.transform.localPosition = Vector3.zero;
            glowLayer.SetActive(false);
            
            // 7. Create OutlineLayer
            GameObject outlineLayer = new GameObject("OutlineLayer");
            outlineLayer.transform.SetParent(newBlock.transform);
            outlineLayer.transform.localPosition = Vector3.zero;
            outlineLayer.SetActive(false);
            
            // 8. Create TileSprites at each position with their looked-up sprites
            for (int i = 0; i < tilePositions.Count; i++)
            {
                Vector3 worldPos = tilePositions[i];
                Sprite sprite = tileSprites[i];
                
                // Create main sprite
                GameObject tileSprite = new GameObject("TileSprite");
                tileSprite.tag = newStatePrefab.tag; // Assign the same tag as the prefab for consistency
                tileSprite.layer = newStatePrefab.layer; // Assign the same layer as the prefab for
                tileSprite.transform.SetParent(newBlock.transform);
                tileSprite.transform.position = worldPos;
                
                SpriteRenderer sr = tileSprite.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerID = TilemapToGameObjects.sortingLayerReference;
                sr.sortingOrder = TilemapToGameObjects.sortingOrderReference;
                
                PolygonCollider2D poly = tileSprite.AddComponent<PolygonCollider2D>();
                poly.usedByComposite = true;
                
                // Shrink the polygon collider if needed
                if (TilemapToGameObjects.colliderShrinkReference > 0f)
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
                        float scale = 1f - TilemapToGameObjects.colliderShrinkReference;
                        for (int j = 0; j < points.Length; j++)
                        {
                            points[j] = centroid + (points[j] - centroid) * scale;
                        }
                        
                        poly.SetPath(pathIndex, points);
                    }
                }
                
                // Create glow sprite (scaled, behind, tinted)
                GameObject glowChild = new GameObject("GlowSprite");
                glowChild.transform.SetParent(glowLayer.transform);
                glowChild.transform.position = worldPos;
                glowChild.transform.localScale = Vector3.one * TilemapToGameObjects.glowScaleReference;
                
                SpriteRenderer glowSr = glowChild.AddComponent<SpriteRenderer>();
                glowSr.sprite = sprite;
                glowSr.color = TilemapToGameObjects.glowColorReference;
                glowSr.sortingLayerID = sr.sortingLayerID;
                glowSr.sortingOrder = sr.sortingOrder - 1;
                
                // Create outline sprites (4 directional offsets)
                Vector3[] outlineOffsets = new Vector3[]
                {
                    new Vector3(TilemapToGameObjects.outlineThicknessReference, 0, 0),
                    new Vector3(-TilemapToGameObjects.outlineThicknessReference, 0, 0),
                    new Vector3(0, TilemapToGameObjects.outlineThicknessReference, 0),
                    new Vector3(0, -TilemapToGameObjects.outlineThicknessReference, 0)
                };
                
                foreach (var offset in outlineOffsets)
                {
                    GameObject outlineChild = new GameObject("OutlineSprite");
                    outlineChild.transform.SetParent(outlineLayer.transform);
                    outlineChild.transform.position = worldPos + offset;
                    
                    SpriteRenderer outlineSr = outlineChild.AddComponent<SpriteRenderer>();
                    outlineSr.sprite = sprite;
                    outlineSr.color = TilemapToGameObjects.outlineColorReference;
                    outlineSr.sortingLayerID = sr.sortingLayerID;
                    outlineSr.sortingOrder = sr.sortingOrder - 1;
                }
            }
            
            // 9. Calculate actual bounds from all polygon colliders and setup interaction
            PolygonCollider2D[] polygons = newBlock.GetComponentsInChildren<PolygonCollider2D>();
            if (polygons.Length > 0)
            {
                Bounds compositeBounds = polygons[0].bounds;
                foreach (var poly in polygons)
                {
                    compositeBounds.Encapsulate(poly.bounds);
                }

                // Resize trigger collider to be 1.2x larger than the composite shape
                BoxCollider2D col = newBlock.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    Vector2 size = compositeBounds.size * 1.2f;
                    col.size = size;
                    col.offset = newBlock.transform.InverseTransformPoint(compositeBounds.center);
                }
                
                // Set up MatterBehaviour references
                MatterBehaviour matterBehaviour = newBlock.GetComponent<MatterBehaviour>();
                if (matterBehaviour != null)
                {
                    matterBehaviour.interactionButton = TilemapToGameObjects.interactionButtonReference;
                    matterBehaviour.glowVisual = glowLayer;
                    matterBehaviour.outlineVisual = outlineLayer;
                    
                    // Pass highlight settings
                    matterBehaviour.glowColor = TilemapToGameObjects.glowColorReference;
                    matterBehaviour.glowScale = TilemapToGameObjects.glowScaleReference;
                    matterBehaviour.outlineColor = TilemapToGameObjects.outlineColorReference;
                    matterBehaviour.outlineThickness = TilemapToGameObjects.outlineThicknessReference;
                }
            }
            
            Debug.Log($"Transformed block to {newStatePrefab.name} with {tilePositions.Count} tiles using sprite lookups from tilemap");
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}