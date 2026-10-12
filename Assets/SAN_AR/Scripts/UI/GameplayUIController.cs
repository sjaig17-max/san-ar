using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using SAN_AR.AR;

namespace SAN_AR.UI
{
    /// <summary>
    /// Manages mobile in-game HUD:
    /// - Tracking indicator and prompt banners
    /// - Reset Dragon button
    /// - Back to Menu button
    /// - Speed / Scale adjustments (optional)
    /// </summary>
    public class GameplayUIController : MonoBehaviour
    {
        [Header("Status UI")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image trackingIndicatorDot;
        [SerializeField] private Color trackingColor = new Color(0.2f, 0.9f, 0.3f, 1f); // Green
        [SerializeField] private Color lostColor = new Color(0.9f, 0.3f, 0.2f, 1f);     // Red

        [Header("Buttons")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button menuButton;

        [Header("Controllers")]
        [SerializeField] private ARImageTrackingController trackingController;

        [Header("Scenes")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Awake()
        {
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetButtonClicked);
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenuButtonClicked);
            }

            if (trackingController == null)
            {
                trackingController = FindFirstObjectByType<ARImageTrackingController>();
            }
        }

        private void OnDestroy()
        {
            if (resetButton != null) resetButton.onClick.RemoveListener(OnResetButtonClicked);
            if (menuButton != null) menuButton.onClick.RemoveListener(OnMenuButtonClicked);
        }

        public void SetTrackingStatus(string message, bool isTracking)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }

            if (trackingIndicatorDot != null)
            {
                trackingIndicatorDot.color = isTracking ? trackingColor : lostColor;
            }
        }

        public void OnResetButtonClicked()
        {
            if (trackingController != null)
            {
                trackingController.ResetDragon();
            }
        }

        public void OnMenuButtonClicked()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
