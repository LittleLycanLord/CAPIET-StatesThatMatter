using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    public class SurveyTrigger : MonoBehaviour
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
        [Header("Survey Settings")]
        [SerializeField] private string surveyURL = "https://forms.gle/XQZbHiHwFywy8nS58";
        
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
        /// Opens the default survey URL in the device's browser.
        /// Can be called from Unity Events (e.g., Button onClick).
        /// </summary>
        public void OpenSurvey()
        {
            OpenURL(surveyURL);
        }
        
        /// <summary>
        /// Opens a specific URL in the device's browser.
        /// Works on all platforms including mobile devices.
        /// </summary>
        /// <param name="url">The URL to open</param>
        public void OpenURL(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning("SurveyTrigger: Cannot open empty URL!");
                return;
            }
            
            Debug.Log($"Opening URL: {url}");
            Application.OpenURL(url);
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}