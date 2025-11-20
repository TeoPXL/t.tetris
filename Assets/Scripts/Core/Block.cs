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

        // Create visuals
        Sprite square = Resources.Load<Sprite>("Square");
        foreach (Vector2Int cell in data.cells)
        {
            GameObject piece = new GameObject("Piece");
            piece.transform.SetParent(transform, false);
            piece.transform.localPosition = (Vector3Int)cell;
            SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = data.color;
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
        transform.Rotate(0, 0, 90);
        if (!board.IsValidPosition(this, Vector3Int.RoundToInt(transform.position)))
        {
            transform.Rotate(0, 0, -90);
        }
    }

    private void Move(Vector3Int translation)
    {
        Vector3Int newPos = Vector3Int.RoundToInt(transform.position) + translation;

        if (board.IsValidPosition(this, newPos))
        {
            transform.position = newPos;
        }
        else
        {
            if (translation.y == -1)
            {
                board.LockBlock(this);
                enabled = false; // Stop processing
            }
        }
    }
}