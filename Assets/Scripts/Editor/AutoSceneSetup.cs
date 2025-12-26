using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;
using System.Linq;

namespace ElectionEmpire.Editor
{
    /// <summary>
    /// Fully automated scene setup for Election Empire.
    /// Creates all scenes, configures all components, and wires everything up automatically.
    /// </summary>
    public static class AutoSceneSetup
    {
        [MenuItem("Tools/Election Empire/Auto Setup Complete Project", false, 1)]
        public static void SetupCompleteProject()
        {
            if (!EditorUtility.DisplayDialog("Auto Setup", 
                "This will create and configure:\n" +
                "• MainMenu scene with all UI panels\n" +
                "• Game scene with all managers\n" +
                "• All component configurations\n" +
                "• Build settings\n\n" +
                "Existing scenes will be backed up.\n\n" +
                "Continue?", 
                "Yes, Setup Everything", "Cancel"))
            {
                return;
            }

            EditorUtility.DisplayProgressBar("Auto Setup", "Starting setup...", 0f);
            
            try
            {
                // Step 1: Backup existing scenes
                EditorUtility.DisplayProgressBar("Auto Setup", "Backing up existing scenes...", 0.1f);
                BackupExistingScenes();
                
                // Step 2: Create MainMenu scene
                EditorUtility.DisplayProgressBar("Auto Setup", "Creating MainMenu scene...", 0.3f);
                CreateMainMenuScene();
                
                // Step 3: Create Game scene
                EditorUtility.DisplayProgressBar("Auto Setup", "Creating Game scene...", 0.6f);
                CreateGameScene();
                
                // Step 4: Configure Build Settings
                EditorUtility.DisplayProgressBar("Auto Setup", "Configuring Build Settings...", 0.9f);
                ConfigureBuildSettings();
                
                EditorUtility.DisplayProgressBar("Auto Setup", "Finalizing...", 1.0f);
                
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
                
                EditorUtility.DisplayDialog("Setup Complete!", 
                    "✅ MainMenu scene created and configured\n" +
                    "✅ Game scene created and configured\n" +
                    "✅ All components wired up\n" +
                    "✅ Build settings configured\n\n" +
                    "You can now open the MainMenu scene and press Play!", 
                    "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Setup Error", 
                    "An error occurred during setup:\n\n" + e.Message + "\n\nCheck the console for details.", 
                    "OK");
                Debug.LogError($"[AutoSceneSetup] Error: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void BackupExistingScenes()
        {
            string backupFolder = "Assets/Scenes_Backup";
            if (!AssetDatabase.IsValidFolder(backupFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes_Backup");
            }

            string[] scenesToBackup = { "MainMenu.unity", "Game.unity" };
            foreach (string sceneName in scenesToBackup)
            {
                string sourcePath = $"Assets/Scenes/{sceneName}";
                if (File.Exists(sourcePath))
                {
                    string backupPath = $"{backupFolder}/{sceneName}.backup_{System.DateTime.Now:yyyyMMdd_HHmmss}";
                    AssetDatabase.MoveAsset(sourcePath, backupPath);
                }
            }
        }

        private static void CreateMainMenuScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create panels
            GameObject mainPanel = CreatePanel(canvasObj.transform, "MainMenuPanel", true);
            AddVerticalLayoutGroup(mainPanel);
            
            CreateTMPText(mainPanel.transform, "TitleText", "ELECTION EMPIRE", 72, FontStyles.Bold);
            CreateTMPText(mainPanel.transform, "TaglineText", "Politics is a dirty game. Time to get your hands filthy.", 24, FontStyles.Italic);
            
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(mainPanel.transform);
            spacer.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 50);
            spacer.AddComponent<LayoutElement>().minHeight = 50;

            // Create buttons
            Button newCampaignBtn = CreateTMPButton(mainPanel.transform, "NewCampaignButton", "New Campaign").GetComponent<Button>();
            Button continueBtn = CreateTMPButton(mainPanel.transform, "ContinueButton", "Continue").GetComponent<Button>();
            Button loadGameBtn = CreateTMPButton(mainPanel.transform, "LoadGameButton", "Load Game").GetComponent<Button>();
            Button settingsBtn = CreateTMPButton(mainPanel.transform, "SettingsButton", "Settings").GetComponent<Button>();
            Button quitBtn = CreateTMPButton(mainPanel.transform, "QuitButton", "Quit").GetComponent<Button>();

            // Create other panels
            GameObject characterCreationPanel = CreatePanel(canvasObj.transform, "CharacterCreationPanel", false);
            GameObject loadGamePanel = CreatePanel(canvasObj.transform, "LoadGamePanel", false);
            GameObject settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel", false);
            GameObject creditsPanel = CreatePanel(canvasObj.transform, "CreditsPanel", false);

            // Add PanelManager and configure it
            var panelManager = canvasObj.AddComponent<ElectionEmpire.UI.PanelManager>();
            ConfigurePanelManager(panelManager, new[] 
            { 
                ("MainMenu", mainPanel), 
                ("CharacterCreation", characterCreationPanel), 
                ("LoadGame", loadGamePanel), 
                ("Settings", settingsPanel),
                ("Credits", creditsPanel)
            });

            // Add SimpleMainMenu and configure it
            var simpleMainMenu = canvasObj.AddComponent<ElectionEmpire.UI.SimpleMainMenu>();
            ConfigureSimpleMainMenu(simpleMainMenu, panelManager, newCampaignBtn, continueBtn, loadGameBtn, settingsBtn, quitBtn);

            // Create SceneBootstrapper
            GameObject bootstrapper = new GameObject("[SceneBootstrapper]");
            bootstrapper.AddComponent<ElectionEmpire.Core.SceneBootstrapper>();

            // Save scene
            EnsureScenesFolder();
            string path = "Assets/Scenes/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[AutoSceneSetup] MainMenu scene created at: {path}");
        }

        private static void CreateGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            // Create GameController with GameSceneInitializer
            GameObject gameController = new GameObject("[GameController]");
            gameController.AddComponent<ElectionEmpire.Gameplay.GameSceneInitializer>();

            // Create Managers container
            GameObject managers = new GameObject("[Managers]");
            
            string[] managerNames = new string[]
            {
                "TimeManager",
                "AIManager", 
                "ScandalManager",
                "CrisisManager",
                "ElectionManager",
                "ResourceManager",
                "NewsEventManager",
                "MediaManager",
                "CharacterManager",
                "StaffManager",
                "AudioManager"
            };

            foreach (string name in managerNames)
            {
                GameObject mgr = new GameObject(name);
                mgr.transform.SetParent(managers.transform);
            }

            // Create Game Canvas
            GameObject canvasObj = new GameObject("GameCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create game panels
            GameObject hudPanel = CreatePanel(canvasObj.transform, "GameHUDPanel", true);
            GameObject pausePanel = CreatePanel(canvasObj.transform, "PauseMenuPanel", false);
            GameObject eventPanel = CreatePanel(canvasObj.transform, "EventPopupPanel", false);
            GameObject scandalPanel = CreatePanel(canvasObj.transform, "ScandalResponsePanel", false);
            GameObject electionPanel = CreatePanel(canvasObj.transform, "ElectionNightPanel", false);
            GameObject staffPanel = CreatePanel(canvasObj.transform, "StaffManagementPanel", false);
            GameObject mapPanel = CreatePanel(canvasObj.transform, "CampaignMapPanel", false);

            // Add PanelManager and configure it
            var panelManager = canvasObj.AddComponent<ElectionEmpire.UI.PanelManager>();
            ConfigurePanelManager(panelManager, new[] 
            { 
                ("GameHUD", hudPanel), 
                ("PauseMenu", pausePanel), 
                ("EventPopup", eventPanel), 
                ("ScandalResponse", scandalPanel),
                ("ElectionNight", electionPanel),
                ("StaffManagement", staffPanel),
                ("CampaignMap", mapPanel)
            }, "GameHUD");

            // Save scene
            EnsureScenesFolder();
            string path = "Assets/Scenes/Game.unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[AutoSceneSetup] Game scene created at: {path}");
        }

