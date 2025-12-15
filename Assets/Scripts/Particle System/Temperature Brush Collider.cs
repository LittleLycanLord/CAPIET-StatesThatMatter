using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Helper component for the Temperature Brush's child collider object
    /// Forwards trigger events to the parent TemperatureBrush
    /// </summary>
    public class TemperatureBrushCollider : MonoBehaviour
    {
        private TemperatureBrush parentBrush;

        void Awake()
        {
            parentBrush = GetComponentInParent<TemperatureBrush>();
            if (parentBrush == null)
            {
                Debug.LogError("TemperatureBrushCollider: No TemperatureBrush found in parent!");
            }
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[BrushChild] Trigger Enter detected with: {other.gameObject.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
            
            if (parentBrush != null)
            {
                parentBrush.OnChildTriggerEnter(other);
            }
        }

        void OnTriggerExit(Collider other)
        {
            Debug.Log($"[BrushChild] Trigger Exit detected with: {other.gameObject.name}");
            
            if (parentBrush != null)
            {
                parentBrush.OnChildTriggerExit(other);
            }
        }
    }
}
