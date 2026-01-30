using UnityEngine;

namespace LilLycanLord_Official
{
    [CreateAssetMenu(fileName = "New Particle Lattice Material", menuName = "States That Matter/Particle Lattice Material")]
    public class ParticleLatticeMaterial : ScriptableObject
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
        [Header("Phase Presets")]
        public ParticleLatticePreset solidPreset;
        public ParticleLatticePreset liquidPreset;
        public ParticleLatticePreset gasPreset;
        
        [Space(10)]
        [Header("Phase Change Behavior")]
        public ParticleLatticeMaterialPhaseChangeEvents phaseChangeEvents;
        
        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        public ParticleLatticePreset GetPresetForPhase(MatterPhase phase)
        {
            switch (phase)
            {
                case MatterPhase.Solid:
                    return solidPreset;
                case MatterPhase.Liquid:
                    return liquidPreset;
                case MatterPhase.Gas:
                    return gasPreset;
                default:
                    return null;
            }
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}