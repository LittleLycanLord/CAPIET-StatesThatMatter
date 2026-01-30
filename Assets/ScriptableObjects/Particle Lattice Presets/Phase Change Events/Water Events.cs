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
            Debug.Log($"Water: Ice melted! Destroying {matterBlock.name}");
            Object.Destroy(matterBlock);
        }
        
        public override void OnFreeze(GameObject matterBlock)
        {
            Debug.Log($"Water: Water froze! {matterBlock.name} is now ice");
            // TODO: Implement freeze behavior (e.g., change sprite, play sound)
        }
        
        public override void OnEvaporate(GameObject matterBlock)
        {
            Debug.Log($"Water: Water evaporated! Destroying {matterBlock.name}");
            Object.Destroy(matterBlock);
        }
        
        public override void OnCondense(GameObject matterBlock)
        {
            Debug.Log($"Water: Steam condensed! {matterBlock.name} is now water");
            // TODO: Implement condensation behavior
        }
        
        public override void OnSublimate(GameObject matterBlock)
        {
            Debug.Log($"Water: Ice sublimated! Destroying {matterBlock.name}");
            Object.Destroy(matterBlock);
        }
        
        public override void OnDeposition(GameObject matterBlock)
        {
            Debug.Log($"Water: Steam deposited! {matterBlock.name} is now ice");
            // TODO: Implement deposition behavior
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}