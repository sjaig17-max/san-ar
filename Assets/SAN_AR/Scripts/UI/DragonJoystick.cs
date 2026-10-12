using UnityEngine;
using UnityEngine.EventSystems;

namespace SAN_AR.UI
{
    /// <summary>
    /// Touchscreen on-screen floating/fixed joystick for mobile AR controls.
    /// Handles drag events, normalizes input vector, and updates UI thumb handle.
    /// Works seamlessly with both legacy Input and new Unity Input System UI events.
    /// </summary>
    public class DragonJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Joystick UI Elements")]
        [Tooltip("The background ring/container of the joystick")]
        [SerializeField] private RectTransform background;

        [Tooltip("The movable handle/knob of the joystick")]
        [SerializeField] private RectTransform handle;

        [Header("Joystick Settings")]
        [Tooltip("Maximum distance handle can move from center (in pixels/canvas units)")]
        [SerializeField] private float handleRange = 80f;

        [Tooltip("Deadzone radius where small accidental finger shakes are ignored")]
        [SerializeField] private float deadZone = 0.05f;

        // Current normalized input values (-1.0 to +1.0)
        private Vector2 inputVector = Vector2.zero;
        private Canvas canvas;
        private Camera targetCamera;

        public float Horizontal => (inputVector.magnitude > deadZone) ? inputVector.x : 0f;
        public float Vertical => (inputVector.magnitude > deadZone) ? inputVector.y : 0f;
        public Vector2 Direction => (inputVector.magnitude > deadZone) ? inputVector : Vector2.zero;

        private void Awake()
        {
            if (background == null)
                background = GetComponent<RectTransform>();

            canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                targetCamera = canvas.worldCamera;
            }

            ResetJoystick();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null) return;

            Vector2 position = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                targetCamera,
                out position
            );

            // Pivot compensation if background pivot is not (0.5, 0.5)
            Vector2 pivotOffset = background.pivot * background.sizeDelta - (background.sizeDelta * 0.5f);
            position -= pivotOffset;

            // Clamp handle displacement
            Vector2 clampedPosition = Vector2.ClampMagnitude(position, handleRange);
            handle.anchoredPosition = clampedPosition;

            // Calculate normalized input vector
            inputVector = clampedPosition / handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetJoystick();
        }

        public void ResetJoystick()
        {
            inputVector = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }

        private void OnDisable()
        {
            ResetJoystick();
        }
    }
}
