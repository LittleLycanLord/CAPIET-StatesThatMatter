using UnityEngine;

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

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}