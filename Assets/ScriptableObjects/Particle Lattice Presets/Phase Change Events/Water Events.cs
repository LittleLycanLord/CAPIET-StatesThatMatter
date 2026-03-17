using UnityEngine;

namespace LilLycanLord_Official
{
    [CreateAssetMenu(fileName = "Water Events", menuName = "States That Matter/Phase Change Events/Water Events")]
    public class WaterEvents : ParticleLatticeMaterialPhaseChangeEvents
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
        // [Space(10)]
        // [Header("Fields")]
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        
        public override void OnMelt(GameObject matterBlock)
        {
            Debug.Log($"Water: Ice melted! Transforming {matterBlock.name} to liquid");

            if (liquidStatePrefab != null)
            {
                TransformMatterBlock(matterBlock, liquidStatePrefab,
                    TilemapStateSynchronizer.liquidSpriteReference,
                    TilemapStateSynchronizer.liquidSpriteReferenceAlt);
            }
            else
            {
                Debug.LogWarning("Water: No liquid state prefab assigned! Destroying instead.");
                Object.Destroy(matterBlock);
            }
        }
        
        public override void OnFreeze(GameObject matterBlock)
        {
            Debug.Log($"Water: Water froze! Transforming {matterBlock.name} to solid ice");

            if (solidStatePrefab != null)
            {
                TransformMatterBlock(matterBlock, solidStatePrefab,
                    TilemapStateSynchronizer.solidSpriteReference,
                    TilemapStateSynchronizer.solidSpriteReferenceAlt);
            }
            else
            {
                Debug.LogWarning("Water: No solid state prefab assigned!");
            }
        }
        
        public override void OnEvaporate(GameObject matterBlock)
        {
            Debug.Log($"Water: Water evaporated! Transforming {matterBlock.name} to gas");

            if (gasStatePrefab != null)
            {
                TransformMatterBlock(matterBlock, gasStatePrefab,
                    TilemapStateSynchronizer.gasSpriteReference,
                    TilemapStateSynchronizer.gasSpriteReferenceAlt);
            }
            else
            {
                Debug.LogWarning("Water: No gas state prefab assigned! Destroying instead.");
                Object.Destroy(matterBlock);
            }
        }
        
        public override void OnCondense(GameObject matterBlock)
        {
            Debug.Log($"Water: Steam condensed! Transforming {matterBlock.name} to liquid water");

            if (liquidStatePrefab != null)
            {
                TransformMatterBlock(matterBlock, liquidStatePrefab,
                    TilemapStateSynchronizer.liquidSpriteReference,
                    TilemapStateSynchronizer.liquidSpriteReferenceAlt);
            }
            else
            {
                Debug.LogWarning("Water: No liquid state prefab assigned!");
            }
        }
        
        public override void OnSublimate(GameObject matterBlock)
        {
            Debug.Log($"Water: Ice sublimated! Transforming {matterBlock.name} to gas");

            if (gasStatePrefab != null)
            {
                TransformMatterBlock(matterBlock, gasStatePrefab,
                    TilemapStateSynchronizer.gasSpriteReference,
                    TilemapStateSynchronizer.gasSpriteReferenceAlt);
            }
            else
            {
                Debug.LogWarning("Water: No gas state prefab assigned! Destroying instead.");
                Object.Destroy(matterBlock);
            }
        }
        
        public override void OnDeposition(GameObject matterBlock)
        {
            Debug.Log($"Water: Steam deposited! Transforming {matterBlock.name} to solid ice");

            if (solidStatePrefab != null)
            {
                TransformMatterBlock(matterBlock, solidStatePrefab,
                    TilemapStateSynchronizer.solidSpriteReference,
                    TilemapStateSynchronizer.solidSpriteReferenceAlt);
            }
            else
            {
                Debug.LogWarning("Water: No solid state prefab assigned!");
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}