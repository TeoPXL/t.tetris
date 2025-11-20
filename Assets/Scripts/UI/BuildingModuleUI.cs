using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingModuleUI : MonoBehaviour
{
    public Button saveButton;
    public Button loadButton; // Could open a list
    public Button clearButton;
    public Button backButton;
    public TMP_InputField blockNameInput;
    public Transform gridContainer; // Parent for grid buttons
    public BlockBuilder blockBuilder;

    private void Start()
    {
        if (blockBuilder == null)
            blockBuilder = FindObjectOfType<BlockBuilder>();

        saveButton.onClick.AddListener(SaveBlock);
        clearButton.onClick.AddListener(ClearGrid);
        backButton.onClick.AddListener(GoBack);
    }

    private void GoBack()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogError("GameManager not found!");
            // Fallback or reload scene?
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    private void SaveBlock()
    {
        if (blockBuilder != null)
        {
            string name = blockNameInput.text;
            if (string.IsNullOrEmpty(name)) name = "NewBlock";
            
            BlockData data = blockBuilder.CreateBlockData(name);
            if (data != null)
            {
                blockBuilder.SaveBlock(data);
                Debug.Log($"Block {name} saved!");
            }
        }
    }

    private void ClearGrid()
    {
        // Reset grid visuals
        // For now, we just reload the scene or reset the builder
        Debug.Log("Clear Grid Clicked");
    }
}
