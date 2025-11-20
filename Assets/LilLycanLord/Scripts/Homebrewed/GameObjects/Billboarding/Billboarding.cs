using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;

namespace LilLycanLord_Official
{
    [System.Serializable]
    public struct BillboardPerformanceInfo
    {
        public int UpdateCount;
        public float LastUpdateTime;
        public float CurrentFrameRate;
        public float DistanceToCamera;
        public bool IsActiveAndEnabled;
    }

    public class Billboarding : MonoBehaviour
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

        [Header("Billboard Status")]
        [SerializeField]
        [Tooltip("Is billboarding currently active?")]
        private bool isActive = true;

        [SerializeField]
        [Tooltip("Show debug information in the inspector")]
        private bool showDebugInfo = false;

        [Space(10)]
        [Header("Camera Configuration")]
        [SerializeField]
        [Tooltip("Target camera to face (null = auto-detect)")]
        private Camera targetCamera;

        [SerializeField]
        [Tooltip("How to find the target camera")]
        private CameraDetectionMode cameraDetection = CameraDetectionMode.MainCamera;

        [SerializeField]
        [Tooltip("Update camera reference every frame (performance cost)")]
        private bool dynamicCameraDetection = false;

        [Space(10)]
        [Header("Billboard Behavior")]
        [SerializeField]
        [Tooltip("Type of billboarding to perform")]
        private BillboardMode billboardMode = BillboardMode.ScreenAligned;

        [SerializeField]
        [Tooltip("How to handle the up vector")]
        private UpVectorMode upVectorMode = UpVectorMode.CameraUp;

        [SerializeField]
        [Tooltip("Custom up vector (when using Custom mode)")]
        private Vector3 customUpVector = Vector3.up;

        [Space(10)]
        [Header("Axis Constraints")]
        [SerializeField]
        [Tooltip("Lock rotation on X axis")]
        private bool freezeXAxis = false;

        [SerializeField]
        [Tooltip("Lock rotation on Y axis")]
        private bool freezeYAxis = false;

        [SerializeField]
        [Tooltip("Lock rotation on Z axis")]
        private bool freezeZAxis = false;

        [Space(10)]
        [Header("Performance Settings")]
        [SerializeField]
        [Tooltip("Update frequency for billboarding")]
        private UpdateFrequency updateFrequency = UpdateFrequency.EveryFrame;

        [SerializeField]
        [Tooltip("Custom update interval in seconds (for TimedUpdate mode)")]
        [Range(0.01f, 1f)]
        private float customUpdateInterval = 0.1f;

        [SerializeField]
        [Tooltip("Minimum distance change to trigger update")]
        [Min(0f)]
        private float distanceThreshold = 0.5f;

        //* Performance tracking
        private float lastUpdateTime = 0f;
        private Vector3 lastCameraPosition;
        private Camera lastUsedCamera;
        private Quaternion originalRotation;

        //* Debug information
        private float lastDistanceToCamera = 0f;
        private int updateCount = 0;

        void Awake()
        {
            //* Store original rotation for reference
            originalRotation = transform.rotation;

            //* Initialize camera detection
            InitializeCamera();
        }

        void Start()
        {
            //* Ensure camera is properly set up
            if (targetCamera == null)
            {
                FindTargetCamera();
            }

            //* Initialize position tracking
            if (targetCamera != null)
            {
                lastCameraPosition = targetCamera.transform.position;
                lastDistanceToCamera = Vector3.Distance(
                    transform.position,
                    targetCamera.transform.position
                );
            }
        }

        void LateUpdate()
        {
            //* Only update if billboarding is active
            if (!isActive)
                return;

            //* Check if we should update based on frequency
            if (ShouldUpdate())
            {
                UpdateBillboard();
            }
        }

        void OnValidate()
        {
            //* Ensure valid settings in editor
            customUpdateInterval = Mathf.Max(0.01f, customUpdateInterval);
            distanceThreshold = Mathf.Max(0f, distanceThreshold);
        }

        private void InitializeCamera()
        {
            if (targetCamera == null)
            {
                FindTargetCamera();
            }
        }