        private static void ConfigurePanelManager(ElectionEmpire.UI.PanelManager panelManager, 
            (string id, GameObject panel)[] panels, string defaultPanelId = "MainMenu")
        {
            SerializedObject so = new SerializedObject(panelManager);
            SerializedProperty panelsProp = so.FindProperty("panels");
            SerializedProperty defaultPanelProp = so.FindProperty("defaultPanelId");

            if (panelsProp != null && defaultPanelProp != null)
            {
                panelsProp.ClearArray();
                
                for (int i = 0; i < panels.Length; i++)
                {
                    panelsProp.InsertArrayElementAtIndex(i);
                    SerializedProperty element = panelsProp.GetArrayElementAtIndex(i);
                    
                    element.FindPropertyRelative("panelId").stringValue = panels[i].id;
                    element.FindPropertyRelative("panelObject").objectReferenceValue = panels[i].panel;
                    element.FindPropertyRelative("canvasGroup").objectReferenceValue = panels[i].panel.GetComponent<CanvasGroup>();
                    element.FindPropertyRelative("transitionIn").enumValueIndex = (int)ElectionEmpire.UI.PanelManager.TransitionType.Fade;
                    element.FindPropertyRelative("transitionOut").enumValueIndex = (int)ElectionEmpire.UI.PanelManager.TransitionType.Fade;
                }
                
                defaultPanelProp.stringValue = defaultPanelId;
                so.ApplyModifiedProperties();
            }
        }

