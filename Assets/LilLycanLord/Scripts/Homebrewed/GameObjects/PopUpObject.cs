using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace LilLycanLord_Official
{
    public class PopUpObject : MonoBehaviour
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

        [Header("Displays")]
        [Space(10)]
        [Header("Fields")]
        [SerializeField]
        Vector3 popUpOffset = new Vector3();
        public Vector3 popUpScale = new Vector3(1.0f, 1.0f, 1.0f);
        public bool destroyOnShrink = false;

        [Header("Timing")]
        [SerializeField]
        float popUpDuration = 0.4f;

        [SerializeField]
        float stayDuration = 0.4f;

        [SerializeField]
        float shrinkDownDuration = 0.4f;

        [SerializeField]
        UnityEvent onPopUp;

        [SerializeField]
        UnityEvent onShrinkDown;

        void Awake() { }

        void Start()
        {
            transform.localScale = new Vector3();
        }

        void Update() { }

        void ShrinkDown()
        {
            transform.LeanScale(new Vector3(), shrinkDownDuration);
            Invoke("RevertOffset", shrinkDownDuration);
        }

        void RevertOffset()
        {
            transform.position -= popUpOffset;
            onShrinkDown?.Invoke();
            if (destroyOnShrink)
                Destroy(gameObject);
        }

        [ContextMenu("Pop Up")]
        public void PopUp()
        {
            onPopUp?.Invoke();
            transform.position += popUpOffset;
            transform.LeanScale(popUpScale, popUpDuration);
            Invoke("ShrinkDown", popUpDuration + stayDuration);
        }
    }
}
