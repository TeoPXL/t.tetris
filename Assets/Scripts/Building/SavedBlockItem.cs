using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq; // Added for Max/Min/Avg calculations

public class SavedBlockItem : MonoBehaviour
{
    public TextMeshProUGUI blockNameText;
    public RectTransform previewContainer;
    public Button loadButton;
    public Button deleteButton;
    
    public Sprite pixelSprite; 

    public void Setup(string name, BlockData data, Action<string> onLoad, Action<string> onDelete)
    {
        blockNameText.text = name;

        // Setup Buttons
        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(() => onLoad(name));

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete(name));

        // Generate Visual Preview
        GeneratePreview(data);
    }

    private void GeneratePreview(BlockData data)
    {
        // Clear previous preview
        foreach (Transform child in previewContainer) Destroy(child.gameObject);

        if (data == null || data.cells == null || data.cells.Length == 0 || pixelSprite == null) return;

        // 1. Calculate bounds to center the shape
        int minX = data.cells.Min(c => c.x);
        int maxX = data.cells.Max(c => c.x);
        int minY = data.cells.Min(c => c.y);
        int maxY = data.cells.Max(c => c.y);

        float width = maxX - minX + 1;
        float height = maxY - minY + 1;

        // 2. Determine dynamic size for preview cells based on container size
        float containerWidth = previewContainer.rect.width;
        float containerHeight = previewContainer.rect.height;

        // Choose the smaller dimension of the block to fit within the smaller dimension of the container
        float maxBlockDim = Mathf.Max(width, height); 
        // Ensure minimum scale context for small blocks, e.g., for a 1x1 block
        maxBlockDim = Mathf.Max(maxBlockDim, 3f); 

        // Cell size calculation: Take the smaller side of the container and divide by max block dimension
        float cellSize = Mathf.Min(containerWidth, containerHeight) / maxBlockDim; 

        // Add padding, reduce size slightly
        float actualCellSize = cellSize * 0.9f; 
        
        // 3. Center offset (World coordinates of the center of the bounding box)
        // Center of the entire bounding box is:
        float boundingBoxCenterX = (float)(minX + maxX) / 2f;
        float boundingBoxCenterY = (float)(minY + maxY) / 2f;
        
        // The container center (0, 0 in local space) maps to these coordinates.
        // The position of a cell (cell.x, cell.y) needs to be shifted by this amount.
        
        // 4. Instantiate cells
        foreach (Vector2Int cell in data.cells)
        {
            GameObject pObj = new GameObject("PreviewPixel", typeof(Image));
            pObj.transform.SetParent(previewContainer, false);
            
            Image img = pObj.GetComponent<Image>();
            img.sprite = pixelSprite; 
            img.color = data.color;

            RectTransform rt = pObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(actualCellSize - 1, actualCellSize - 1); // -1 for gap
            
            // Position: (Grid Pos - Grid Center) * Cell Size
            float posX = (cell.x - boundingBoxCenterX) * cellSize;
            float posY = (cell.y - boundingBoxCenterY) * cellSize;
            
            rt.anchoredPosition = new Vector2(posX, posY);
        }
    }
}