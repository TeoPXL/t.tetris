using Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public class Block : MonoBehaviour
    {
        public BlockData data;
        public Board board;

        private float stepTime = 1f;
        private float stepTimer = 0f;
        private InputSystem_Actions inputActions;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }

        public void Initialize(Board board, BlockData data)
        {
            this.board = board;
            this.data = data;
            Sprite square = Resources.Load<Sprite>("Square");

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

        private void OnEnable()
        {
            if (inputActions != null)
            {
                inputActions.Player.Enable();
                // inputActions.Player.Jump.performed += OnRotate; // Removed to avoid conflict with Space (Hard Drop)
                inputActions.Player.Move.performed += OnMove;
            }
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                // inputActions.Player.Jump.performed -= OnRotate;
                inputActions.Player.Move.performed -= OnMove;
                inputActions.Player.Disable();
            }
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();
        }

        private void Update()
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepTime)
            {
                Move(new Vector3Int(0, -1, 0)); // Gravity move (soundless)
                stepTimer = 0f;
            }

            // Input Handling
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                HardDrop();
            }

            if (Keyboard.current.shiftKey.wasPressedThisFrame || Keyboard.current.cKey.wasPressedThisFrame)
            {
                board.HoldBlock();
            }

            if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
            {
                Rotate();
            }
        }

        private void HardDrop()
        {
            while (Move(new Vector3Int(0, -1, 0)))
            {
                // Continue moving down until we hit something
            }
            // The last Move call failed and called LockBlock, so we are done.
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            bool success = false;

            // We capture the result (true/false) of the Move function
            if (input.x > 0.5f) success = Move(new Vector3Int(1, 0, 0));
            else if (input.x < -0.5f) success = Move(new Vector3Int(-1, 0, 0));
            else if (input.y < -0.5f) success = Move(new Vector3Int(0, -1, 0));

            // Only play sound if the move actually happened
            if (success)
            {
                board.PlayMoveSound();
            }
        }

        private void Rotate()
        {
            // 1. Rotate tentatively
            transform.Rotate(0, 0, 90);

            // 2. Fix rotation drift
            transform.eulerAngles = new Vector3(0, 0, Mathf.Round(transform.eulerAngles.z / 90) * 90);

            // 3. Check if valid
            bool valid = board.IsValidPosition(this, Vector3Int.RoundToInt(transform.position));

            if (!valid)
            {
                // Try Wall Kicks
                if (TryWallKick(new Vector3Int(1, 0, 0))) valid = true;
                else if (TryWallKick(new Vector3Int(-1, 0, 0))) valid = true;
                else if (TryWallKick(new Vector3Int(0, 1, 0))) valid = true;

                // If still invalid, revert rotation
                if (!valid)
                {
                    transform.Rotate(0, 0, -90);
                }
            }

            // Play sound if valid
            if (valid)
            {
                board.PlayMoveSound();
            }
        }

        private bool TryWallKick(Vector3Int offset)
        {
            Vector3Int testPos = Vector3Int.RoundToInt(transform.position) + offset;

            if (board.IsValidPosition(this, testPos))
            {
                transform.position += offset;
                return true;
            }

            return false;
        }

        // FIX IS HERE: Changed from 'void' to 'bool'
        private bool Move(Vector3Int translation)
        {
            Vector3Int newPosition = Vector3Int.RoundToInt(transform.position) + translation;

            if (board.IsValidPosition(this, newPosition))
            {
                transform.position = newPosition;
                return true; // Move successful
            }
            else
            {
                if (translation.y == -1)
                {
                    enabled = false;
                    board.LockBlock(this);
                }

                return false; // Move failed
            }
        }
    }
}