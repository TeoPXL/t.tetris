using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System; // Added System for Action

public class BuildingModuleUI : MonoBehaviour
{
    public BlockBuilder blockBuilder;
    public Button saveButton;
    public Button clearButton;
    public Button backButton;
    public TMP_InputField blockNameInput;
    public Transform gridContainer;
    public Transform savedListContent;

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject addButtonPrefab;
    public GameObject savedItemPrefab; // Must have SavedBlockItem component

    private float cellSize = 40f;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI savedListTitleText;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSave);
        clearButton.onClick.AddListener(OnClear);
        backButton.onClick.AddListener(() => GameManager.Instance.LoadScene("MainMenu"));

        // FIX: Force input alignment
        if (blockNameInput.textComponent != null)
        {
            // Center the actual input text within the field
            blockNameInput.textComponent.alignment = TextAlignmentOptions.Center; 
        }

        OnClear(); // Initial Draw
        RefreshSavedList();
        
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateUITexts;
            UpdateUITexts();
        }
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateUITexts;
        }
    }

    private void UpdateUITexts()
    {
        if (LocalizationManager.Instance == null) return;
        
        if (titleText != null) titleText.text = LocalizationManager.Instance.GetText("block_builder");
        if (savedListTitleText != null) savedListTitleText.text = LocalizationManager.Instance.GetText("saved_blocks");
        
        SetButtonText(clearButton, "reset");
        SetButtonText(backButton, "back");
        // Save button text is handled dynamically in OnSave, but we set default here
        SetButtonText(saveButton, "save");
    }

    private void SetButtonText(Button btn, string key)
    {
        if (btn != null)
        {
            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = LocalizationManager.Instance.GetText(key);
        }
    }

    private void OnSave()
    {
        string name = blockNameInput.text;
        if (string.IsNullOrEmpty(name)) return;
        
        blockBuilder.SaveBlock(name);
        blockNameInput.text = "";
        RefreshSavedList();

        // Visual Feedback
        StartCoroutine(SaveButtonFeedback());
    }

    private IEnumerator SaveButtonFeedback()
    {
        TextMeshProUGUI btnText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
        // Ensure text component is found
        if (btnText == null) yield break; 
        
        // Use localization for feedback if possible
        string originalKey = (LocalizationManager.Instance != null) ? "save" : "Save";
        string savedText = (LocalizationManager.Instance != null) ? LocalizationManager.Instance.GetText("saved_feedback") : "Saved!";
        
        string originalText = btnText.text;
        
        btnText.text = savedText;
        saveButton.interactable = false;

        yield return new WaitForSeconds(1.0f);

        // Restore original text, possibly localized
        if (LocalizationManager.Instance != null)
        {
             btnText.text = LocalizationManager.Instance.GetText(originalKey);
        }
        else
        {
            btnText.text = originalText;
        }

        saveButton.interactable = true;
    }

    private void OnClear()
    {
        blockBuilder.ResetBuilder();
        blockNameInput.text = ""; // Also clear name input on reset
        RefreshGrid();
    }

    public void RefreshGrid()
    {
        // Clear container
        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        List<Vector2Int> cells = blockBuilder.GetActiveCells();
        List<Vector2Int> expansions = blockBuilder.GetAvailableExpansions();

        // Draw Active Cells
        foreach (Vector2Int pos in cells)
        {
            GameObject obj = Instantiate(cellPrefab, gridContainer);
            PositionElement(obj, pos);
            
            Button btn = obj.AddComponent<Button>();
            Vector2Int p = pos;
            btn.onClick.AddListener(() => {
                if (p != Vector2Int.zero) {
                    blockBuilder.RemoveCell(p);
                    RefreshGrid();
                }
            });
        }

        // Draw Expansion Buttons
        foreach (Vector2Int pos in expansions)
        {
            GameObject obj = Instantiate(addButtonPrefab, gridContainer);
            PositionElement(obj, pos);
            
            Button btn = obj.GetComponent<Button>();
            Vector2Int p = pos;
            btn.onClick.AddListener(() => {
                blockBuilder.AddCell(p);
                RefreshGrid();
            });
        }
    }

    private void PositionElement(GameObject obj, Vector2Int gridPos)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(gridPos.x * cellSize, gridPos.y * cellSize);
    }

    public void RefreshSavedList()
    {
        foreach (Transform child in savedListContent) Destroy(child.gameObject);

        // Pre-fetch the sprite from the cellPrefab for previews
        Sprite cellSprite = null;
        if (cellPrefab != null)
        {
            Image sourceImg = cellPrefab.GetComponent<Image>();
            if (sourceImg != null) cellSprite = sourceImg.sprite;
        }

        List<string> savedNames = blockBuilder.GetRegistry();
        foreach (string name in savedNames)
        {
            BlockData data = blockBuilder.GetBlockData(name);
            if (data == null) continue;

            GameObject itemObj = Instantiate(savedItemPrefab, savedListContent);
            SavedBlockItem itemComp = itemObj.GetComponent<SavedBlockItem>();
            
            if (itemComp != null)
            {
                // Ensure the sprite is assigned for the preview drawing logic
                if (itemComp.pixelSprite == null) itemComp.pixelSprite = cellSprite;

                itemComp.Setup(name, data, LoadBlock, DeleteBlock);
            }
        }
    }

    private void LoadBlock(string name)
    {
        blockBuilder.LoadBlockToBuilder(name);
        blockNameInput.text = name;
        RefreshGrid();
    }

    private void DeleteBlock(string name)
    {
        blockBuilder.DeleteBlock(name);
        RefreshSavedList();
    }
}