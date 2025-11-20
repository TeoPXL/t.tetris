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

        foreach (Vector2Int cell in data.cells)
        {
            GameObject piece = new GameObject("Piece");
            piece.transform.SetParent(transform, false);
            // STRICT INTEGER POSITIONING
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
        transform.Rotate(0, 0, 90);
        
        // Round to eliminate floating point drift
        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            Mathf.Round(transform.position.z)
        );

        if (!IsValid())
        {
            if (TryWallKick(new Vector3Int(1, 0, 0))) return;
            if (TryWallKick(new Vector3Int(-1, 0, 0))) return;
            if (TryWallKick(new Vector3Int(0, 1, 0))) return;
            transform.Rotate(0, 0, -90); // Fail
        }
    }

    private bool TryWallKick(Vector3Int offset)
    {
        transform.position += offset;
        if (IsValid()) return true;
        transform.position -= offset;
        return false;
    }

    private void Move(Vector3Int translation)
    {
        transform.position += translation;
        if (!IsValid())
        {
            transform.position -= translation;
            if (translation.y == -1)
            {
                enabled = false;
                board.LockBlock(this);
            }
        }
    }

    private bool IsValid()
    {
        foreach (Transform child in transform)
        {
            // Use the child's exact world position rounded to Int
            Vector3Int pos = Vector3Int.RoundToInt(child.position);
            if (!board.IsValidPosition(pos)) return false;
        }
        return true;
    }
}