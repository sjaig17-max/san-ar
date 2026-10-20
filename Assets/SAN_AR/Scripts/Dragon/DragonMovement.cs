using UnityEngine;
using SAN_AR.UI;

namespace SAN_AR.Dragon
{
    /// <summary>
    /// Controls dragon movement and rotation in AR world-space.
    /// Moves relative to camera orientation or tracked image reference plane,
    /// rotates smoothly in movement direction, and drives animations.
    /// </summary>
    public class DragonMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Dragon forward movement speed in units/sec")]
        [SerializeField] private float moveSpeed = 1.2f;

        [Tooltip("Rotation smoothing speed")]
        [SerializeField] private float rotationSpeed = 10f;

        [Tooltip("Dragon overall scale multiplier in AR")]
        [SerializeField] private float dragonScale = 0.25f;

        [Header("Input Source")]
        [SerializeField] private DragonJoystick joystick;

        [Header("Optional References")]
        [Tooltip("AR Camera used for camera-relative forward orientation")]
        [SerializeField] private Camera arCamera;

        [Tooltip("Dragon animation controller component if present")]
        [SerializeField] private DragonAnimationController animationController;

        private Vector3 initialLocalPosition;
        private Quaternion initialLocalRotation;
        private Transform initialParent;
        private bool isAnchored = true;

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0.1f, value);
        }

        public float DragonScale
        {
            get => dragonScale;
            set
            {
                dragonScale = Mathf.Max(0.01f, value);
                transform.localScale = Vector3.one * dragonScale;
            }
        }

        private void Awake()
        {
            if (arCamera == null)
            {
                arCamera = Camera.main;
            }

            if (animationController == null)
            {
                animationController = GetComponent<DragonAnimationController>();
            }

            transform.localScale = Vector3.one * dragonScale;
        }

        private void Start()
        {
            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;
            initialParent = transform.parent;

            if (joystick == null)
            {
                joystick = FindFirstObjectByType<DragonJoystick>();
            }
        }

        public void SetJoystick(DragonJoystick newJoystick)
        {
            joystick = newJoystick;
        }

        public void SetARCamera(Camera cam)
        {
            arCamera = cam;
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector2 input = Vector2.zero;

            if (joystick != null)
            {
                input = joystick.Direction;
            }

            // Keyboard fallback for Unity Editor testing using new Input System
            if (input == Vector2.zero)
            {
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current != null)
                {
                    float h = 0f;
                    float v = 0f;
                    var kb = UnityEngine.InputSystem.Keyboard.current;
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
                    if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
                    if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
                    input = new Vector2(h, v).normalized;
                }
#endif
            }

            float inputMagnitude = Mathf.Clamp01(input.magnitude);

            if (inputMagnitude > 0.05f)
            {
                // Unparent from image transform upon user movement so the dragon
                // can move freely throughout the AR environment without being tethered
                if (isAnchored && transform.parent != null)
                {
                    DetachFromAnchor();
                }

                // Compute world movement direction relative to AR camera yaw
                Vector3 camForward = Vector3.forward;
                Vector3 camRight = Vector3.right;

                if (arCamera != null)
                {
                    camForward = arCamera.transform.forward;
                    camRight = arCamera.transform.right;
                    camForward.y = 0f;
                    camRight.y = 0f;
                    camForward.Normalize();
                    camRight.Normalize();
                }

                Vector3 moveDirection = (camRight * input.x + camForward * input.y).normalized;

                // Move dragon smoothly
                transform.position += moveDirection * (moveSpeed * inputMagnitude * Time.deltaTime);

                // Smoothly face movement heading
                if (moveDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }

                // Update animation state
                if (animationController != null)
                {
                    animationController.SetMoving(true, inputMagnitude);
                }
            }
            else
            {
                if (animationController != null)
                {
                    animationController.SetMoving(false, 0f);
                }
            }
        }

        /// <summary>
        /// Detaches from tracked image anchor so user can explore with joystick freely.
        /// Preserves world position and rotation seamlessly.
        /// </summary>
        public void DetachFromAnchor()
        {
            isAnchored = false;
            transform.SetParent(null, true);
        }

        /// <summary>
        /// Re-attaches to anchor or teleports back to spawn transform.
        /// </summary>
        public void ResetToAnchor(Transform anchorTransform)
        {
            if (anchorTransform != null)
            {
                transform.SetParent(anchorTransform);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                isAnchored = true;
            }
            else if (initialParent != null)
            {
                transform.SetParent(initialParent);
                transform.localPosition = initialLocalPosition;
                transform.localRotation = initialLocalRotation;
                isAnchored = true;
            }
            else
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }

            transform.localScale = Vector3.one * dragonScale;

            if (animationController != null)
            {
                animationController.TriggerRoarOrReset();
            }
        }
    }
}
