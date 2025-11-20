using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    public class DialogueInputManager : MonoBehaviour
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

        static DialogueInputManager instance;
        public static DialogueInputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject(
                        "Dialogue Input Manager"
                    ).AddComponent<DialogueInputManager>();
                }
                return instance;
            }
        }

        void Awake()
        {
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
            if (instance == null || instance != this)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            //* - - - - - Non - Singleton Awake Content - - - - -
            controls = new DialogueControls();
        }

        //! - - - - - - - - - - -

        DialogueControls controls;

        [Header("Displays")]
        [SerializeField]
        bool playerConfirm;

        [SerializeField]
        bool playerSkip;

        [Space(10)]
        [Header("Fields")]
        [SerializeField]
        bool debugMode = false;

        void Update()
        {
            if (!debugMode)
                return;
            playerConfirm = GetPlayerConfirmDialogue(true);
            playerSkip = GetPlayerSkip(true);
        }

        void OnEnable()
        {
            controls.Enable();
        }

        void OnDisable()
        {
            controls.Disable();
        }

        public bool GetPlayerConfirmDialogue(bool held)
        {
            if (held)
                return controls.Dialogue_Keyboard.Confirm.IsPressed();
            return controls.Dialogue_Keyboard.Confirm.WasPressedThisFrame();
        }

        public bool GetPlayerSkip(bool held)
        {
            if (held)
                return controls.Dialogue_Keyboard.Skip.IsPressed();
            return controls.Dialogue_Keyboard.Skip.WasPressedThisFrame();
        }
    }
}
