using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Synchronizes drawing tilemaps (where you paint) to sprite reference tilemaps (used for phase transformations).
    /// Drawing tilemaps spawn the actual game objects, sprite reference tilemaps provide sprites during state changes.
    /// </summary>
    public class TilemapStateSynchronizer : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝
        [Header("Drawing Tilemaps")]
        [Tooltip("Where you draw the solid state tiles (e.g., ice) - these spawn the actual blocks")]
        public Tilemap solidDrawingTilemap;
        
        [Tooltip("Where you draw the liquid state tiles (e.g., water) - these spawn the actual blocks")]
        public Tilemap liquidDrawingTilemap;
        
        [Tooltip("Where you draw the gas state tiles (e.g., steam) - these spawn the actual blocks")]
        public Tilemap gasDrawingTilemap;

        [Space(10)]
        [Header("Drawing Tilemaps (Alternate)")]
        [Tooltip("Alternate map for solid state tiles to prevent merging with main map clusters")]
        public Tilemap solidDrawingTilemapAlt;

        [Tooltip("Alternate map for liquid state tiles to prevent merging with main map clusters")]
        public Tilemap liquidDrawingTilemapAlt;

        [Tooltip("Alternate map for gas state tiles to prevent merging with main map clusters")]
        public Tilemap gasDrawingTilemapAlt;
        
        [Space(10)]
        [Header("Sprite Reference Tilemaps")]
        [Tooltip("Synced copy for solid sprite lookups during phase transformations")]
        public Tilemap solidSpriteTilemap;
        
        [Tooltip("Synced copy for liquid sprite lookups during phase transformations")]
        public Tilemap liquidSpriteTilemap;
        
        [Tooltip("Synced copy for gas sprite lookups during phase transformations")]
        public Tilemap gasSpriteTilemap;

        [Space(10)]
        [Header("Sprite Reference Tilemaps (Alternate)")]
        [Tooltip("Synced copy for solid sprite lookups for alternate map blocks")]
        public Tilemap solidSpriteTilemapAlt;

        [Tooltip("Synced copy for liquid sprite lookups for alternate map blocks")]
        public Tilemap liquidSpriteTilemapAlt;

        [Tooltip("Synced copy for gas sprite lookups for alternate map blocks")]
        public Tilemap gasSpriteTilemapAlt;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝
        // [Header("Displays")]

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝
        [Space(10)]
        [Header("Tile References")]
        [Tooltip("The tile to place on solid sprite tilemap when syncing")]
        public TileBase solidTile;
        
        [Tooltip("The tile to place on liquid sprite tilemap when syncing")]
        public TileBase liquidTile;
        
        [Tooltip("The tile to place on gas sprite tilemap when syncing")]
        public TileBase gasTile;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝
        
        // Static references for phase change system to access sprite tilemaps
        public static Tilemap solidSpriteReference;
        public static Tilemap liquidSpriteReference;
        public static Tilemap gasSpriteReference;
        
        // Static references for alternate phase change system
        public static Tilemap solidSpriteReferenceAlt;
        public static Tilemap liquidSpriteReferenceAlt;
        public static Tilemap gasSpriteReferenceAlt;

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake()
        {
            // Set static references for phase change system
            solidSpriteReference = solidSpriteTilemap;
            liquidSpriteReference = liquidSpriteTilemap;
            gasSpriteReference = gasSpriteTilemap;
            
            solidSpriteReferenceAlt = solidSpriteTilemapAlt;
            liquidSpriteReferenceAlt = liquidSpriteTilemapAlt;
            gasSpriteReferenceAlt = gasSpriteTilemapAlt;
            
            // Disable all tilemap renderers (hide tilemaps but keep them active for sprite lookups)
            DisableAllTilemapRenderers();
        }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        /// <summary>
        /// Synchronizes sprite reference tilemaps from drawing tilemaps.
        /// Copies all tile positions from drawing tilemaps to ALL sprite reference tilemaps.
        /// Call this from the custom editor button after drawing your level.
        /// </summary>
        public void SynchronizeTilemaps()
        {
            // Validate drawing tilemaps
            if (solidDrawingTilemap == null || liquidDrawingTilemap == null || gasDrawingTilemap == null)
            {
                Debug.LogError("TilemapStateSynchronizer: All three drawing tilemaps must be assigned!");
                return;
            }
            
            // Validate sprite reference tilemaps
            if (solidSpriteTilemap == null || liquidSpriteTilemap == null || gasSpriteTilemap == null)
            {
                Debug.LogError("TilemapStateSynchronizer: All three sprite reference tilemaps must be assigned!");
                return;
            }
            
            // Validate tile references
            if (solidTile == null || liquidTile == null || gasTile == null)
            {
                Debug.LogError("TilemapStateSynchronizer: All three tile references must be assigned!");
                return;
            }
            
            // Clear all sprite reference tilemaps first
            solidSpriteTilemap.ClearAllTiles();
            liquidSpriteTilemap.ClearAllTiles();
            gasSpriteTilemap.ClearAllTiles();
            
            // Collect all positions from ALL drawing tilemaps
            HashSet<Vector3Int> allPositions = new HashSet<Vector3Int>();
            
            // Collect from solid drawing tilemap
            foreach (var pos in solidDrawingTilemap.cellBounds.allPositionsWithin)
            {
                if (solidDrawingTilemap.HasTile(pos))
                {
                    allPositions.Add(pos);
                }
            }
            
            // Collect from liquid drawing tilemap
            foreach (var pos in liquidDrawingTilemap.cellBounds.allPositionsWithin)
            {
                if (liquidDrawingTilemap.HasTile(pos))
                {
                    allPositions.Add(pos);
                }
            }
            
            // Collect from gas drawing tilemap
            foreach (var pos in gasDrawingTilemap.cellBounds.allPositionsWithin)
            {
                if (gasDrawingTilemap.HasTile(pos))
                {
                    allPositions.Add(pos);
                }
            }
            
            // Copy ALL positions to ALL sprite reference tilemaps
            foreach (var pos in allPositions)
            {
                solidSpriteTilemap.SetTile(pos, solidTile);
                liquidSpriteTilemap.SetTile(pos, liquidTile);
                gasSpriteTilemap.SetTile(pos, gasTile);
            }
            
            // --- Sync Alternate Maps ---
            if (solidDrawingTilemapAlt != null && liquidDrawingTilemapAlt != null && gasDrawingTilemapAlt != null &&
                solidSpriteTilemapAlt != null && liquidSpriteTilemapAlt != null && gasSpriteTilemapAlt != null)
            {
                // Clear alternate sprite maps
                solidSpriteTilemapAlt.ClearAllTiles();
                liquidSpriteTilemapAlt.ClearAllTiles();
                gasSpriteTilemapAlt.ClearAllTiles();
                
                HashSet<Vector3Int> allPositionsAlt = new HashSet<Vector3Int>();
                
                // Collect from solid alt
                foreach (var pos in solidDrawingTilemapAlt.cellBounds.allPositionsWithin)
                {
                    if (solidDrawingTilemapAlt.HasTile(pos)) allPositionsAlt.Add(pos);
                }
                
                // Collect from liquid alt
                foreach (var pos in liquidDrawingTilemapAlt.cellBounds.allPositionsWithin)
                {
                    if (liquidDrawingTilemapAlt.HasTile(pos)) allPositionsAlt.Add(pos);
                }
                
                // Collect from gas alt
                foreach (var pos in gasDrawingTilemapAlt.cellBounds.allPositionsWithin)
                {
                    if (gasDrawingTilemapAlt.HasTile(pos)) allPositionsAlt.Add(pos);
                }
                
                // Copy to alt sprite maps
                foreach (var pos in allPositionsAlt)
                {
                    solidSpriteTilemapAlt.SetTile(pos, solidTile);
                    liquidSpriteTilemapAlt.SetTile(pos, liquidTile);
                    gasSpriteTilemapAlt.SetTile(pos, gasTile);
                }
                
                Debug.Log($"TilemapStateSynchronizer: Synchronized {allPositionsAlt.Count} positions for Alternate maps.");
            }
            
            // Disable all tilemap renderers to hide them
            DisableAllTilemapRenderers();
            
            Debug.Log($"TilemapStateSynchronizer: Synchronized {allPositions.Count} positions. All tilemap renderers disabled.");
        }
        
        /// <summary>
        /// Resets all tilemaps by clearing them.
        /// </summary>
        public void ResetTilemaps()
        {
            if (solidDrawingTilemap != null) solidDrawingTilemap.ClearAllTiles();
            if (liquidDrawingTilemap != null) liquidDrawingTilemap.ClearAllTiles();
            if (gasDrawingTilemap != null) gasDrawingTilemap.ClearAllTiles();
            
            if (solidSpriteTilemap != null) solidSpriteTilemap.ClearAllTiles();
            if (liquidSpriteTilemap != null) liquidSpriteTilemap.ClearAllTiles();
            if (gasSpriteTilemap != null) gasSpriteTilemap.ClearAllTiles();
            
            // Reset alternate maps too
            if (solidDrawingTilemapAlt != null) solidDrawingTilemapAlt.ClearAllTiles();
            if (liquidDrawingTilemapAlt != null) liquidDrawingTilemapAlt.ClearAllTiles();
            if (gasDrawingTilemapAlt != null) gasDrawingTilemapAlt.ClearAllTiles();
            
            if (solidSpriteTilemapAlt != null) solidSpriteTilemapAlt.ClearAllTiles();
            if (liquidSpriteTilemapAlt != null) liquidSpriteTilemapAlt.ClearAllTiles();
            if (gasSpriteTilemapAlt != null) gasSpriteTilemapAlt.ClearAllTiles();
            
            Debug.Log("TilemapStateSynchronizer: All tilemaps reset.");
        }
        
        /// <summary>
        /// Re-enables drawing tilemap renderers so the painted matter is visible in the editor after syncing.
        /// </summary>
        public void EnableDrawingTilemaps()
        {
            if (solidDrawingTilemap != null)
            {
                TilemapRenderer renderer = solidDrawingTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = true;
            }

            if (liquidDrawingTilemap != null)
            {
                TilemapRenderer renderer = liquidDrawingTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = true;
            }

            if (gasDrawingTilemap != null)
            {
                TilemapRenderer renderer = gasDrawingTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = true;
            }
            
            // Enable alternate drawing tilemaps
            if (solidDrawingTilemapAlt != null)
            {
                TilemapRenderer renderer = solidDrawingTilemapAlt.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = true;
            }
            
            if (liquidDrawingTilemapAlt != null)
            {
                TilemapRenderer renderer = liquidDrawingTilemapAlt.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = true;
            }
            
            if (gasDrawingTilemapAlt != null)
            {
                TilemapRenderer renderer = gasDrawingTilemapAlt.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = true;
            }

            Debug.Log("TilemapStateSynchronizer: Drawing tilemap renderers re-enabled.");
        }

        /// <summary>
        /// Disables all tilemap renderers to hide tilemaps while keeping them active for sprite lookups
        /// </summary>
        private void DisableAllTilemapRenderers()
        {
            // Disable drawing tilemap renderers
            if (solidDrawingTilemap != null)
            {
                TilemapRenderer renderer = solidDrawingTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            if (liquidDrawingTilemap != null)
            {
                TilemapRenderer renderer = liquidDrawingTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            if (gasDrawingTilemap != null)
            {
                TilemapRenderer renderer = gasDrawingTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            // Disable alternate drawing tilemaps
            if (solidDrawingTilemapAlt != null)
            {
                TilemapRenderer renderer = solidDrawingTilemapAlt.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            if (liquidDrawingTilemapAlt != null)
            {
                TilemapRenderer renderer = liquidDrawingTilemapAlt.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            if (gasDrawingTilemapAlt != null)
            {
                TilemapRenderer renderer = gasDrawingTilemapAlt.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            // Disable sprite reference tilemap renderers
            if (solidSpriteTilemap != null)
            {
                TilemapRenderer renderer = solidSpriteTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            if (liquidSpriteTilemap != null)
            {
                TilemapRenderer renderer = liquidSpriteTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            if (gasSpriteTilemap != null)
            {
                TilemapRenderer renderer = gasSpriteTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            // Disable alternate sprite reference tilemaps
            if (solidSpriteTilemapAlt != null)
            {
                TilemapRenderer renderer = solidSpriteTilemapAlt.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            if (liquidSpriteTilemapAlt != null)
            {
                TilemapRenderer renderer = liquidSpriteTilemapAlt.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            
            if (gasSpriteTilemapAlt != null)
            {
                TilemapRenderer renderer = gasSpriteTilemapAlt.GetComponent<TilemapRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}