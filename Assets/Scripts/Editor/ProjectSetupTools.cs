using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Tilemaps;

public class ProjectSetupTools : EditorWindow
{
    [MenuItem("Tetris/Setup Project")]
    public static void SetupProject()
    {
        if (!EditorUtility.DisplayDialog("Setup Project", "This will overwrite Scenes and Resources. Sure?", "Yes", "No")) return;

        EnsureDirectory("Assets/Scenes");
        EnsureDirectory("Assets/Resources/Blocks");
        EnsureDirectory("Assets/Prefabs/UI");

        CreateScenes();
        CreateStandardBlocks(); 
        CreateBuilderAssets();  

        SetupMainMenu();
        SetupGameScene();
        SetupOptionsScene();
        SetupBuildingScene();

        EditorUtility.DisplayDialog("Project Setup", "Setup complete! \n1. Add scenes to Build Settings.\n2. TextMeshPro Essentials might need importing.", "OK");
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        AssetDatabase.Refresh();
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

    private static void CreateStandardBlocks()
    {
        CreateBlockAsset("I_Block", Color.cyan, new Vector2Int[] { new(0,0), new(-1,0), new(1,0), new(2,0) });
        CreateBlockAsset("J_Block", Color.blue, new Vector2Int[] { new(0,0), new(-1,0), new(1,0), new(-1,1) });
        CreateBlockAsset("L_Block", new Color(1f, 0.5f, 0f), new Vector2Int[] { new(0,0), new(-1,0), new(1,0), new(1,1) });
        CreateBlockAsset("O_Block", Color.yellow, new Vector2Int[] { new(0,0), new(1,0), new(0,1), new(1,1) });
        CreateBlockAsset("S_Block", Color.green, new Vector2Int[] { new(0,0), new(-1,0), new(0,1), new(1,1) });
        CreateBlockAsset("T_Block", Color.magenta, new Vector2Int[] { new(0,0), new(-1,0), new(1,0), new(0,1) });
        CreateBlockAsset("Z_Block", Color.red, new Vector2Int[] { new(0,0), new(1,0), new(0,1), new(-1,1) });
        AssetDatabase.SaveAssets();
    }

    private static void CreateBlockAsset(string name, Color col, Vector2Int[] cells)
    {
        string path = $"Assets/Resources/Blocks/{name}.asset";
        BlockData data = AssetDatabase.LoadAssetAtPath<BlockData>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<BlockData>();
            AssetDatabase.CreateAsset(data, path);
        }
        data.cells = cells;
        data.color = col;
        EditorUtility.SetDirty(data);
    }

