using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

namespace ElectionEmpire.Editor
{
    public class SceneSetupWizard : EditorWindow
    {
        [MenuItem("Tools/Election Empire/Scene Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupWizard>("EE Scene Setup");
        }

    private void OnGUI()
    {
        GUILayout.Label("Election Empire Scene Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This wizard creates the required scenes for Election Empire.\n\n" +
            "1. Create MainMenu scene with UI panels\n" +
            "2. Create Game scene with manager hierarchy\n" +
            "3. Add scenes to Build Settings", 
            MessageType.Info);

        GUILayout.Space(20);

        if (GUILayout.Button("Create MainMenu Scene", GUILayout.Height(40)))
        {
            CreateMainMenuScene();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Create Game Scene", GUILayout.Height(40)))
        {
            CreateGameScene();
        }

        GUILayout.Space(20);
        GUILayout.Label("Quick Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Open Build Settings"))
        {
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
        }

        if (GUILayout.Button("Add Open Scene to Build"))
        {
            AddCurrentSceneToBuild();
        }

        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "After creating scenes:\n" +
            "1. Add both scenes to Build Settings (MainMenu index 0)\n" +
            "2. Wire up button OnClick events\n" +
            "3. Attach manager scripts to manager objects",
            MessageType.Warning);
    }

    private void CreateMainMenuScene()
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

        // Create MainMenuPanel
        GameObject mainPanel = CreatePanel(canvasObj.transform, "MainMenuPanel", true);
        AddVerticalLayoutGroup(mainPanel);
        
        // Add title
        CreateTMPText(mainPanel.transform, "TitleText", "ELECTION EMPIRE", 72, FontStyles.Bold);
        CreateTMPText(mainPanel.transform, "TaglineText", "Politics is a dirty game. Time to get your hands filthy.", 24, FontStyles.Italic);
        
        // Add spacer
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(mainPanel.transform);
        spacer.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 50);
        spacer.AddComponent<LayoutElement>().minHeight = 50;

        // Add buttons
        CreateTMPButton(mainPanel.transform, "NewCampaignButton", "New Campaign");
        CreateTMPButton(mainPanel.transform, "ContinueButton", "Continue");
        CreateTMPButton(mainPanel.transform, "LoadGameButton", "Load Game");
        CreateTMPButton(mainPanel.transform, "SettingsButton", "Settings");
        CreateTMPButton(mainPanel.transform, "QuitButton", "Quit");

        // Create other panels (inactive)
        CreatePanel(canvasObj.transform, "SettingsPanel", false);
        CreatePanel(canvasObj.transform, "CharacterCreationPanel", false);
        CreatePanel(canvasObj.transform, "LoadGamePanel", false);
        CreatePanel(canvasObj.transform, "CreditsPanel", false);

        // Add required scripts to Canvas
        canvasObj.AddComponent<ElectionEmpire.UI.PanelManager>();
        canvasObj.AddComponent<ElectionEmpire.UI.SimpleMainMenu>();

        // Create SceneBootstrapper
        GameObject bootstrapper = new GameObject("[SceneBootstrapper]");
        bootstrapper.AddComponent<ElectionEmpire.Core.SceneBootstrapper>();

        // Save scene
        string scenesFolder = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(scenesFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
        
        string path = scenesFolder + "/MainMenu.unity";
        
        // Warn if scene already exists
        if (File.Exists(path))
        {
            if (!EditorUtility.DisplayDialog("Scene Exists", 
                $"MainMenu.unity already exists at:\n{path}\n\nOverwrite it?", 
                "Yes, Overwrite", "Cancel"))
            {
                return;
            }
        }
        
        EditorSceneManager.SaveScene(scene, path);
        
        Debug.Log("MainMenu scene created at: " + path);
        EditorUtility.DisplayDialog("Success", "MainMenu scene created at:\n" + path + "\n\nRemember to add it to Build Settings!", "OK");
    }

    private void CreateGameScene()
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
        CreatePanel(canvasObj.transform, "GameHUDPanel", true);
        CreatePanel(canvasObj.transform, "PauseMenuPanel", false);
        CreatePanel(canvasObj.transform, "EventPopupPanel", false);
        CreatePanel(canvasObj.transform, "ScandalResponsePanel", false);
        CreatePanel(canvasObj.transform, "ElectionNightPanel", false);
        CreatePanel(canvasObj.transform, "StaffManagementPanel", false);
        CreatePanel(canvasObj.transform, "CampaignMapPanel", false);

        // Add PanelManager to GameCanvas
        canvasObj.AddComponent<ElectionEmpire.UI.PanelManager>();

        // Save scene
        string scenesFolder = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(scenesFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
        
        string path = scenesFolder + "/Game.unity";
        
        // Warn if scene already exists
        if (File.Exists(path))
        {
            if (!EditorUtility.DisplayDialog("Scene Exists", 
                $"Game.unity already exists at:\n{path}\n\nOverwrite it?", 
                "Yes, Overwrite", "Cancel"))
            {
                return;
            }
        }
        
        EditorSceneManager.SaveScene(scene, path);
        
        Debug.Log("Game scene created at: " + path);
        EditorUtility.DisplayDialog("Success", "Game scene created at:\n" + path + "\n\nRemember to add it to Build Settings!", "OK");
    }

    private GameObject CreatePanel(Transform parent, string name, bool active)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        panel.AddComponent<CanvasGroup>();
        
        // Add background image
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        
        panel.SetActive(active);
        
        return panel;
    }

    private void AddVerticalLayoutGroup(GameObject panel)
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

    private void CreateTMPText(Transform parent, string name, string text, int fontSize, FontStyles style)
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

    private void CreateTMPButton(Transform parent, string name, string buttonText)
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
        
        // Add text child
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
        
        // Layout element for vertical layout group
        LayoutElement le = buttonObj.AddComponent<LayoutElement>();
        le.minHeight = 60;
        le.preferredHeight = 60;
        le.minWidth = 300;
    }

    private void AddCurrentSceneToBuild()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path))
        {
            EditorUtility.DisplayDialog("Error", "Save the scene first!", "OK");
            return;
        }

        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        bool alreadyAdded = false;
        foreach (var s in buildScenes)
        {
            if (s.path == scene.path)
            {
                alreadyAdded = true;
                break;
            }
        }

        if (!alreadyAdded)
        {
            buildScenes.Add(new EditorBuildSettingsScene(scene.path, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log("Added to build: " + scene.path);
            EditorUtility.DisplayDialog("Success", "Scene added to Build Settings!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Info", "Scene already in Build Settings.", "OK");
        }
    }
    }
}
