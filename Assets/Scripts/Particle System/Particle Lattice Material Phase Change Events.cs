using UnityEngine;
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
        protected void TransformMatterBlock(GameObject matterBlock, GameObject newStatePrefab)
        {
            if (newStatePrefab == null)
            {
                Debug.LogWarning("TransformMatterBlock: newStatePrefab is null!");
                return;
            }
            
            // 1. Extract sprite from the new prefab
            Sprite newSprite = null;
            SpriteRenderer prefabRenderer = newStatePrefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabRenderer != null)
            {
                newSprite = prefabRenderer.sprite;
            }
            
            if (newSprite == null)
            {
                Debug.LogError("TransformMatterBlock: No sprite found in newStatePrefab!");
                return;
            }
            
            // 2. Extract tile positions from current block
            List<Vector3> tilePositions = new List<Vector3>();
            SpriteRenderer[] spriteRenderers = matterBlock.GetComponentsInChildren<SpriteRenderer>();
            
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                // Skip glow and outline layers
                if (sr.transform.parent.name == "GlowLayer" || sr.transform.parent.name == "OutlineLayer")
                    continue;
                    
                // Collect tile positions
                if (sr.gameObject.name == "TileSprite" || sr.transform.parent == matterBlock.transform)
                {
                    tilePositions.Add(sr.transform.position);
                }
            }
            
            if (tilePositions.Count == 0)
            {
                Debug.LogError("TransformMatterBlock: No tile positions found in matter block!");
                return;
            }
            
            // 3. Calculate center position
            Vector3 centerPosition = Vector3.zero;
            foreach (Vector3 pos in tilePositions)
            {
                centerPosition += pos;
            }
            centerPosition /= tilePositions.Count;
            
            // 4. Store parent container
            Transform container = matterBlock.transform.parent;
            
            // 5. Destroy old block completely
            Object.Destroy(matterBlock);
            
            // 6. Instantiate ONE new prefab at center position
            GameObject newBlock = Object.Instantiate(newStatePrefab, centerPosition, Quaternion.identity, container);
            
            // 7. Create GlowLayer
            GameObject glowLayer = new GameObject("GlowLayer");
            glowLayer.transform.SetParent(newBlock.transform);
            glowLayer.transform.localPosition = Vector3.zero;
            glowLayer.SetActive(false);
            
            // 8. Create OutlineLayer
            GameObject outlineLayer = new GameObject("OutlineLayer");
            outlineLayer.transform.SetParent(newBlock.transform);
            outlineLayer.transform.localPosition = Vector3.zero;
            outlineLayer.SetActive(false);
            
            // 9. Create TileSprites at each position
            foreach (Vector3 position in tilePositions)
            {
                GameObject tileSprite = new GameObject("TileSprite");
                tileSprite.transform.SetParent(newBlock.transform);
                tileSprite.transform.position = position;
                
                SpriteRenderer sr = tileSprite.AddComponent<SpriteRenderer>();
                sr.sprite = newSprite;
                
                PolygonCollider2D poly = tileSprite.AddComponent<PolygonCollider2D>();
                poly.usedByComposite = true;
            }
            
            Debug.Log($"Destroyed old block and created new {newStatePrefab.name} cluster with {tilePositions.Count} tiles");
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}