    private static void CreateBuilderAssets()
    {
        // 1. Create a Square Sprite with correct PPU
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point; 
        
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                bool isBorder = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                pixels[y * size + x] = isBorder ? new Color(0.8f, 0.8f, 0.8f) : Color.white;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes("Assets/Resources/Square.png", bytes);
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath("Assets/Resources/Square.png") as TextureImporter;
        if (importer != null) {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32; 
            importer.filterMode = FilterMode.Point;
            importer.compressionQuality = 0;
            importer.SaveAndReimport();
        }

        Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Square.png");

        // 2. Create "FilledCell" Prefab
        GameObject cellObj = new GameObject("CellPrefab");
        Image img = cellObj.AddComponent<Image>();
        img.sprite = squareSprite;
        img.color = Color.cyan;
        RectTransform rect = cellObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(40, 40);
        PrefabUtility.SaveAsPrefabAsset(cellObj, "Assets/Prefabs/UI/CellPrefab.prefab");
        DestroyImmediate(cellObj);

        // 3. Create "AddButton" Prefab
        GameObject addObj = new GameObject("AddButtonPrefab");
        Image addImg = addObj.AddComponent<Image>();
        addImg.sprite = squareSprite;
        addImg.color = new Color(1, 1, 1, 0.3f);
        Button btn = addObj.AddComponent<Button>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(addObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "+";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 30;
        tmp.color = Color.black;
        tmp.raycastTarget = false;
        tmp.rectTransform.anchorMin = Vector2.zero;
        tmp.rectTransform.anchorMax = Vector2.one;
        
        rect = addObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(40, 40);
        PrefabUtility.SaveAsPrefabAsset(addObj, "Assets/Prefabs/UI/AddButtonPrefab.prefab");
        DestroyImmediate(addObj);
        
        // 4. Saved Item Prefab
        GameObject listObj = new GameObject("SavedItemPrefab");
        SavedBlockItem savedItemScript = listObj.AddComponent<SavedBlockItem>();
        
        // Add Image and Layout
        listObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
        LayoutElement le = listObj.AddComponent<LayoutElement>();
        le.minHeight = 60; 
        le.preferredHeight = 60;

        RectTransform listRect = listObj.GetComponent<RectTransform>();
        listRect.sizeDelta = new Vector2(0, 60);

        // A. Create Preview Container (Left side)
        GameObject previewObj = new GameObject("PreviewContainer", typeof(RectTransform));
        previewObj.transform.SetParent(listObj.transform, false);
        RectTransform previewRect = previewObj.GetComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0, 0);
        previewRect.anchorMax = new Vector2(0.2f, 1); // Left 20%
        previewRect.offsetMin = new Vector2(5, 5);
        previewRect.offsetMax = new Vector2(-5, -5);
        
        // B. Create Name Text (Middle-ish)
        // Note: CreateText now defaults to center anchors, so we override them for the list item
        GameObject nameObj = CreateText(listObj.transform, "Name", "BlockName", Vector2.zero, 20);
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.2f, 0);
        nameRect.anchorMax = new Vector2(0.6f, 1);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        nameObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
        nameObj.GetComponent<TextMeshProUGUI>().margin = new Vector4(10, 0, 0, 0);

        // C. Create Load Button
        // Note: CreateButton now defaults to center anchors, we override for list item layout
        GameObject lBtn = CreateButton(listObj.transform, "Load", "Load", Vector2.zero);
        ((RectTransform)lBtn.transform).anchorMin = new Vector2(0.6f, 0);
        ((RectTransform)lBtn.transform).anchorMax = new Vector2(0.8f, 1);
        ((RectTransform)lBtn.transform).offsetMin = Vector2.zero; 
        ((RectTransform)lBtn.transform).offsetMax = Vector2.zero;
        
        // D. Create Delete Button
        GameObject dBtn = CreateButton(listObj.transform, "Del", "X", Vector2.zero);
        dBtn.GetComponent<Image>().color = Color.red;
        ((RectTransform)dBtn.transform).anchorMin = new Vector2(0.8f, 0);
        ((RectTransform)dBtn.transform).anchorMax = new Vector2(1, 1);
        ((RectTransform)dBtn.transform).offsetMin = Vector2.zero;
        ((RectTransform)dBtn.transform).offsetMax = Vector2.zero;

        // E. Assign References
        savedItemScript.blockNameText = nameObj.GetComponent<TextMeshProUGUI>();
        savedItemScript.previewContainer = previewRect;
        savedItemScript.loadButton = lBtn.GetComponent<Button>();
        savedItemScript.deleteButton = dBtn.GetComponent<Button>();
        savedItemScript.pixelSprite = squareSprite;

