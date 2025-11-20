using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Example interactable object that demonstrates custom hold duration.
    /// This example shows how to override the global hold duration on a per-interaction basis.
    /// </summary>
    public class ExampleHoldInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField]
        private string interactionPrompt = "Hold to Activate";

        [SerializeField]
        [Tooltip("Hold duration in seconds (-1 = use global setting)")]
        [Range(-1f, 10f)]
        private float customHoldDuration = 2f;

        [SerializeField]
        private int interactionPriority = 5;

        [Header("Feedback")]
        [SerializeField]
        private bool enableDebugLogs = true;

        [SerializeField]
        private Color normalColor = Color.white;

        [SerializeField]
        private Color holdingColor = Color.yellow;

        [SerializeField]
        private Color completedColor = Color.green;

        private Renderer objectRenderer;
        private Color originalColor;
        private bool isBeingHeld = false;

        void Start()
        {
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                originalColor = objectRenderer.material.color;
                objectRenderer.material.color = normalColor;
            }

            // Register for hold events
            RegisterHoldEvents();
        }

        void OnDestroy()
        {
            UnregisterHoldEvents();
        }

        private void RegisterHoldEvents()
        {
            if (GameEventManager.Instance != null)
            {
                GameEventManager.Instance.AddAction("Interaction_HoldStarted", OnHoldStarted);
                GameEventManager.Instance.AddAction("Interaction_HoldProgress", OnHoldProgress);
                GameEventManager.Instance.AddAction("Interaction_HoldCompleted", OnHoldCompleted);
                GameEventManager.Instance.AddAction("Interaction_HoldCancelled", OnHoldCancelled);
            }
        }

        private void UnregisterHoldEvents()
        {
            if (GameEventManager.Instance != null)
            {
                GameEventManager.Instance.RemoveAction("Interaction_HoldStarted", OnHoldStarted);
                GameEventManager.Instance.RemoveAction("Interaction_HoldProgress", OnHoldProgress);
                GameEventManager.Instance.RemoveAction(
                    "Interaction_HoldCompleted",
                    OnHoldCompleted
                );
                GameEventManager.Instance.RemoveAction(
                    "Interaction_HoldCancelled",
                    OnHoldCancelled
                );
            }
        }

        public void Interact(bool held)
        {
            if (enableDebugLogs)
            {
                Debug.Log(
                    $"{name} interacted with! Hold Duration: {GetHoldDuration()}s (held: {held})"
                );
            }

            // This will be called when the hold duration is completed
            if (held && GetHoldDuration() > 0)
            {
                PerformHoldAction();
            }
            else if (!held && GetHoldDuration() <= 0)
            {
                PerformInstantAction();
            }
        }

        private void PerformInstantAction()
        {
            if (enableDebugLogs)
            {
                Debug.Log($"{name}: Performing instant action!");
            }

            // Flash green briefly
            if (objectRenderer != null)
            {
                StartCoroutine(FlashColor(completedColor, 0.3f));
            }
        }

        private void PerformHoldAction()
        {
            if (enableDebugLogs)
            {
                Debug.Log($"{name}: Hold action completed after {customHoldDuration}s!");
            }

            // Change to completed color
            if (objectRenderer != null)
            {
                objectRenderer.material.color = completedColor;
                StartCoroutine(ResetColorAfterDelay(1f));
            }
        }

        public void OnInteractionEnter()
        {
            if (enableDebugLogs)
            {
                Debug.Log($"{name}: Player entered interaction range");
            }
        }

        public void OnInteractionExit()
        {
            if (enableDebugLogs)
            {
                Debug.Log($"{name}: Player exited interaction range");
            }

            isBeingHeld = false;
            if (objectRenderer != null)
            {
                objectRenderer.material.color = normalColor;
            }
        }

        public bool CanInteract()
        {
            return enabled && gameObject.activeInHierarchy;
        }

        public int GetInteractionPriority()
        {
            return interactionPriority;
        }

        public string GetInteractionPrompt()
        {
            if (customHoldDuration > 0)
            {
                return $"{interactionPrompt} ({customHoldDuration}s)";
            }
            else if (customHoldDuration == 0)
            {
                return interactionPrompt.Replace("Hold", "Press");
            }
            else
            {
                return $"{interactionPrompt} (Global)";
            }
        }

        public float GetHoldDuration()
        {
            return customHoldDuration;
        }

        private void OnHoldStarted()
        {
            isBeingHeld = true;
            if (objectRenderer != null)
            {
                objectRenderer.material.color = holdingColor;
            }
        }

        private void OnHoldProgress()
        {
            // Visual feedback during hold progress
            if (isBeingHeld && objectRenderer != null)
            {
                // Pulsate between holding color and normal color
                float pulse = Mathf.Sin(Time.time * 10f) * 0.3f + 0.7f;
                objectRenderer.material.color = Color.Lerp(normalColor, holdingColor, pulse);
            }
        }

        private void OnHoldCompleted()
        {
            isBeingHeld = false;
        }

        private void OnHoldCancelled()
        {
            isBeingHeld = false;
            if (objectRenderer != null)
            {
                objectRenderer.material.color = normalColor;
            }
        }

        private System.Collections.IEnumerator FlashColor(Color flashColor, float duration)
        {
            if (objectRenderer != null)
            {
                Color startColor = objectRenderer.material.color;
                objectRenderer.material.color = flashColor;
                yield return new WaitForSeconds(duration);
                objectRenderer.material.color = startColor;
            }
        }

        private System.Collections.IEnumerator ResetColorAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (objectRenderer != null)
            {
                objectRenderer.material.color = normalColor;
            }
        }

        // Editor gizmo for visual feedback
        void OnDrawGizmosSelected()
        {
            Gizmos.color = customHoldDuration > 0 ? Color.yellow : Color.green;
            Gizmos.DrawWireSphere(transform.position, 2f);

            // Draw hold duration info
            if (customHoldDuration > 0)
            {
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 2f,
                    $"Hold: {customHoldDuration}s"
                );
            }
            else if (customHoldDuration == 0)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "Instant");
            }
            else
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "Global Setting");
            }
        }
    }
}
