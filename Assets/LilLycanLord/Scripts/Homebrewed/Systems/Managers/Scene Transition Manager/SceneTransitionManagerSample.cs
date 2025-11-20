using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    public class SceneTransitionManagerSample : MonoBehaviour
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

        // [Header("Displays")]

        public void GoToA()
        {
            SceneTransitionManager.Instance.LoadSceneWithTransition("Transition A", "Crossfade");
        }

        public void GoToB()
        {
            SceneTransitionManager.Instance.LoadSceneWithTransition("Transition B", "Crossfade");
        }

        public void GoToC()
        {
            SceneTransitionManager.Instance.LoadSceneWithTransition("Transition C", "Crossfade");
        }
    }
}
