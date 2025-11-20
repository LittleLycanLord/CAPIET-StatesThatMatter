using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    [RequireComponent(typeof(BoxCollider))]
    public class PlayerIsInTrigger : MonoBehaviour, Interaction
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [HideInInspector]
        public BoxCollider boxCollider;

        [Header("Displays")]
        public bool playerIsInTrigger { get; private set; } = false;

        [SerializeField]
        float timePlayerIsInTrigger = 0.0f;

        public bool wasInteractedWith = false;

        [Space(10)]
        [Header("Fields")]
        [SerializeField]
        float durationBeforeTrue = 0.0f;

        bool staysInTrigger = false;

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔══════════════╗
        //* ║ Monobehaviour ║
        //* ╚══════════════╝

        void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.name.Contains("Player"))
                if (durationBeforeTrue == 0.0f)
                    playerIsInTrigger = true;
                else
                    staysInTrigger = true;
        }

        void Update()
        {
            if (durationBeforeTrue > 0.0f && staysInTrigger)
            {
                timePlayerIsInTrigger += Time.deltaTime;
                playerIsInTrigger = timePlayerIsInTrigger >= durationBeforeTrue;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.name.Contains("Player"))
            {
                playerIsInTrigger = false;
                staysInTrigger = false;
                timePlayerIsInTrigger = 0.0f;
            }
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        public void Interact(bool held)
        {
            if (!held)
                wasInteractedWith = playerIsInTrigger;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
