using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SAN_AR.UI
{
    /// <summary>
    /// Controls the mobile start screen for SAN AR Dragon:
    /// - Start AR button (loads ARGameplay scene)
    /// - Instructions popup modal
    /// - Exit application button
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button startARButton;
        [SerializeField] private Button instructionsButton;
        [SerializeField] private Button closeInstructionsButton;
        [SerializeField] private Button exitButton;

        [Header("Panels")]
        [SerializeField] private GameObject instructionsPanel;

        [Header("Scenes")]
        [SerializeField] private string arSceneName = "ARGameplay";

        private void Awake()
        {
            if (startARButton != null)
                startARButton.onClick.AddListener(OnStartARClicked);

            if (instructionsButton != null)
                instructionsButton.onClick.AddListener(OnInstructionsClicked);

            if (closeInstructionsButton != null)
                closeInstructionsButton.onClick.AddListener(OnCloseInstructionsClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);

            if (instructionsPanel != null)
                instructionsPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (startARButton != null) startARButton.onClick.RemoveListener(OnStartARClicked);
            if (instructionsButton != null) instructionsButton.onClick.RemoveListener(OnInstructionsClicked);
            if (closeInstructionsButton != null) closeInstructionsButton.onClick.RemoveListener(OnCloseInstructionsClicked);
            if (exitButton != null) exitButton.onClick.RemoveListener(OnExitClicked);
        }

        public void OnStartARClicked()
        {
            SceneManager.LoadScene(arSceneName);
        }

        public void OnInstructionsClicked()
        {
            if (instructionsPanel != null)
            {
                instructionsPanel.SetActive(true);
            }
        }

        public void OnCloseInstructionsClicked()
        {
            if (instructionsPanel != null)
            {
                instructionsPanel.SetActive(false);
            }
        }

        public void OnExitClicked()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
