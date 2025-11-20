using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    public class HoverableSample : MonoBehaviour
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
        void Awake() { }

        void Start() { }

        void Update() { }

        //* ╔═════════════════════╗
        //* ║ Non - Monobehaviour ║
        //* ╚═════════════════════╝
        public void SampleHoverOn()
        {
            Debug.Log("Hover started on: " + gameObject.name);
        }

        public void SampleHoverOff()
        {
            Debug.Log("Hover ended on: " + gameObject.name);
        }

        public void SampleHoverOneSecond()
        {
            Debug.Log("Hovering for one second on: " + gameObject.name);
        }

        public void SampleHoverFiveSeconds()
        {
            Debug.Log("Hovering for five seconds on: " + gameObject.name);
        }

        public void SampleHoverTenSeconds()
        {
            Debug.Log("Hovering for ten seconds on: " + gameObject.name);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
