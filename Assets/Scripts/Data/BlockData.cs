using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "NewBlock", menuName = "Tetris/BlockData")]
    public class BlockData : ScriptableObject
    {
        [Header("Block Configuration")] public Vector2Int[] cells;
        public Color color = Color.white;

        public void Initialize()
        {
            if (cells == null || cells.Length == 0)
            {
                cells = new Vector2Int[] { Vector2Int.zero };
            }
        }
    }
}