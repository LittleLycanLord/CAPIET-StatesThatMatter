using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LilLycanLord_Official
{
    /// <summary>
    /// Creates a pixelated camera effect using a dual camera system.
    /// The original camera remains untouched for perfect raycasting compatibility,
    /// while a separate pixel camera handles the low-resolution rendering.
    /// Works in both editor mode and play mode with asset-based RenderTextures.
    /// </summary>
    [System.Serializable]
    [ExecuteInEditMode]
    public class PixelViewMaker : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        [Header("Camera Settings")]
        [SerializeField]
        private Camera targetCamera;

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        [Header("Pixel View Settings")]
        [SerializeField]
        private Vector2Int _pixelResolution = new Vector2Int(192, 108);

        [SerializeField]
        private FilterMode filterMode = FilterMode.Point;

        [SerializeField]
        private int renderTextureDepth = 24;

        [SerializeField]
        [Tooltip("Canvas render mode for the pixel view overlay")]
        private RenderMode canvasRenderMode = RenderMode.ScreenSpaceOverlay;

        [Header("Debug Settings")]
        [SerializeField]
        private bool showDebugInfo = true;

        [SerializeField]
        private bool showMouseRayGizmo = true;

        [Header("Runtime Info (Read-Only)")]
        [SerializeField]
        [Tooltip("Whether pixel view is currently active")]
        private bool isPixelViewActive = false;

        [SerializeField]
        [Tooltip("Current screen resolution")]
        private Vector2Int currentResolution;

        // Private runtime variables
        private RenderTexture pixelRenderTexture;
        private GameObject canvasObject;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private RawImage displayImage;
        private Camera pixelCamera;

        //* ╔═══════════╗
        //* ║ Properties ║
        //* ╚═══════════╝

        /// <summary>
        /// The target camera for pixel view effects
        /// </summary>
        public Camera TargetCamera => targetCamera;

        /// <summary>
        /// Access to the original camera for raycasting (never modified by pixel view)
        /// Use this for Hoverable scripts and other raycast operations
        /// </summary>
        public Camera OriginalCamera => targetCamera;

        /// <summary>
        /// The pixel resolution for the render texture (automatically clamped to reasonable values)
        /// </summary>
        public Vector2Int pixelResolution
        {
            get => _pixelResolution;
            set
            {
                _pixelResolution = new Vector2Int(
                    Mathf.Clamp(value.x, 64, 1920),
                    Mathf.Clamp(value.y, 64, 1080)
                );
            }
        }

        /// <summary>
        /// Gets whether the pixel view is currently active
        /// </summary>
        public bool IsPixelViewActive => isPixelViewActive;

        /// <summary>
        /// Gets the current render texture asset (null if not active or in build)
        /// </summary>
        public RenderTexture CurrentRenderTexture => pixelRenderTexture;

#if UNITY_EDITOR
        /// <summary>
        /// Gets the asset path of the current render texture (editor only)
        /// </summary>
        public string RenderTextureAssetPath
        {
            get
            {
                if (pixelRenderTexture != null)
                {
                    return UnityEditor.AssetDatabase.GetAssetPath(pixelRenderTexture);
                }
                return null;
            }
        }
#endif

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Awake()
        {
#if UNITY_EDITOR
            // Clean up any editor-time pixel view effects when entering play mode
            if (Application.isPlaying && isPixelViewActive)
            {
                RemovePixelView();
            }
#endif
        }

        void Update()
        {
            // Handle resolution changes to maintain constant pixel size
            if (isPixelViewActive && pixelRenderTexture != null)
            {
                Vector2Int screenRes = new Vector2Int(Screen.width, Screen.height);
                if (screenRes != currentResolution)
                {
                    currentResolution = screenRes;
                    UpdateCanvasScaling();
                }

                // Keep pixel camera in sync with original camera
                UpdatePixelCamera();
            }
        }

        void OnDestroy()
        {
            // Clean up when destroyed
            if (isPixelViewActive)
            {
                RemovePixelView();
            }
        }

        /// <summary>
        /// Applies the pixel view effect using a dual camera system.
        /// Creates a separate pixel camera and render texture while keeping the original camera intact.
        /// </summary>
        public void ApplyPixelView()
        {
            if (targetCamera == null)
            {
                Debug.LogError("PixelViewMaker: No target camera assigned!");
                return;
            }

            if (isPixelViewActive)
            {
                Debug.LogWarning("PixelViewMaker: Pixel view is already active!");
                return;
            }

            CreateRenderTexture();
            CreatePixelCamera();
            CreateCanvas();
            SetupDisplayImage();

            // Original camera remains unchanged - pixel camera handles the rendering
            isPixelViewActive = true;
            currentResolution = new Vector2Int(Screen.width, Screen.height);

#if UNITY_EDITOR
            // Clear console after applying pixel view
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
#endif

            if (showDebugInfo)
            {
                Debug.Log(
                    $"PixelViewMaker: Applied dual camera pixel view with resolution {pixelResolution.x}x{pixelResolution.y}"
                );
            }
        }

        /// <summary>
        /// Removes the pixel view effect and cleans up all created objects.
        /// Original camera is verified to remain unchanged with dual camera system.
        /// </summary>
        public void RemovePixelView()
        {
            if (showDebugInfo)
            {
                Debug.Log("PixelViewMaker: Removing dual camera pixel view and cleaning up...");
            }

            // With dual camera system, original camera should never have been modified
            if (targetCamera != null && targetCamera.targetTexture != null)
            {
                Debug.LogWarning(
                    "PixelViewMaker: Original camera had targetTexture assigned - this shouldn't happen with dual camera system!"
                );
                targetCamera.targetTexture = null;
            }

            // Clean up created objects in proper order
            DestroyPixelCamera();
            DestroyCanvas();
            DestroyRenderTexture();

            // Reset state variables
            isPixelViewActive = false;
            currentResolution = Vector2Int.zero;

            if (showDebugInfo)
            {
                Debug.Log("PixelViewMaker: Successfully removed dual camera pixel view");
            }
        }

        /// <summary>
        /// Manually refreshes the pixel view display (useful after runtime changes)
        /// </summary>
        public void RefreshPixelView()
        {
            if (isPixelViewActive && displayImage != null)
            {
                // With RawImage, the texture is automatically updated
                // But we can force a refresh by reassigning the texture
                displayImage.texture = pixelRenderTexture;
            }
        }

        /// <summary>
        /// Creates a ray from screen position using the original camera.
        /// Works perfectly with dual camera system - no coordinate conversion needed.
        /// </summary>
        /// <param name="screenPosition">Screen position in pixels</param>
        /// <returns>Ray from the original camera</returns>
        public Ray GetRayFromScreen(Vector3 screenPosition)
        {
            if (targetCamera == null)
            {
                Debug.LogError("PixelViewMaker: No target camera assigned for raycast operations!");
                return new Ray();
            }

            // With dual camera system, original camera is never modified
            // So raycasting always works correctly regardless of pixel view state
            return targetCamera.ScreenPointToRay(screenPosition);
        }

        /// <summary>
        /// Converts screen position to world position using the original camera.
        /// Works perfectly with dual camera system - no coordinate conversion needed.
        /// </summary>
        /// <param name="screenPosition">Screen position in pixels</param>
        /// <returns>World position from the original camera</returns>
        public Vector3 ScreenToWorldPoint(Vector3 screenPosition)
        {
            if (targetCamera == null)
            {
                Debug.LogError(
                    "PixelViewMaker: No target camera assigned for world point conversion!"
                );
                return Vector3.zero;
            }

            // With dual camera system, original camera is never modified
            // So coordinate conversion always works correctly regardless of pixel view state
            return targetCamera.ScreenToWorldPoint(screenPosition);
        }

        /// <summary>
        /// Performs a raycast from screen position using the original camera.
        /// Transparent wrapper that works seamlessly with dual camera system.
        /// </summary>
        /// <param name="screenPosition">Screen position in pixels</param>
        /// <param name="hit">Raycast hit information</param>
        /// <param name="maxDistance">Maximum raycast distance</param>
        /// <param name="layerMask">Layer mask for raycast</param>
        /// <returns>True if raycast hit something</returns>
        public bool RaycastFromScreen(
            Vector3 screenPosition,
            out RaycastHit hit,
            float maxDistance = 100f,
            int layerMask = -1
        )
        {
            hit = new RaycastHit();

            if (targetCamera == null)
            {
                Debug.LogError("PixelViewMaker: No target camera assigned for raycast operations!");
                return false;
            }

            // Use GetRayFromScreen for consistent dual camera behavior
            Ray ray = GetRayFromScreen(screenPosition);
            return Physics.Raycast(ray, out hit, maxDistance, layerMask);
        }

        private void CreatePixelCamera()
        {
            if (targetCamera == null)
            {
                Debug.LogError("PixelViewMaker: Cannot create pixel camera without target camera!");
                return;
            }

            // Create pixel camera as child of original camera for organization
            string pixelCameraName = $"PixelView_Camera_{pixelResolution.x}x{pixelResolution.y}";
            GameObject pixelCameraObject = new GameObject(pixelCameraName);
            pixelCameraObject.transform.SetParent(targetCamera.transform, false);

#if UNITY_EDITOR
            // Mark the scene as dirty so changes are saved
            EditorUtility.SetDirty(targetCamera.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
#endif

            // Add camera component and copy all settings from original camera
            pixelCamera = pixelCameraObject.AddComponent<Camera>();
            CopyCamera(targetCamera, pixelCamera);

            // Assign render texture to pixel camera only
            pixelCamera.targetTexture = pixelRenderTexture;

            // Set pixel camera to render after original camera but before UI
            pixelCamera.depth = targetCamera.depth + 0.1f;

            // Disable pixel camera's audio listener to avoid conflicts
            AudioListener pixelAudioListener = pixelCamera.GetComponent<AudioListener>();
            if (pixelAudioListener != null)
            {
                pixelAudioListener.enabled = false;
            }

            if (showDebugInfo)
            {
                Debug.Log(
                    $"PixelViewMaker: Created pixel camera '{pixelCameraName}' with render texture"
                );
            }
        }

        private void CopyCamera(Camera source, Camera destination)
        {
            // Copy all camera settings to keep them in sync
            destination.clearFlags = source.clearFlags;
            destination.backgroundColor = source.backgroundColor;
            destination.cullingMask = source.cullingMask;
            destination.orthographic = source.orthographic;
            destination.fieldOfView = source.fieldOfView;
            destination.orthographicSize = source.orthographicSize;
            destination.nearClipPlane = source.nearClipPlane;
            destination.farClipPlane = source.farClipPlane;
            destination.rect = source.rect;
            destination.renderingPath = source.renderingPath;
            destination.useOcclusionCulling = source.useOcclusionCulling;
            destination.allowHDR = source.allowHDR;
            destination.allowMSAA = source.allowMSAA;
            destination.allowDynamicResolution = source.allowDynamicResolution;

            // Copy transform (should already be copied by parenting, but ensure accuracy)
            destination.transform.localPosition = Vector3.zero;
            destination.transform.localRotation = Quaternion.identity;
            destination.transform.localScale = Vector3.one;
        }

        private void UpdatePixelCamera()
        {
            if (pixelCamera != null && targetCamera != null)
            {
                // Keep pixel camera in sync with original camera
                CopyCamera(targetCamera, pixelCamera);
                // Restore render texture assignment
                pixelCamera.targetTexture = pixelRenderTexture;
                // Restore depth offset
                pixelCamera.depth = targetCamera.depth + 0.1f;
            }
        }

        private void DestroyPixelCamera()
        {
            if (pixelCamera != null)
            {
                GameObject pixelCameraObject = pixelCamera.gameObject;

                if (Application.isPlaying)
                {
                    Destroy(pixelCameraObject);
                }
                else
                {
                    DestroyImmediate(pixelCameraObject);
                }

                pixelCamera = null;

                if (showDebugInfo)
                {
                    Debug.Log("PixelViewMaker: Destroyed pixel camera");
                }
                return;
            }

            // Fallback: Find and destroy pixel camera by name if reference is lost
            if (targetCamera != null)
            {
                string expectedCameraName =
                    $"PixelView_Camera_{pixelResolution.x}x{pixelResolution.y}";
                Transform cameraTransform = targetCamera.transform.Find(expectedCameraName);

                if (cameraTransform != null && cameraTransform.GetComponent<Camera>() != null)
                {
                    GameObject foundCamera = cameraTransform.gameObject;
                    if (Application.isPlaying)
                    {
                        Destroy(foundCamera);
                    }
                    else
                    {
                        DestroyImmediate(foundCamera);
                    }

                    if (showDebugInfo)
                    {
                        Debug.Log(
                            $"PixelViewMaker: Found and destroyed orphaned pixel camera '{expectedCameraName}'"
                        );
                    }
                }
            }
        }

        private void CreateRenderTexture()
        {
            // Clean up existing render texture
            if (pixelRenderTexture != null)
            {
                DestroyRenderTexture();
            }

#if UNITY_EDITOR
            // Create render texture asset in the project folder
            string folderPath = "Assets/LilLycanLord/Sprites";
            string fileName = $"PixelGameView({pixelResolution.x}x{pixelResolution.y})";
            string assetPath = $"{folderPath}/{fileName}.renderTexture";

            // Ensure the folder exists
            if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
            {
                // Create the folder structure if it doesn't exist
                string[] folders = folderPath.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string parentPath = currentPath;
                    currentPath += "/" + folders[i];
                    if (!UnityEditor.AssetDatabase.IsValidFolder(currentPath))
                    {
                        UnityEditor.AssetDatabase.CreateFolder(parentPath, folders[i]);
                    }
                }
            }

            // Check if render texture asset already exists
            pixelRenderTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<RenderTexture>(
                assetPath
            );

            if (pixelRenderTexture == null)
            {
                // Create new render texture asset
                pixelRenderTexture = new RenderTexture(
                    pixelResolution.x,
                    pixelResolution.y,
                    renderTextureDepth
                )
                {
                    filterMode = filterMode,
                    name = fileName,
                };

                // Save as asset
                UnityEditor.AssetDatabase.CreateAsset(pixelRenderTexture, assetPath);
                UnityEditor.AssetDatabase.SaveAssets();

                if (showDebugInfo)
                {
                    Debug.Log($"PixelViewMaker: Created new RenderTexture asset at {assetPath}");
                }
            }
            else
            {
                // Update existing asset properties if needed
                if (
                    pixelRenderTexture.width != pixelResolution.x
                    || pixelRenderTexture.height != pixelResolution.y
                    || pixelRenderTexture.depth != renderTextureDepth
                    || pixelRenderTexture.filterMode != filterMode
                )
                {
                    pixelRenderTexture.Release();
                    pixelRenderTexture.width = pixelResolution.x;
                    pixelRenderTexture.height = pixelResolution.y;
                    pixelRenderTexture.depth = renderTextureDepth;
                    pixelRenderTexture.filterMode = filterMode;

                    UnityEditor.EditorUtility.SetDirty(pixelRenderTexture);

                    if (showDebugInfo)
                    {
                        Debug.Log(
                            $"PixelViewMaker: Updated existing RenderTexture asset at {assetPath}"
                        );
                    }
                }
                else if (showDebugInfo)
                {
                    Debug.Log($"PixelViewMaker: Using existing RenderTexture asset at {assetPath}");
                }
            }

            // Ensure the render texture is created/recreated
            if (!pixelRenderTexture.IsCreated())
            {
                pixelRenderTexture.Create();
            }
#else
            // In builds, create a runtime render texture as fallback
            pixelRenderTexture = new RenderTexture(
                pixelResolution.x,
                pixelResolution.y,
                renderTextureDepth
            )
            {
                filterMode = filterMode,
                name = $"PixelGameView({pixelResolution.x}x{pixelResolution.y})",
            };

            pixelRenderTexture.Create();
#endif
        }

        private void CreateCanvas()
        {
            // Create canvas as child of camera for organization
            string canvasName = $"PixelView_Canvas_{pixelResolution.x}x{pixelResolution.y}";
            canvasObject = new GameObject(canvasName);

            // Parent to camera for organization
            if (targetCamera != null)
            {
                canvasObject.transform.SetParent(targetCamera.transform, false);
            }

#if UNITY_EDITOR
            // Mark the scene as dirty so changes are saved
            EditorUtility.SetDirty(targetCamera.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
#endif

            // Setup canvas component
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = canvasRenderMode;
            canvas.sortingOrder = -1; // Render behind UI but on top of world

            // Add CanvasGroup for better raycast control
            canvasGroup = canvasObject.AddComponent<CanvasGroup>();

            // Add canvas scaler for consistent pixel size
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(pixelResolution.x, pixelResolution.y);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // Balanced scaling

            // Never add GraphicRaycaster - we always want raycast passthrough for visual-only effect

            // Apply raycast settings
            UpdateRaycastSettings();
        }

        private void SetupDisplayImage()
        {
            // Create image object
            GameObject imageObject = new GameObject("PixelView_Image");
            imageObject.transform.SetParent(canvasObject.transform, false);

            // Setup RawImage component (much more efficient than Image with sprite)
            displayImage = imageObject.AddComponent<RawImage>();

            // Make image non-raycast to preserve camera raycasting
            displayImage.raycastTarget = false;

            // Directly assign the render texture to the RawImage
            displayImage.texture = pixelRenderTexture;

            // Setup RectTransform to fill screen
            RectTransform rectTransform = displayImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // No need for coroutines or manual texture updates with RawImage!
            // The RawImage automatically displays the RenderTexture content in real-time
        }

        private void UpdateRaycastSettings()
        {
            if (canvasGroup != null)
            {
                // Always allow raycasts to pass through for visual-only effect
                canvasGroup.blocksRaycasts = false;
            }

            if (displayImage != null)
            {
                // Always disable raycast target on the RawImage itself for maximum passthrough
                displayImage.raycastTarget = false;
            }

            // Remove any GraphicRaycaster to ensure complete passthrough
            GraphicRaycaster raycaster = canvasObject?.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                // Remove GraphicRaycaster to ensure complete passthrough
                if (Application.isPlaying)
                {
                    Destroy(raycaster);
                }
                else
                {
                    DestroyImmediate(raycaster);
                }
            }
        }

        private void UpdateCanvasScaling()
        {
            if (canvas != null)
            {
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    // Maintain constant pixel size regardless of screen resolution
                    float screenAspect = (float)Screen.width / Screen.height;
                    float targetAspect = (float)pixelResolution.x / pixelResolution.y;

                    if (screenAspect > targetAspect)
                    {
                        scaler.matchWidthOrHeight = 1f; // Match height
                    }
                    else
                    {
                        scaler.matchWidthOrHeight = 0f; // Match width
                    }
                }
            }
        }

        private void DestroyCanvas()
        {
            // First try to destroy the stored reference
            if (canvasObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(canvasObject);
                }
                else
                {
                    DestroyImmediate(canvasObject);
                }
                canvasObject = null;
                canvas = null;
                canvasGroup = null;
                displayImage = null;
                return;
            }

            // Fallback: Find and destroy canvas by name if reference is lost
            if (targetCamera != null)
            {
                string expectedCanvasName =
                    $"PixelView_Canvas_{pixelResolution.x}x{pixelResolution.y}";
                Transform canvasTransform = targetCamera.transform.Find(expectedCanvasName);

                if (canvasTransform != null && canvasTransform.GetComponent<Canvas>() != null)
                {
                    GameObject foundCanvas = canvasTransform.gameObject;
                    if (Application.isPlaying)
                    {
                        Destroy(foundCanvas);
                    }
                    else
                    {
                        DestroyImmediate(foundCanvas);
                    }
                }
            }
        }

        private void DestroyRenderTexture()
        {
            if (pixelRenderTexture != null)
            {
                if (pixelRenderTexture.IsCreated())
                {
                    pixelRenderTexture.Release();
                }

#if UNITY_EDITOR
                // Don't destroy asset-based render textures, just release them
                // The asset will remain in the project for reuse
                if (showDebugInfo)
                {
                    Debug.Log(
                        $"PixelViewMaker: Released RenderTexture asset '{pixelRenderTexture.name}'"
                    );
                }
#else
                // In builds, destroy runtime render textures
                if (Application.isPlaying)
                {
                    Destroy(pixelRenderTexture);
                }
                else
                {
                    DestroyImmediate(pixelRenderTexture);
                }
#endif
                pixelRenderTexture = null;
            }
        }

        //* ╔═══════════════╗
        //* ║ Editor Gizmos ║
        //* ╚═══════════════╝

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!showMouseRayGizmo || targetCamera == null)
                return;

            // Get current mouse position in screen coordinates
            Vector3 mousePosition = Input.mousePosition;

            // Create ray from our method (now always works correctly with dual camera system)
            Ray ourRay = GetRayFromScreen(mousePosition);

            // For comparison: what would happen with the old single-camera approach
            Ray oldApproachRay;
            if (isPixelViewActive && pixelCamera != null)
            {
                // Simulate the old approach where main camera would have had the render texture
                oldApproachRay = pixelCamera.ScreenPointToRay(mousePosition);
            }
            else
            {
                oldApproachRay = ourRay; // Same when pixel view is off
            }

            // Always draw the GREEN ray (our dual camera system - always correct)
            Gizmos.color = Color.green;
            Vector3 ourEndPoint = ourRay.origin + ourRay.direction * 5f;
            Gizmos.DrawLine(ourRay.origin, ourEndPoint);
            Gizmos.DrawSphere(ourEndPoint, 0.12f);

            // Draw the RED ray (what the old single-camera approach would give)
            if (isPixelViewActive && pixelCamera != null)
            {
                Gizmos.color = Color.red;
                Vector3 oldEndPoint = oldApproachRay.origin + oldApproachRay.direction * 5f;
                Gizmos.DrawLine(oldApproachRay.origin, oldEndPoint);
                Gizmos.DrawSphere(oldEndPoint, 0.06f);
            }

            // Draw a small sphere at the camera origin for reference
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(targetCamera.transform.position, 0.03f);

            // Debug raycast collision detection using our correct method
            if (Physics.Raycast(ourRay, out RaycastHit hit, Mathf.Infinity))
            {
                // Draw hit point
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(hit.point, 0.05f);

                // Show hit info in scene view
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(
                    hit.point + Vector3.up * 0.1f,
                    $"Hit: {hit.collider.name}\nDistance: {hit.distance:F2}\nDual Camera System: Working!"
                );
            }
        }
#endif
    }
}
