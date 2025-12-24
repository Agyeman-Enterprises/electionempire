using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Main menu controller
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("UI References")]
        public Button NewCampaignButton;
        public Button LoadGameButton;
        public Button CharacterLibraryButton;
        public Button QuitButton;
        
        [Header("Panels")]
        public GameObject CharacterCreationPanel;
        public GameObject LoadGamePanel;
        public GameObject CharacterLibraryPanel;
        
        private void Start()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (NewCampaignButton != null)
                NewCampaignButton.onClick.AddListener(OnNewCampaign);
            
            if (LoadGameButton != null)
                LoadGameButton.onClick.AddListener(OnLoadGame);
            
            if (CharacterLibraryButton != null)
                CharacterLibraryButton.onClick.AddListener(OnCharacterLibrary);
            
            if (QuitButton != null)
                QuitButton.onClick.AddListener(OnQuit);
        }
        
        private void OnNewCampaign()
        {
            if (CharacterCreationPanel != null)
            {
                CharacterCreationPanel.SetActive(true);
                gameObject.SetActive(false);
            }
            else
            {
                // Fallback: Load character creation scene
                SceneManager.LoadScene("CharacterCreation");
            }
        }
        
        private void OnLoadGame()
        {
            if (LoadGamePanel != null)
            {
                LoadGamePanel.SetActive(true);
                gameObject.SetActive(false);
            }
            else
            {
                // Fallback: Load game scene with load prompt
                SceneManager.LoadScene("Game");
            }
        }
        
        private void OnCharacterLibrary()
        {
            if (CharacterLibraryPanel != null)
            {
                CharacterLibraryPanel.SetActive(true);
                gameObject.SetActive(false);
            }
        }
        
        private void OnQuit()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}

