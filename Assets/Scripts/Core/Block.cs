using UnityEngine;
using UnityEngine.InputSystem;

public class Block : MonoBehaviour
{
    public BlockData data;
    public Board board;
    
    private float stepTime = 1f;
    private float stepTimer = 0f;
    private InputSystem_Actions inputActions;

    public void Initialize(Board board, BlockData data)
    {
        this.board = board;
        this.data = data;
        Sprite square = Resources.Load<Sprite>("Square");

        // Create visual child objects
        foreach (Vector2Int cell in data.cells)
        {
            GameObject piece = new GameObject("Piece");
            piece.transform.SetParent(transform, false);
            piece.transform.localPosition = new Vector3(cell.x, cell.y, 0);
            SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = data.color;
            sr.sortingOrder = 10; 
        }
    }

    private void Awake() { inputActions = new InputSystem_Actions(); }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += OnRotate;
        inputActions.Player.Move.performed += OnMove;
    }

    private void OnDisable() { inputActions.Player.Disable(); }

    private void Update()
    {
        stepTimer += Time.deltaTime;
        if (stepTimer >= stepTime)
        {
            Move(new Vector3Int(0, -1, 0));
            stepTimer = 0f;
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (input.x > 0.5f) Move(new Vector3Int(1, 0, 0));
        else if (input.x < -0.5f) Move(new Vector3Int(-1, 0, 0));
        else if (input.y < -0.5f) Move(new Vector3Int(0, -1, 0));
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        // 1. Rotate tentatively
        transform.Rotate(0, 0, 90);
        
        // 2. Fix rotation drift
        transform.eulerAngles = new Vector3(0, 0, Mathf.Round(transform.eulerAngles.z / 90) * 90);

        // 3. Check if valid using the BOARD'S logic
        if (!board.IsValidPosition(this, Vector3Int.RoundToInt(transform.position)))
        {
            // Try Wall Kicks
            if (TryWallKick(new Vector3Int(1, 0, 0))) return;
            if (TryWallKick(new Vector3Int(-1, 0, 0))) return;
            if (TryWallKick(new Vector3Int(0, 1, 0))) return; // Floor kick
            
            // If all fail, rotate back
            transform.Rotate(0, 0, -90); 
        }
    }

    private bool TryWallKick(Vector3Int offset)
    {
        // Test position with offset
        Vector3Int testPos = Vector3Int.RoundToInt(transform.position) + offset;
        
        if (board.IsValidPosition(this, testPos)) 
        {
            transform.position += offset;
            return true;
        }
        return false;
    }

    private void Move(Vector3Int translation)
    {
        // Calculate where we WANT to go
        Vector3Int newPosition = Vector3Int.RoundToInt(transform.position) + translation;

        // Ask the Board if that spot is valid
        if (board.IsValidPosition(this, newPosition))
        {
            transform.position = newPosition;
        }
        else
        {
            // If we failed to move DOWN, lock the block
            if (translation.y == -1)
            {
                enabled = false; // Disable input
                board.LockBlock(this);
            }
        }
    }
}