using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ElectionEmpire.Character;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// UI for character library - view, load, share, delete saved characters
    /// </summary>
    public class CharacterLibraryUI : MonoBehaviour
    {
        [Header("Library Display")]
        public Transform CharacterGrid;
        public GameObject CharacterCardPrefab;
        
        [Header("Import/Export")]
        public GameObject ImportPanel;
        public TMP_InputField ImportCodeInput;
        public Button ImportButton;
        public Button CloseImportButton;
        public TextMeshProUGUI ExportCodeText;
        
        [Header("Navigation")]
        public Button BackButton;
        public Button CreateNewButton;
        
        public System.Action<ElectionEmpire.Character.Character> OnCharacterSelected;
        
        private List<SavedCharacter> _displayedCharacters = new List<SavedCharacter>();
        
        private void Start()
        {
            if (BackButton != null)
                BackButton.onClick.AddListener(OnBack);
            
            if (CreateNewButton != null)
                CreateNewButton.onClick.AddListener(OnCreateNew);
            
            if (ImportButton != null)
                ImportButton.onClick.AddListener(OnImport);
            
            if (CloseImportButton != null)
                CloseImportButton.onClick.AddListener(() => ImportPanel?.SetActive(false));
            
            RefreshLibrary();
        }
        
        public void RefreshLibrary()
        {
            ClearGrid();
            
            var library = CharacterLibrary.Instance;
            var characters = library.GetAllCharacters();
            _displayedCharacters = characters;
            
            foreach (var savedChar in characters)
            {
                CreateCharacterCard(savedChar);
            }
        }
        
        private void CreateCharacterCard(SavedCharacter savedChar)
        {
            GameObject cardObj;
            
            if (CharacterCardPrefab != null)
            {
                cardObj = Instantiate(CharacterCardPrefab, CharacterGrid);
            }
            else
            {
                // Fallback: create simple card
                cardObj = new GameObject($"Card_{savedChar.ID}");
                cardObj.transform.SetParent(CharacterGrid);
                
                Image bg = cardObj.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.2f, 0.2f);
                
                // Add layout
                var layout = cardObj.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 10;
                layout.padding = new RectOffset(10, 10, 10, 10);
            }
            
            // Add character info
            SetupCharacterCard(cardObj, savedChar);
        }
        
        private void SetupCharacterCard(GameObject cardObj, SavedCharacter savedChar)
        {
            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cardObj.transform);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = savedChar.CustomName;
            nameText.fontSize = 18;
            nameText.fontStyle = FontStyles.Bold;
            
            // Background
            if (savedChar.Character.Background != null)
            {
                GameObject bgObj = new GameObject("Background");
                bgObj.transform.SetParent(cardObj.transform);
                TextMeshProUGUI bgText = bgObj.AddComponent<TextMeshProUGUI>();
                bgText.text = savedChar.Character.Background.Name;
                bgText.fontSize = 14;
            }
            
            // Stats
            GameObject statsObj = new GameObject("Stats");
            statsObj.transform.SetParent(cardObj.transform);
            TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
            statsText.text = $"Used: {savedChar.TimesUsed} times";
            if (!string.IsNullOrEmpty(savedChar.BestResult))
                statsText.text += $"\nBest: {savedChar.BestResult}";
            statsText.fontSize = 12;
            
            // Buttons
            GameObject buttonContainer = new GameObject("Buttons");
            buttonContainer.transform.SetParent(cardObj.transform);
            var buttonLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 5;
            
            // Load button
            CreateButton(buttonContainer, "Load", () => OnLoadCharacter(savedChar.ID));
            
            // Edit button
            CreateButton(buttonContainer, "Edit", () => OnEditCharacter(savedChar.ID));
            
            // Share button
            CreateButton(buttonContainer, "Share", () => OnShareCharacter(savedChar.ID));
            
            // Delete button
            CreateButton(buttonContainer, "Delete", () => OnDeleteCharacter(savedChar.ID));
        }
        
        private void CreateButton(GameObject parent, string text, System.Action onClick)
        {
            GameObject btnObj = new GameObject($"Button_{text}");
            btnObj.transform.SetParent(parent.transform);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.3f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick());
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform);
            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 12;
            btnText.alignment = TextAlignmentOptions.Center;
        }
        
        private void OnLoadCharacter(string characterID)
        {
            var character = CharacterLibrary.Instance.LoadCharacter(characterID);
            if (character != null)
            {
                OnCharacterSelected?.Invoke(character);
            }
        }
        
        private void OnEditCharacter(string characterID)
        {
            // Load character first
            var savedCharacter = CharacterLibrary.Instance.GetSavedCharacter(characterID);
            if (savedCharacter == null)
            {
                Debug.LogWarning($"[CharacterLibraryUI] Character {characterID} not found.");
                return;
            }
            
            // Open character builder with this character pre-filled
            var builderUI = FindFirstObjectByType<CharacterBuilderUI>();
            if (builderUI != null)
            {
                builderUI.LoadCharacterForEditing(savedCharacter.Character);
            }
            else
            {
                Debug.LogWarning("[CharacterLibraryUI] CharacterBuilderUI not found. Cannot edit character.");
            }
            Debug.Log($"Edit character: {characterID}");
        }
        
        private void OnShareCharacter(string characterID)
        {
            string shareCode = CharacterLibrary.Instance.ShareCharacter(characterID);
            if (!string.IsNullOrEmpty(shareCode))
            {
                if (ExportCodeText != null)
                {
                    ExportCodeText.text = $"Share Code:\n{shareCode}";
                }
                
                // Copy to clipboard
                GUIUtility.systemCopyBuffer = shareCode;
                Debug.Log($"Share code copied to clipboard: {shareCode}");
            }
        }
        
        private void OnDeleteCharacter(string characterID)
        {
            // Load character first
            var savedCharacter = CharacterLibrary.Instance.GetSavedCharacter(characterID);
            if (savedCharacter == null)
            {
                Debug.LogWarning($"[CharacterLibraryUI] Character {characterID} not found.");
                return;
            }
            
            // Show confirmation dialog
            bool confirmed = ConfirmDeleteDialog.Show(savedCharacter.CustomName);
            
            if (!confirmed) return;
            CharacterLibrary.Instance.DeleteCharacter(characterID);
            RefreshLibrary();
        }
        
        private void OnBack()
        {
            gameObject.SetActive(false);
        }
        
        private void OnCreateNew()
        {
            // Open character creation flow
            var creationFlow = FindFirstObjectByType<CharacterCreationFlow>();
            if (creationFlow != null)
            {
                // Show mode selection to start character creation
                creationFlow.gameObject.SetActive(true);
            }
            gameObject.SetActive(false);
        }
        
        private void OnImport()
        {
            if (ImportCodeInput != null && !string.IsNullOrEmpty(ImportCodeInput.text))
            {
                bool success = CharacterLibrary.Instance.ImportCharacter(ImportCodeInput.text);
                if (success)
                {
                    RefreshLibrary();
                    ImportPanel?.SetActive(false);
                    ImportCodeInput.text = "";
                }
                else
                {
                    Debug.LogError("Failed to import character. Invalid code.");
                }
            }
        }
        
        private void ClearGrid()
        {
            foreach (Transform child in CharacterGrid)
            {
                Destroy(child.gameObject);
            }
        }
    }
}