        PrefabUtility.SaveAsPrefabAsset(listObj, "Assets/Prefabs/UI/SavedItemPrefab.prefab");
        DestroyImmediate(listObj);
    }

    private static void SetupMainMenu()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        ClearScene();
        EnsureCamera();
        EnsureEventSystem();
        EnsureGameManager();
        EnsureScoreBoard();

        GameObject canvas = CreateCanvas("Canvas");
        GameObject panel = CreatePanel(canvas.transform, "MenuPanel");
        
        // Static UI: Title
        GameObject titleText = CreateText(panel.transform, "Title", "TETRIS", new Vector2(0, 100), 60);
        titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        titleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        
        // Static UI: Buttons
        GameObject playBtn = CreateButton(panel.transform, "PlayButton", "Play", new Vector2(0, 20));
        playBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
        playBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);

        GameObject optionsBtn = CreateButton(panel.transform, "OptionsButton", "Options", new Vector2(0, -40));
        optionsBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
        optionsBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);

        GameObject buildBtn = CreateButton(panel.transform, "BuildButton", "Build", new Vector2(0, -100));
        buildBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
        buildBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);

        GameObject exitBtn = CreateButton(panel.transform, "ExitButton", "Exit", new Vector2(0, -160));
        exitBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
        exitBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -160);


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

        // 1. Board & Environment
        GameObject boardObj = new GameObject("Board");
        Board board = boardObj.AddComponent<Board>();

        GameObject gridObj = new GameObject("Grid", typeof(Grid));
        gridObj.transform.SetParent(boardObj.transform);
        Grid grid = gridObj.GetComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        // Background Grid (Visuals)
        GameObject bgTilemapObj = new GameObject("BackgroundTilemap", typeof(Tilemap), typeof(TilemapRenderer));
        bgTilemapObj.transform.SetParent(gridObj.transform);
        Tilemap bgTm = bgTilemapObj.GetComponent<Tilemap>();
        TilemapRenderer bgTr = bgTilemapObj.GetComponent<TilemapRenderer>();
        bgTm.tileAnchor = Vector3.zero;
        bgTr.sortingOrder = -1;
        // Darken the background tiles
        bgTilemapObj.GetComponent<TilemapRenderer>().material.color = new Color(0.5f, 0.5f, 0.5f);

        // Active Piece Tilemap
        GameObject tilemapObj = new GameObject("Tilemap", typeof(Tilemap), typeof(TilemapRenderer));
        tilemapObj.transform.SetParent(gridObj.transform);
        Tilemap tm = tilemapObj.GetComponent<Tilemap>();
        TilemapRenderer tr = tilemapObj.GetComponent<TilemapRenderer>();
        tm.tileAnchor = Vector3.zero;
        tr.sortingOrder = 1;

        board.tilemap = tm;
        board.backgroundTilemap = bgTm;

        // 2. Main UI Canvas
        GameObject canvas = CreateCanvas("Canvas");
        
        // --- LAYOUT STRUCTURE ---
        // We use a horizontal layout group to split the screen into 3 columns: 
        // Left (Hold/Controls), Center (Game View), Right (Score/Next)
        
        GameObject layoutRoot = new GameObject("LayoutRoot", typeof(RectTransform));
        layoutRoot.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = layoutRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // LEFT PANEL (Hold & Controls)
        GameObject leftPanel = CreatePanel(layoutRoot.transform, "LeftPanel");
        leftPanel.GetComponent<Image>().color = Color.clear; // Transparent
        RectTransform leftRect = leftPanel.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0, 0);
        leftRect.anchorMax = new Vector2(0.25f, 1); // 25% width
        leftRect.offsetMin = new Vector2(20, 20); // Padding
        leftRect.offsetMax = new Vector2(0, -20);

        // RIGHT PANEL (Score & Next)
        GameObject rightPanel = CreatePanel(layoutRoot.transform, "RightPanel");
        rightPanel.GetComponent<Image>().color = Color.clear;
        RectTransform rightRect = rightPanel.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.75f, 0); // Starts at 75%
        rightRect.anchorMax = new Vector2(1, 1);
        rightRect.offsetMin = new Vector2(0, 20);
        rightRect.offsetMax = new Vector2(-20, -20);

        // --- LEFT SIDE CONTENT ---
        
        // HOLD Section
        GameObject holdLabel = CreateText(leftPanel.transform, "HoldLabel", "HOLD", new Vector2(0, 350), 28);
        holdLabel.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.8f);
        holdLabel.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.8f);
        holdLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        GameObject holdContainerObj = CreateUIContainer(leftPanel.transform, "HoldContainer", new Vector2(0, -60), 100);
        holdContainerObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.8f);
        holdContainerObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.8f);

        // CONTROLS Section
        GameObject controlsHeader = CreateText(leftPanel.transform, "ControlsHeader", "CONTROLS", Vector2.zero, 22);
        RectTransform chRect = controlsHeader.GetComponent<RectTransform>();
        chRect.anchorMin = new Vector2(0.5f, 0.4f);
        chRect.anchorMax = new Vector2(0.5f, 0.4f);
        chRect.anchoredPosition = new Vector2(0, 0);

        string controlsStr = "Move: Arrows\nRotate: Up / W\nHold: Shift / C\nDrop: Space\nPause: Esc";
        GameObject controlsText = CreateText(leftPanel.transform, "ControlsList", controlsStr, Vector2.zero, 18);
        TextMeshProUGUI ctTMP = controlsText.GetComponent<TextMeshProUGUI>();
        ctTMP.alignment = TextAlignmentOptions.TopLeft;
        ctTMP.lineSpacing = 10;
        RectTransform ctRect = controlsText.GetComponent<RectTransform>();
        ctRect.sizeDelta = new Vector2(200, 200);
        ctRect.anchorMin = new Vector2(0.5f, 0.4f);
        ctRect.anchorMax = new Vector2(0.5f, 0.4f);
        ctRect.anchoredPosition = new Vector2(0, -120);

        // --- RIGHT SIDE CONTENT ---

        // SCORE Section
        GameObject scoreLabel = CreateText(rightPanel.transform, "ScoreLabel", "SCORE", Vector2.zero, 28);
        scoreLabel.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.85f);
        scoreLabel.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.85f);
        scoreLabel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        GameObject scoreValue = CreateText(rightPanel.transform, "ScoreValue", "0", Vector2.zero, 40);
        scoreValue.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        scoreValue.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.8f);
        scoreValue.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.8f);
        scoreValue.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);

        // NEXT Section
        GameObject nextLabel = CreateText(rightPanel.transform, "NextLabel", "NEXT", Vector2.zero, 28);
        nextLabel.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.6f);
        nextLabel.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.6f);
        nextLabel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        List<RectTransform> nextContainers = new List<RectTransform>();
        for (int i = 0; i < 3; i++)
        {
            GameObject nextCont = CreateUIContainer(rightPanel.transform, $"Next_{i}", Vector2.zero, 80);
            RectTransform nr = nextCont.GetComponent<RectTransform>();
            nr.anchorMin = new Vector2(0.5f, 0.6f);
            nr.anchorMax = new Vector2(0.5f, 0.6f);
            nr.anchoredPosition = new Vector2(0, -70 - (i * 90));
            nextContainers.Add(nr);
        }

        // PAUSE BUTTON (Top Right Corner of Canvas)
        GameObject pauseBtn = CreateButton(canvas.transform, "PauseButton", "||", new Vector2(-40, -40));
        RectTransform pbRect = pauseBtn.GetComponent<RectTransform>();
        pbRect.anchorMin = Vector2.one;
        pbRect.anchorMax = Vector2.one;
        pbRect.anchoredPosition = new Vector2(-50, -50);
        pbRect.sizeDelta = new Vector2(50, 50);


        // --- OVERLAY PANELS (Pause & Game Over) ---
        
        // Pause Menu
        GameObject pauseMenu = CreatePanel(canvas.transform, "PauseMenu");
        pauseMenu.SetActive(false);
        CreateText(pauseMenu.transform, "Title", "PAUSED", new Vector2(0, 100), 50);
        GameObject resumeBtn = CreateButton(pauseMenu.transform, "Resume", "Resume", new Vector2(0, 20));
        GameObject pOptBtn = CreateButton(pauseMenu.transform, "Options", "Options", new Vector2(0, -40));
        GameObject pQuitBtn = CreateButton(pauseMenu.transform, "Quit", "Main Menu", new Vector2(0, -100));

        // Game Over
        GameObject gameOverMenu = CreatePanel(canvas.transform, "GameOverPanel");
        gameOverMenu.SetActive(false);
        CreateText(gameOverMenu.transform, "Title", "GAME OVER", new Vector2(0, 150), 60);
        GameObject finalScoreText = CreateText(gameOverMenu.transform, "FinalScore", "0", new Vector2(0, 50), 36);
        
        GameObject nameInput = CreateInputField(gameOverMenu.transform, "NameInput", new Vector2(0, -20));
        GameObject submitBtn = CreateButton(gameOverMenu.transform, "Submit", "Submit", new Vector2(150, -20));
        ((RectTransform)submitBtn.transform).sizeDelta = new Vector2(100, 40);
        
        GameObject restartBtn = CreateButton(gameOverMenu.transform, "Restart", "Try Again", new Vector2(-100, -100));
        GameObject goMenuBtn = CreateButton(gameOverMenu.transform, "Menu", "Main Menu", new Vector2(100, -100));

        // --- SETUP HUD COMPONENT ---
        GameObject hudObj = new GameObject("GameHUD");
        GameHUD hud = hudObj.AddComponent<GameHUD>();
        
        // Assign UI
        hud.scoreText = scoreValue.GetComponent<TextMeshProUGUI>();
        hud.holdContainer = holdContainerObj.GetComponent<RectTransform>();
        hud.nextContainers = nextContainers;
        
        hud.pauseButton = pauseBtn.GetComponent<Button>();
        hud.pauseMenuPanel = pauseMenu;
        hud.resumeButton = resumeBtn.GetComponent<Button>();
        hud.optionsButton = pOptBtn.GetComponent<Button>();
        hud.quitButton = pQuitBtn.GetComponent<Button>();

        hud.gameOverPanel = gameOverMenu;
        hud.finalScoreText = finalScoreText.GetComponent<TextMeshProUGUI>();
        hud.nameInputField = nameInput.GetComponent<TMP_InputField>();
        hud.submitScoreButton = submitBtn.GetComponent<Button>();
        hud.restartButton = restartBtn.GetComponent<Button>();
        hud.menuButton = goMenuBtn.GetComponent<Button>();

        // CRITICAL: Load the Cell Prefab created in CreateBuilderAssets
        hud.cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/CellPrefab.prefab");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
    
    // Helper for the specific black semi-transparent boxes used for Next/Hold
    private static GameObject CreateUIContainer(Transform parent, string name, Vector2 pos, float size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.5f); // Dark semi-transparent background
        
        // Outline
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.2f);
        outline.effectDistance = new Vector2(2, -2);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = pos;
        return go;
    }

    private static void SetupOptionsScene()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/OptionsScene.unity");
        ClearScene();
        EnsureCamera();
        EnsureEventSystem();
        EnsureGameManager();
        EnsureLocalizationManager();

        GameObject canvas = CreateCanvas("Canvas");
        GameObject panel = CreatePanel(canvas.transform, "OptionsPanel");

        // Static UI: Title
        GameObject optionsTitle = CreateText(panel.transform, "Title", "OPTIONS", new Vector2(0, 200), 48);
        optionsTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        optionsTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 200);

        // --- UI Elements ---
        // Resolution Label
        GameObject resLabel = CreateText(panel.transform, "ResLabel", "Resolution", new Vector2(-200, 100), 30);
        resLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        resLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, 100);
        
        GameObject resDropdownObj = CreateDropdown(panel.transform, "ResolutionDropdown", new Vector2(100, 100));
        
        // Language Label
        GameObject langLabel = CreateText(panel.transform, "LangLabel", "Language", new Vector2(-200, 20), 30);
        langLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        langLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, 20);

        GameObject langDropdownObj = CreateDropdown(panel.transform, "LanguageDropdown", new Vector2(100, 20));

        // Clear Scores Button
        GameObject clearBtn = CreateButton(panel.transform, "ClearScoresButton", "Clear Scores", new Vector2(0, -60));
        clearBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 40); 
        clearBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);

        // Back Button
        GameObject backBtn = CreateButton(panel.transform, "BackButton", "Back", new Vector2(0, -150));
        backBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
        backBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -150);

        // Warning Panel
        GameObject warningPanel = CreatePanel(canvas.transform, "WarningPanel");
        warningPanel.SetActive(false);
        
        GameObject warningTitle = CreateText(warningPanel.transform, "WarningTitle", "Are you sure?", new Vector2(0, 50), 40);
        warningTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        warningTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);

        GameObject yesBtn = CreateButton(warningPanel.transform, "YesButton", "Yes", new Vector2(-100, -50));
        yesBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 40);
        yesBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, -50);

        GameObject noBtn = CreateButton(warningPanel.transform, "NoButton", "No", new Vector2(100, -50));
        noBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 40);
        noBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, -50);


        // --- OptionsUI Component ---
        GameObject uiObj = new GameObject("OptionsUI");
        OptionsUI ui = uiObj.AddComponent<OptionsUI>();
        
        ui.resolutionDropdown = resDropdownObj.GetComponent<TMP_Dropdown>();
        ui.languageDropdown = langDropdownObj.GetComponent<TMP_Dropdown>();
        ui.clearScoresButton = clearBtn.GetComponent<Button>();
        ui.backButton = backBtn.GetComponent<Button>();
        ui.warningPanel = warningPanel;
        ui.confirmClearButton = yesBtn.GetComponent<Button>();
        ui.cancelClearButton = noBtn.GetComponent<Button>();
        
        ui.resolutionLabel = resLabel.GetComponent<TextMeshProUGUI>();
        ui.languageLabel = langLabel.GetComponent<TextMeshProUGUI>();
        ui.warningText = warningTitle.GetComponent<TextMeshProUGUI>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject CreateDropdown(Transform parent, string name, Vector2 pos)
    {
        // Create root
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = Color.white;
        RectTransform rt = go.GetComponent<RectTransform>();
        
        // FIX: Use Center Anchors
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        
        rt.sizeDelta = new Vector2(300, 40);
        rt.anchoredPosition = pos;

        TMP_Dropdown dropdown = go.AddComponent<TMP_Dropdown>();
        dropdown.targetGraphic = img;
        
        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(go.transform, false);
        TextMeshProUGUI text = label.AddComponent<TextMeshProUGUI>();
        text.text = "Option A";
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.Left;
        text.fontSize = 24;
        ((RectTransform)label.transform).anchorMin = Vector2.zero;
        ((RectTransform)label.transform).anchorMax = Vector2.one;
        ((RectTransform)label.transform).offsetMin = new Vector2(10, 0);
        dropdown.captionText = text;

        // Arrow
        GameObject arrow = new GameObject("Arrow");
        arrow.transform.SetParent(go.transform, false);
        Image arrowImg = arrow.AddComponent<Image>();
        arrowImg.color = Color.black;
        ((RectTransform)arrow.transform).anchorMin = new Vector2(1, 0.5f);
        ((RectTransform)arrow.transform).anchorMax = new Vector2(1, 0.5f);
        ((RectTransform)arrow.transform).sizeDelta = new Vector2(20, 20);
        ((RectTransform)arrow.transform).anchoredPosition = new Vector2(-15, 0);

        // Template (The popup)
        GameObject template = new GameObject("Template");
        template.transform.SetParent(go.transform, false);
        Image tempImg = template.AddComponent<Image>();
        tempImg.color = new Color(0.9f, 0.9f, 0.9f);
        ScrollRect scroll = template.AddComponent<ScrollRect>();
        
        RectTransform templateRect = template.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 2);
        templateRect.sizeDelta = new Vector2(0, 150);
        
        template.SetActive(false);
        dropdown.template = templateRect;
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        viewport.AddComponent<Image>().maskable = true;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 28);
        
        scroll.content = contentRect;
        scroll.viewport = viewportRect;

        // Item
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        Toggle toggle = item.AddComponent<Toggle>();
        item.AddComponent<Image>().color = Color.white; 
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 20);
        
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);
        TextMeshProUGUI itemText = itemLabel.AddComponent<TextMeshProUGUI>();
        itemText.text = "Option";
        itemText.color = Color.black;
        itemText.fontSize = 24;
        itemText.alignment = TextAlignmentOptions.Left;
        RectTransform itemLabelRect = itemLabel.GetComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(10, 0);
        
        dropdown.itemText = itemText;
        
        return go;
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
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f); 

        // Static UI: Title
        GameObject titleObj = CreateText(panel.transform, "Title", "BLOCK BUILDER", new Vector2(0, 350), 40); 
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 350);

        // Controls 
        GameObject saveBtn = CreateButton(panel.transform, "SaveButton", "Save", new Vector2(-150, -200));
        saveBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
        saveBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -200);

        GameObject clearBtn = CreateButton(panel.transform, "ClearButton", "Reset", new Vector2(0, -200));
        clearBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
        clearBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);

        GameObject backBtn = CreateButton(panel.transform, "BackButton", "Back", new Vector2(150, -200));
        backBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
        backBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(150, -200);

        GameObject nameInput = CreateInputField(panel.transform, "NameInput", new Vector2(0, 150));
        nameInput.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
        nameInput.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 150);

        // Grid Container
        GameObject gridContainer = new GameObject("GridContainer");
        gridContainer.transform.SetParent(panel.transform, false);
        RectTransform gridRect = gridContainer.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.anchoredPosition = new Vector2(100, 0); 
        gridRect.sizeDelta = new Vector2(300, 300);

        // Saved List Container Panel (Left Side)
        GameObject savedListPanel = new GameObject("SavedListPanel");
        savedListPanel.transform.SetParent(panel.transform, false);
        Image listBg = savedListPanel.AddComponent<Image>();
        listBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        RectTransform savedRect = savedListPanel.GetComponent<RectTransform>();
        savedRect.anchorMin = new Vector2(0, 0);
        savedRect.anchorMax = new Vector2(0.25f, 1);
        savedRect.offsetMin = new Vector2(20, 20);
        savedRect.offsetMax = new Vector2(-10, -20);

        // Note: Title inside the list panel. Using custom anchors for this specific element.
        GameObject listTitleObj = CreateText(savedListPanel.transform, "ListTitle", "Saved Blocks", new Vector2(0, 0), 24);
        RectTransform titleRect = listTitleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(0, 40);

        // --- SCROLL VIEW STRUCTURE ---
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(savedListPanel.transform, false);
        ScrollRect sr = scrollView.AddComponent<ScrollRect>();
        
        RectTransform svRect = scrollView.GetComponent<RectTransform>();
        svRect.anchorMin = Vector2.zero;
        svRect.anchorMax = Vector2.one;
        svRect.offsetMin = new Vector2(10, 10);
        svRect.offsetMax = new Vector2(-10, -50); 

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        viewport.AddComponent<Image>().color = new Color(1,1,1,0.01f); 
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        RectTransform vpRect = viewport.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.sizeDelta = Vector2.zero;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false; 
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 5;
        
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        sr.content = contentRect;
        sr.viewport = vpRect;
        sr.vertical = true;
        sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Elastic;

        // Assign UI References
        GameObject uiObj = new GameObject("BuildingUI");
        BuildingModuleUI ui = uiObj.AddComponent<BuildingModuleUI>();
        ui.saveButton = saveBtn.GetComponent<Button>();
        ui.clearButton = clearBtn.GetComponent<Button>();
        ui.backButton = backBtn.GetComponent<Button>();
        ui.blockNameInput = nameInput.GetComponent<TMP_InputField>();
        ui.gridContainer = gridContainer.transform;
        ui.savedListContent = content.transform; 
        
        ui.titleText = titleObj.GetComponent<TextMeshProUGUI>();
        ui.savedListTitleText = listTitleObj.GetComponent<TextMeshProUGUI>();
        
        ui.cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/CellPrefab.prefab");
        ui.addButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/AddButtonPrefab.prefab");
        ui.savedItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/SavedItemPrefab.prefab");

        GameObject builderObj = new GameObject("BlockBuilder");
        BlockBuilder builder = builderObj.AddComponent<BlockBuilder>();
        ui.blockBuilder = builder;
        
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // --- Helpers ---
    private static void ClearScene()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects()) DestroyImmediate(root);
    }

    private static void EnsureCamera()
    {
        GameObject cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cam.tag = "MainCamera";
        cam.transform.position = new Vector3(0, 0, -10);
        cam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        cam.GetComponent<Camera>().backgroundColor = new Color(0.15f, 0.15f, 0.15f);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }

    private static void EnsureScoreBoard() 
    { 
        if(Object.FindObjectOfType<ScoreBoard>() == null) 
            new GameObject("ScoreBoard").AddComponent<ScoreBoard>(); 
    }
    
    private static void EnsureGameManager() { if(Object.FindObjectOfType<GameManager>() == null) new GameObject("GameManager").AddComponent<GameManager>(); }
    
    private static void EnsureLocalizationManager() { if(Object.FindObjectOfType<LocalizationManager>() == null) new GameObject("LocalizationManager").AddComponent<LocalizationManager>(); }

    private static GameObject CreateCanvas(string name)
    {
        GameObject go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        go.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        return go;
    }

    private static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        // Using semi-transparent black for panels
        go.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        
        // --- CHANGED SECTION START ---
        Image img = go.AddComponent<Image>();
        
        // Load the 'Square' sprite generated in CreateBuilderAssets
        Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Square.png");
        if (btnSprite != null) 
        {
            img.sprite = btnSprite;
            img.type = Image.Type.Sliced; // Ensures corners don't stretch weirdly
            img.pixelsPerUnitMultiplier = 1;
        }
        else
        {
            // Fallback color if sprite isn't found yet
            img.color = new Color(0.9f, 0.9f, 0.9f); 
        }
        // --- CHANGED SECTION END ---

        Button btn = go.AddComponent<Button>();
        // Optional: Make the button transition color slightly darker when pressed
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = colors;
        
        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        RectTransform r = go.GetComponent<RectTransform>();
        
        // Anchors set to Center (0.5, 0.5)
        r.anchorMin = new Vector2(0.5f, 0.5f);
        r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        
        r.anchoredPosition = pos;
        r.sizeDelta = new Vector2(160, 40); 

        RectTransform tr = txt.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;
        
        return go;
    }

    private static GameObject CreateText(Transform parent, string name, string content, Vector2 pos, float size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        RectTransform r = go.GetComponent<RectTransform>();
        // FIX: Use Center Anchors
        r.anchorMin = new Vector2(0.5f, 0.5f);
        r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        
        r.anchoredPosition = pos;
        r.sizeDelta = new Vector2(200, 50); // Default Size

        return go;
    }

    private static GameObject CreateInputField(Transform parent, string name, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(Image), typeof(TMP_InputField));
        go.transform.SetParent(parent, false);
        TMP_InputField input = go.GetComponent<TMP_InputField>();
        
        GameObject textArea = new GameObject("TextArea", typeof(RectTransform));
        textArea.transform.SetParent(go.transform, false);
        ((RectTransform)textArea.transform).anchorMin = Vector2.zero;
        ((RectTransform)textArea.transform).anchorMax = Vector2.one;
        ((RectTransform)textArea.transform).offsetMin = new Vector2(10, 0);
        ((RectTransform)textArea.transform).offsetMax = new Vector2(0, 0); 
        
        GameObject txt = new GameObject("Text", typeof(TextMeshProUGUI));
        txt.transform.SetParent(textArea.transform, false);
        input.textComponent = txt.GetComponent<TextMeshProUGUI>();
        input.textComponent.color = Color.black;
        input.textComponent.fontSize = 20;
        input.textComponent.alignment = TextAlignmentOptions.Left;
        
        RectTransform r = go.GetComponent<RectTransform>();
        // FIX: Use Center Anchors
        r.anchorMin = new Vector2(0.5f, 0.5f);
        r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        
        r.sizeDelta = new Vector2(200, 40);
        r.anchoredPosition = pos;
        return go;
    }
}