        private void FindTargetCamera()
        {
            switch (cameraDetection)
            {
                case CameraDetectionMode.MainCamera:
                    targetCamera = Camera.main;
                    break;

                case CameraDetectionMode.ActiveCamera:
                    //* Find any active camera
                    Camera[] cameras = FindObjectsByType<Camera>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );
                    targetCamera = cameras.Length > 0 ? cameras[0] : null;
                    break;

                case CameraDetectionMode.TaggedCamera:
                    //* Find camera with "MainCamera" tag
                    GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
                    targetCamera = cameraObj?.GetComponent<Camera>();
                    break;

                case CameraDetectionMode.ClosestCamera:
                    targetCamera = FindClosestCamera();
                    break;

                case CameraDetectionMode.Manual:
                    //* Use manually assigned camera
                    break;
            }

            if (targetCamera == null && cameraDetection != CameraDetectionMode.Manual)
            {
                Debug.LogWarning(
                    $"Billboarding '{gameObject.name}': No camera found with detection mode '{cameraDetection}'"
                );
            }
        }

        private Camera FindClosestCamera()
        {
            Camera[] cameras = FindObjectsByType<Camera>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );
            Camera closest = null;
            float closestDistance = float.MaxValue;

            foreach (Camera cam in cameras)
            {
                float distance = Vector3.Distance(transform.position, cam.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = cam;
                }
            }

            return closest;
        }

        private bool ShouldUpdate()
        {
            if (targetCamera == null)
            {
                if (dynamicCameraDetection)
                {
                    FindTargetCamera();
                }
                return false;
            }

            switch (updateFrequency)
            {
                case UpdateFrequency.EveryFrame:
                    return true;

                case UpdateFrequency.TimedUpdate:
                    if (Time.time - lastUpdateTime >= customUpdateInterval)
                    {
                        lastUpdateTime = Time.time;
                        return true;
                    }
                    return false;

                case UpdateFrequency.DistanceBased:
                    float currentDistance = Vector3.Distance(
                        transform.position,
                        targetCamera.transform.position
                    );
                    if (Mathf.Abs(currentDistance - lastDistanceToCamera) >= distanceThreshold)
                    {
                        lastDistanceToCamera = currentDistance;
                        return true;
                    }
                    return false;

                case UpdateFrequency.OnCameraMove:
                    Vector3 currentCameraPos = targetCamera.transform.position;
                    if (Vector3.Distance(currentCameraPos, lastCameraPosition) >= distanceThreshold)
                    {
                        lastCameraPosition = currentCameraPos;
                        return true;
                    }
                    return false;

                default:
                    return true;
            }
        }

        private void UpdateBillboard()
        {
            if (targetCamera == null)
                return;

            Vector3 targetPosition = targetCamera.transform.position;
            Vector3 currentPosition = transform.position;

            Quaternion newRotation = CalculateBillboardRotation(targetPosition, currentPosition);

            //* Apply axis constraints
            newRotation = ApplyAxisConstraints(newRotation);

            //* Apply the rotation
            transform.rotation = newRotation;

            updateCount++;
        }

        private Quaternion CalculateBillboardRotation(
            Vector3 cameraPosition,
            Vector3 objectPosition
        )
        {
            switch (billboardMode)
            {
                case BillboardMode.LookAtCamera:
                    return CalculateLookAtRotation(cameraPosition, objectPosition);

                case BillboardMode.LookAtCameraInverted:
                    //* Look away from camera
                    Vector3 invertedDirection = objectPosition - cameraPosition;
                    return Quaternion.LookRotation(invertedDirection, GetUpVector());

                case BillboardMode.MatchCameraRotation:
                    //* Match camera's rotation exactly
                    return targetCamera.transform.rotation;

                case BillboardMode.YAxisOnly:
                    //* Only rotate around Y axis
                    Vector3 directionY = (cameraPosition - objectPosition);
                    directionY.y = 0; //* Flatten to XZ plane
                    return directionY.magnitude > 0.001f
                        ? Quaternion.LookRotation(directionY, Vector3.up)
                        : transform.rotation;

                case BillboardMode.ScreenAligned:
                    //* Align with camera's forward but maintain world up
                    Vector3 forward = targetCamera.transform.forward;
                    return Quaternion.LookRotation(forward, Vector3.up);

                default:
                    return CalculateLookAtRotation(cameraPosition, objectPosition);
            }
        }

        private Quaternion CalculateLookAtRotation(Vector3 cameraPosition, Vector3 objectPosition)
        {
            Vector3 direction = (cameraPosition - objectPosition).normalized;
            if (direction.magnitude < 0.001f)
                return transform.rotation;

            return Quaternion.LookRotation(direction, GetUpVector());
        }

        private Vector3 GetUpVector()
        {
            switch (upVectorMode)
            {
                case UpVectorMode.WorldUp:
                    return Vector3.up;

                case UpVectorMode.CameraUp:
                    return targetCamera != null ? targetCamera.transform.up : Vector3.up;

                case UpVectorMode.ObjectUp:
                    return transform.up;

                case UpVectorMode.Custom:
                    return customUpVector.normalized;

                default:
                    return Vector3.up;
            }
        }

        private Quaternion ApplyAxisConstraints(Quaternion rotation)
        {
            if (!freezeXAxis && !freezeYAxis && !freezeZAxis)
                return rotation;

            Vector3 eulerAngles = rotation.eulerAngles;
            Vector3 originalEuler = originalRotation.eulerAngles;

            if (freezeXAxis)
                eulerAngles.x = originalEuler.x;
            if (freezeYAxis)
                eulerAngles.y = originalEuler.y;
            if (freezeZAxis)
                eulerAngles.z = originalEuler.z;

            return Quaternion.Euler(eulerAngles);
        }

        public void SetTargetCamera(Camera newCamera)
        {
            targetCamera = newCamera;
            lastCameraPosition = newCamera != null ? newCamera.transform.position : Vector3.zero;
            lastDistanceToCamera =
                newCamera != null
                    ? Vector3.Distance(transform.position, newCamera.transform.position)
                    : 0f;
        }

        public void SetBillboardMode(BillboardMode newMode)
        {
            billboardMode = newMode;
        }

        public void SetBillboardingEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public void ForceUpdate()
        {
            if (targetCamera != null)
            {
                UpdateBillboard();
            }
        }

        public void ResetToOriginalRotation()
        {
            transform.rotation = originalRotation;
        }

        public float GetDistanceToCamera()
        {
            return targetCamera != null
                ? Vector3.Distance(transform.position, targetCamera.transform.position)
                : 0f;
        }

        public BillboardPerformanceInfo GetPerformanceInfo()
        {
            return new BillboardPerformanceInfo
            {
                UpdateCount = updateCount,
                LastUpdateTime = lastUpdateTime,
                CurrentFrameRate = 1f / Time.deltaTime,
                DistanceToCamera = GetDistanceToCamera(),
                IsActiveAndEnabled = isActiveAndEnabled,
            };
        }

        public bool IsConfigurationValid()
        {
            if (cameraDetection == CameraDetectionMode.Manual && targetCamera == null)
            {
                return false;
            }

            if (updateFrequency == UpdateFrequency.TimedUpdate && customUpdateInterval <= 0f)
            {
                return false;
            }

            if (upVectorMode == UpVectorMode.Custom && customUpVector.magnitude < 0.001f)
            {
                return false;
            }

            return true;
        }

        public string GetConfigurationSummary()
        {
            return $"Billboard Config: Mode={billboardMode}, Camera={cameraDetection}, "
                + $"Update={updateFrequency}, Constraints=X:{freezeXAxis},Y:{freezeYAxis},Z:{freezeZAxis}";
        }
    }

    public enum CameraDetectionMode
    {
        MainCamera, //* Use Camera.main
        ActiveCamera, //* Find any active camera
        TaggedCamera, //* Find camera with MainCamera tag
        ClosestCamera, //* Find closest camera to object
        Manual, //* Manually assigned camera
    }

    public enum BillboardMode
    {
        ScreenAligned, //* Align with camera's forward direction
        LookAtCamera, //* Face towards camera
        LookAtCameraInverted, //* Face away from camera
        MatchCameraRotation, //* Match camera's exact rotation
        YAxisOnly, //* Only rotate around Y axis (vertical billboard)
    }

    public enum UpVectorMode
    {
        CameraUp, //* Use camera's up vector
        WorldUp, //* Use world up (Vector3.up)
        ObjectUp, //* Use object's current up vector
        Custom, //* Use custom specified up vector
    }

    public enum UpdateFrequency
    {
        EveryFrame, //* Update every LateUpdate
        TimedUpdate, //* Update at specified intervals
        DistanceBased, //* Update when distance changes significantly
        OnCameraMove, //* Update only when camera moves
    }
}
