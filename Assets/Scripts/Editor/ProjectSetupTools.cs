using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class ProjectSetupTools : EditorWindow
{
    [MenuItem("Tetris/Setup Project")]
    public static void SetupProject()
    {
        if (!EditorUtility.DisplayDialog("Setup Project", "This will overwrite the scenes in Assets/Scenes. Are you sure?", "Yes", "No"))
        {
            return;
        }

        EnsureDirectory("Assets/Scenes");
        CreateScenes();
        
        SetupMainMenu();
        SetupGameScene();
        SetupOptionsScene();
        SetupBuildingScene();
        
        EditorUtility.DisplayDialog("Project Setup", "Project setup complete! Please add scenes to Build Settings.", "OK");
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static void CreateScenes()
    {
        string[] scenes = { "MainMenu", "GameScene", "OptionsScene", "BuildingScene" };
        foreach (string sceneName in scenes)
        {
            string path = $"Assets/Scenes/{sceneName}.unity";
            if (!File.Exists(path))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, path);
            }
        }
    }

    private static void SetupMainMenu()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        ClearScene();
        
        EnsureCamera();
        EnsureEventSystem();
        EnsureGameManager();
        EnsureLocalizationManager();

        GameObject canvas = CreateCanvas("Canvas");
        GameObject panel = CreatePanel(canvas.transform, "MenuPanel");
        
        CreateText(panel.transform, "Title", "TETRIS", new Vector2(0, 100), 60);
        
        GameObject playBtn = CreateButton(panel.transform, "PlayButton", "Play", new Vector2(0, 20));
        GameObject optionsBtn = CreateButton(panel.transform, "OptionsButton", "Options", new Vector2(0, -40));
        GameObject buildBtn = CreateButton(panel.transform, "BuildButton", "Build", new Vector2(0, -100));
        GameObject exitBtn = CreateButton(panel.transform, "ExitButton", "Exit", new Vector2(0, -160));

        GameObject managerObj = new GameObject("MainMenuManager");
        MainMenuUI ui = managerObj.AddComponent<MainMenuUI>();
        ui.playButton = playBtn.GetComponent<Button>();
        ui.optionsButton = optionsBtn.GetComponent<Button>();
        ui.buildButton = buildBtn.GetComponent<Button>();
        ui.exitButton = exitBtn.GetComponent<Button>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupGameScene()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
        ClearScene();
        
        EnsureCamera();
        EnsureEventSystem();
        EnsureGameManager();
        EnsureScoreBoard();

        GameObject canvas = CreateCanvas("Canvas");
        GameObject hudPanel = CreatePanel(canvas.transform, "HUDPanel");
        
        GameObject scoreText = CreateText(hudPanel.transform, "ScoreText", "Score: 0", new Vector2(-300, 150), 36);
        GameObject nextBlockText = CreateText(hudPanel.transform, "NextBlockText", "Next", new Vector2(300, 150), 36);
        GameObject pauseBtn = CreateButton(hudPanel.transform, "PauseButton", "Pause", new Vector2(350, 100));
        
        GameObject pauseMenu = CreatePanel(canvas.transform, "PauseMenu");
        pauseMenu.SetActive(false);
        CreateText(pauseMenu.transform, "PauseTitle", "PAUSED", new Vector2(0, 50), 48);
        GameObject resumeBtn = CreateButton(pauseMenu.transform, "ResumeButton", "Resume", new Vector2(0, -20));
        GameObject optionsBtn = CreateButton(pauseMenu.transform, "OptionsButton", "Options", new Vector2(0, -80));
        GameObject quitBtn = CreateButton(pauseMenu.transform, "QuitButton", "Quit", new Vector2(0, -140));

        GameObject hudObj = new GameObject("GameHUD");
        GameHUD hud = hudObj.AddComponent<GameHUD>();
        hud.scoreText = scoreText.GetComponent<TextMeshProUGUI>();
        hud.nextBlockText = nextBlockText.GetComponent<TextMeshProUGUI>();
        hud.pauseButton = pauseBtn.GetComponent<Button>();
        hud.pauseMenuPanel = pauseMenu;
        hud.resumeButton = resumeBtn.GetComponent<Button>();
        hud.optionsButton = optionsBtn.GetComponent<Button>();
        hud.quitButton = quitBtn.GetComponent<Button>();

        GameObject boardObj = new GameObject("Board");
        boardObj.AddComponent<Board>();
        boardObj.AddComponent<Grid>();
        
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupOptionsScene()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/OptionsScene.unity");
        ClearScene();
        
        EnsureCamera();
        EnsureEventSystem();
        EnsureGameManager();

        GameObject canvas = CreateCanvas("Canvas");
        GameObject panel = CreatePanel(canvas.transform, "OptionsPanel");
        
        CreateText(panel.transform, "Title", "OPTIONS", new Vector2(0, 150), 48);
        
        GameObject clearBtn = CreateButton(panel.transform, "ClearScoresButton", "Clear High Scores", new Vector2(0, 50));
        GameObject backBtn = CreateButton(panel.transform, "BackButton", "Back", new Vector2(0, -150));
        
        GameObject warningPanel = CreatePanel(canvas.transform, "WarningPanel");
        warningPanel.SetActive(false);
        CreateText(warningPanel.transform, "WarningText", "Are you sure?", new Vector2(0, 50), 36);
        GameObject confirmBtn = CreateButton(warningPanel.transform, "ConfirmButton", "Yes", new Vector2(-50, -20));
        GameObject cancelBtn = CreateButton(warningPanel.transform, "CancelButton", "No", new Vector2(50, -20));

        GameObject uiObj = new GameObject("OptionsUI");
        OptionsUI ui = uiObj.AddComponent<OptionsUI>();
        ui.clearScoresButton = clearBtn.GetComponent<Button>();
        ui.backButton = backBtn.GetComponent<Button>();
        ui.warningPanel = warningPanel;
        ui.confirmClearButton = confirmBtn.GetComponent<Button>();
        ui.cancelClearButton = cancelBtn.GetComponent<Button>();
        
        GameObject resDropdown = CreateDropdown(panel.transform, "ResolutionDropdown", new Vector2(0, 100));
        GameObject langDropdown = CreateDropdown(panel.transform, "LanguageDropdown", new Vector2(0, 0));
        ui.resolutionDropdown = resDropdown.GetComponent<TMP_Dropdown>();
        ui.languageDropdown = langDropdown.GetComponent<TMP_Dropdown>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupBuildingScene()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/BuildingScene.unity");
        ClearScene();
        
        EnsureCamera();
        EnsureEventSystem();
        EnsureGameManager();

        GameObject canvas = CreateCanvas("Canvas");
        GameObject panel = CreatePanel(canvas.transform, "BuildingPanel");
        
        CreateText(panel.transform, "Title", "BUILDER", new Vector2(0, 150), 48);

        GameObject saveBtn = CreateButton(panel.transform, "SaveButton", "Save", new Vector2(-200, -150));
        GameObject clearBtn = CreateButton(panel.transform, "ClearButton", "Clear", new Vector2(0, -150));
        GameObject backBtn = CreateButton(panel.transform, "BackButton", "Back", new Vector2(200, -150));
        
        GameObject nameInput = CreateInputField(panel.transform, "NameInput", new Vector2(0, 100));

        GameObject uiObj = new GameObject("BuildingUI");
        BuildingModuleUI ui = uiObj.AddComponent<BuildingModuleUI>();
        ui.saveButton = saveBtn.GetComponent<Button>();
        ui.clearButton = clearBtn.GetComponent<Button>();
        ui.backButton = backBtn.GetComponent<Button>();
        ui.blockNameInput = nameInput.GetComponent<TMP_InputField>();

        GameObject builderObj = new GameObject("BlockBuilder");
        BlockBuilder builder = builderObj.AddComponent<BlockBuilder>();
        ui.blockBuilder = builder;
        
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void ClearScene()
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            // Keep Main Camera if it exists and is standard? No, let's just recreate to be safe.
            DestroyImmediate(root);
        }
    }

    // Helper Methods
    private static void EnsureCamera()
    {
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        Camera c = cam.AddComponent<Camera>();
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
        c.orthographic = true;
        c.orthographicSize = 10;
        cam.AddComponent<AudioListener>();
    }

    private static void EnsureEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    private static void EnsureGameManager()
    {
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    private static void EnsureScoreBoard()
    {
        new GameObject("ScoreBoard").AddComponent<ScoreBoard>();
    }

    private static void EnsureLocalizationManager()
    {
        new GameObject("LocalizationManager").AddComponent<LocalizationManager>();
    }

    private static GameObject CreateCanvas(string name)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.5f);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return go;
    }

    private static TMP_FontAsset GetDefaultFont()
    {
        // Try explicit path first
        string path = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        
        if (font == null)
        {
            // Try TMP Settings
            if (TMP_Settings.defaultFontAsset != null)
            {
                font = TMP_Settings.defaultFontAsset;
            }
        }

        if (font == null)
        {
            // Fallback search
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            }
        }
        
        return font;
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = Color.white; // Button background white
        Button btn = go.AddComponent<Button>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = Color.black; // Text black
        tmp.font = GetDefaultFont(); // Assign font
        
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 40);
        rect.anchoredPosition = position;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return go;
    }

    private static GameObject CreateText(Transform parent, string name, string content, Vector2 position, float fontSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white; // Title text white
        tmp.font = GetDefaultFont(); // Assign font
        
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 100);
        rect.anchoredPosition = position;
        
        return go;
    }

    private static GameObject CreateInputField(Transform parent, string name, Vector2 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = Color.white;
        TMP_InputField input = go.AddComponent<TMP_InputField>();
        
        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(go.transform, false);
        RectTransform areaRect = textArea.AddComponent<RectTransform>();
        areaRect.anchorMin = Vector2.zero;
        areaRect.anchorMax = Vector2.one;
        areaRect.offsetMin = new Vector2(10, 10);
        areaRect.offsetMax = new Vector2(-10, -10);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 24;
        text.color = Color.black;
        text.font = GetDefaultFont();
        
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholder.fontSize = 24;
        placeholder.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholder.font = GetDefaultFont();
        placeholder.text = "Enter text...";
        
        input.textViewport = areaRect;
        input.textComponent = text;
        input.placeholder = placeholder;
        
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 40);
        rect.anchoredPosition = position;
        
        return go;
    }

    private static GameObject CreateDropdown(Transform parent, string name, Vector2 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        TMP_Dropdown dropdown = go.AddComponent<TMP_Dropdown>();
        
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 40);
        rect.anchoredPosition = position;
        
        return go;
    }
}
