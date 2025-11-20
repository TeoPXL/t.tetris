using UnityEngine;
using UnityEngine.InputSystem;

public class Block : MonoBehaviour
{
    public BlockData data;
    public Board board;
    public Vector3 rotationPoint;
    private float stepTime = 1f;
    private float stepTimer = 0f;
    
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += OnRotate;
        inputActions.Player.Move.performed += OnMove;
    }

    private void OnDisable()
    {
        inputActions.Player.Jump.performed -= OnRotate;
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Disable();
    }

    private void Start()
    {
        if (data == null) return;
        // Initialize block visuals based on data
    }

    private void Update()
    {
        if (board == null) return;

        stepTimer += Time.deltaTime;
        if (stepTimer >= stepTime)
        {
            Move(new Vector2Int(0, -1));
            stepTimer = 0f;
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (input.x > 0.5f) Move(new Vector2Int(1, 0));
        else if (input.x < -0.5f) Move(new Vector2Int(-1, 0));
        else if (input.y < -0.5f) Move(new Vector2Int(0, -1));
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        Rotate();
    }

    private void Move(Vector2Int translation)
    {
        transform.position += (Vector3Int)translation;

        if (!board.IsValidPosition(this))
        {
            transform.position -= (Vector3Int)translation;
            if (translation.y == -1) // Hit bottom or another block
            {
                board.LockBlock(this);
            }
        }
    }

    private void Rotate()
    {
        transform.RotateAround(transform.TransformPoint(rotationPoint), Vector3.forward, 90);

        if (!board.IsValidPosition(this))
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint), Vector3.forward, -90);
        }
    }
}