        private static void ConfigureSimpleMainMenu(ElectionEmpire.UI.SimpleMainMenu simpleMainMenu,
            ElectionEmpire.UI.PanelManager panelManager, Button newCampaign, Button continueBtn, 
            Button loadGame, Button settings, Button quit)
        {
            SerializedObject so = new SerializedObject(simpleMainMenu);
            
            so.FindProperty("panelManager").objectReferenceValue = panelManager;
            so.FindProperty("newCampaignButton").objectReferenceValue = newCampaign;
            so.FindProperty("continueButton").objectReferenceValue = continueBtn;
            so.FindProperty("loadGameButton").objectReferenceValue = loadGame;
            so.FindProperty("settingsButton").objectReferenceValue = settings;
            so.FindProperty("quitButton").objectReferenceValue = quit;
            
            so.ApplyModifiedProperties();
        }

        private static GameObject CreatePanel(Transform parent, string name, bool active)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            panel.AddComponent<CanvasGroup>();
            
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            
            panel.SetActive(active);
            
            return panel;
        }

        private static void AddVerticalLayoutGroup(GameObject panel)
        {
            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 15;
            vlg.padding = new RectOffset(400, 400, 150, 150);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        private static void CreateTMPText(Transform parent, string name, string text, int fontSize, FontStyles style)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, fontSize + 20);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            LayoutElement le = textObj.AddComponent<LayoutElement>();
            le.minHeight = fontSize + 20;
            le.preferredHeight = fontSize + 20;
        }

        private static GameObject CreateTMPButton(Transform parent, string name, string buttonText)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 60);
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.4f, 0.6f, 1f);
            
            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.5f, 0.7f, 1f);
            colors.pressedColor = new Color(0.15f, 0.3f, 0.45f, 1f);
            button.colors = colors;
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = buttonText;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            LayoutElement le = buttonObj.AddComponent<LayoutElement>();
            le.minHeight = 60;
            le.preferredHeight = 60;
            le.minWidth = 300;
            
            return buttonObj;
        }

        private static void ConfigureBuildSettings()
        {
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            
            // Remove existing Election Empire scenes
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.path.Contains("Assets/Scenes/MainMenu.unity") && 
                    !scene.path.Contains("Assets/Scenes/Game.unity"))
                {
                    buildScenes.Add(scene);
                }
            }
            
            // Add MainMenu first (index 0)
            buildScenes.Insert(0, new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true));
            
            // Add Game second (index 1)
            buildScenes.Insert(1, new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true));
            
            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log("[AutoSceneSetup] Build settings configured: MainMenu (0), Game (1)");
        }

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }
    }
}

