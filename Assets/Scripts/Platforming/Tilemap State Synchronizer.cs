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
        [Header("Sprite Reference Tilemaps")]
        [Tooltip("Synced copy for solid sprite lookups during phase transformations")]
        public Tilemap solidSpriteTilemap;
        
        [Tooltip("Synced copy for liquid sprite lookups during phase transformations")]
        public Tilemap liquidSpriteTilemap;
        
        [Tooltip("Synced copy for gas sprite lookups during phase transformations")]
        public Tilemap gasSpriteTilemap;

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

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝
        void Awake()
        {
            // Set static references for phase change system
            solidSpriteReference = solidSpriteTilemap;
            liquidSpriteReference = liquidSpriteTilemap;
            gasSpriteReference = gasSpriteTilemap;
            
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
            
            // Disable all tilemap renderers to hide them
            DisableAllTilemapRenderers();
            
            Debug.Log($"TilemapStateSynchronizer: Synchronized {allPositions.Count} positions. All tilemap renderers disabled.");
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
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}