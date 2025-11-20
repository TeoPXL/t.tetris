using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BuildingModuleUI : MonoBehaviour
{
    public BlockBuilder blockBuilder;
    public Button saveButton;
    public Button clearButton;
    public Button backButton;
    public TMP_InputField blockNameInput;
    public Transform gridContainer;
    public Transform savedListContent;

    public GameObject cellPrefab;
    public GameObject addButtonPrefab;
    public GameObject savedItemPrefab;

    private float cellSize = 40f;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSave);
        clearButton.onClick.AddListener(OnClear);
        backButton.onClick.AddListener(() => GameManager.Instance.LoadScene("MainMenu"));

        OnClear(); // Initial Draw
        RefreshSavedList();
    }

    private void OnSave()
    {
        string name = blockNameInput.text;
        if (string.IsNullOrEmpty(name)) return;
        
        blockBuilder.SaveBlock(name);
        blockNameInput.text = "";
        RefreshSavedList();
    }

    private void OnClear()
    {
        blockBuilder.ResetBuilder();
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

    private void RefreshSavedList()
    {
        foreach (Transform child in savedListContent) Destroy(child.gameObject);

        List<string> savedNames = blockBuilder.GetRegistry();
        foreach (string name in savedNames)
        {
            GameObject item = Instantiate(savedItemPrefab, savedListContent);
            
            // Find components by hierarchy order (assumed from SetupTools)
            TextMeshProUGUI txt = item.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            Button loadBtn = item.transform.GetChild(1).GetComponent<Button>();
            Button delBtn = item.transform.GetChild(2).GetComponent<Button>();

            txt.text = name;
            
            string n = name;
            loadBtn.onClick.AddListener(() => {
                blockBuilder.LoadBlockToBuilder(n);
                blockNameInput.text = n;
                RefreshGrid();
            });

            delBtn.onClick.AddListener(() => {
                blockBuilder.DeleteBlock(n);
                RefreshSavedList();
            });
        }
    }
}