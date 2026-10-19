using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using SAN_AR.Dragon;
using SAN_AR.UI;

namespace SAN_AR.AR
{
    /// <summary>
    /// Core AR Foundation Image Tracking Controller for SAN AR Dragon.
    /// Manages ARTrackedImageManager events, spawns single dragon instance,
    /// maintains AR stability, reports tracking status to gameplay UI,
    /// and handles reset operations.
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class ARImageTrackingController : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [Tooltip("Prefab to instantiate when the dragon reference image is recognized")]
        [SerializeField] private GameObject dragonPrefab;

        [Tooltip("The reference image name to track (matching the library entry)")]
        [SerializeField] private string targetImageName = "DragonReference";

        [Tooltip("Prefab placement offset above the tracked image plane")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.02f, 0);

        [Header("UI & Input Connections")]
        [SerializeField] private GameplayUIController gameplayUI;
        [SerializeField] private DragonJoystick joystick;
        [SerializeField] private Camera arCamera;

        private ARTrackedImageManager trackedImageManager;
        private GameObject spawnedDragonInstance;
        private DragonMovement dragonMovement;
        private ARTrackedImage currentTrackedImage;
        private bool isTrackingImage;

        public GameObject SpawnedDragon => spawnedDragonInstance;
        public bool IsTracking => isTrackingImage;

        private void Awake()
        {
            trackedImageManager = GetComponent<ARTrackedImageManager>();

            if (arCamera == null)
            {
                arCamera = Camera.main;
            }

            if (gameplayUI == null)
            {
                gameplayUI = FindFirstObjectByType<GameplayUIController>();
            }

            if (joystick == null)
            {
                joystick = FindFirstObjectByType<DragonJoystick>();
            }
        }

        private void OnEnable()
        {
            if (trackedImageManager != null)
            {
                trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
            }
        }

        private void OnDisable()
        {
            if (trackedImageManager != null)
            {
                trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
            }
        }

        private void Start()
        {
            UpdateUIStatus("Point your camera at the dragon image.", false);
        }

        private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            // Process newly added images
            foreach (ARTrackedImage addedImage in eventArgs.added)
            {
                HandleImageTracking(addedImage);
            }

            // Process updated images
            foreach (ARTrackedImage updatedImage in eventArgs.updated)
            {
                HandleImageTracking(updatedImage);
            }

            // Process removed images
            foreach (KeyValuePair<TrackableId, ARTrackedImage> removedPair in eventArgs.removed)
            {
                if (currentTrackedImage != null && removedPair.Key == currentTrackedImage.trackableId)
                {
                    OnTrackingLost();
                }
            }
        }

        private void HandleImageTracking(ARTrackedImage trackedImage)
        {
            string imgName = trackedImage.referenceImage.name;

            // If targetImageName is specified, ensure it matches (or allow any reference image if empty or contains dragon)
            if (!string.IsNullOrEmpty(targetImageName) && 
                !imgName.Equals(targetImageName, System.StringComparison.OrdinalIgnoreCase) && 
                !imgName.ToLower().Contains("dragon"))
            {
                return;
            }

            currentTrackedImage = trackedImage;

            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                isTrackingImage = true;

                // Spawn dragon if not already instantiated
                if (spawnedDragonInstance == null)
                {
                    SpawnDragon(trackedImage);
                }
                else
                {
                    // If dragon is still active, notify UI
                    UpdateUIStatus("Dragon Found! Use the joystick to move.", true);
                }
            }
            else if (trackedImage.trackingState == TrackingState.Limited || trackedImage.trackingState == TrackingState.None)
            {
                OnTrackingLost();
            }
        }

        private void SpawnDragon(ARTrackedImage trackedImage)
        {
            if (dragonPrefab == null)
            {
                Debug.LogWarning("[ARImageTrackingController] Dragon prefab is not assigned in the inspector!");
                return;
            }

            // Spawn attached to the image initially
            Vector3 spawnPos = trackedImage.transform.TransformPoint(spawnOffset);
            Quaternion spawnRot = trackedImage.transform.rotation;

            spawnedDragonInstance = Instantiate(dragonPrefab, spawnPos, spawnRot, trackedImage.transform);
            spawnedDragonInstance.name = "SAN_AR_Dragon_Instance";

            // Wire up movement and joystick
            dragonMovement = spawnedDragonInstance.GetComponent<DragonMovement>();
            if (dragonMovement == null)
            {
                dragonMovement = spawnedDragonInstance.AddComponent<DragonMovement>();
            }

            if (joystick != null)
            {
                dragonMovement.SetJoystick(joystick);
            }

            if (arCamera != null)
            {
                dragonMovement.SetARCamera(arCamera);
            }

            UpdateUIStatus("Dragon Found! Use the joystick to move.", true);
            Debug.Log($"[ARImageTrackingController] Dragon spawned successfully at image: {trackedImage.referenceImage.name}");
        }

        private void OnTrackingLost()
        {
            isTrackingImage = false;
            UpdateUIStatus("Image tracking lost. Keep pointing at the image or explore.", false);
        }

        private void UpdateUIStatus(string message, bool isTracking)
        {
            if (gameplayUI != null)
            {
                gameplayUI.SetTrackingStatus(message, isTracking);
            }
        }

        /// <summary>
        /// Resets the dragon back to the tracked image position or re-enables initial placement.
        /// Called by the Reset Dragon UI button.
        /// </summary>
        public void ResetDragon()
        {
            if (spawnedDragonInstance != null && currentTrackedImage != null)
            {
                if (dragonMovement != null)
                {
                    dragonMovement.ResetToAnchor(currentTrackedImage.transform);
                }
                else
                {
                    spawnedDragonInstance.transform.SetParent(currentTrackedImage.transform);
                    spawnedDragonInstance.transform.localPosition = spawnOffset;
                    spawnedDragonInstance.transform.localRotation = Quaternion.identity;
                }

                UpdateUIStatus("Dragon repositioned to image marker.", true);
            }
            else
            {
                UpdateUIStatus("Scan the dragon image to spawn the dragon.", false);
            }
        }
    }
}
