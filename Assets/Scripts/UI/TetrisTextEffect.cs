using TMPro;
using UnityEngine;

namespace UI
{
    public class TetrisTextEffect : MonoBehaviour
    {
        private TMP_Text textComponent;

        private Color32[] tetrisColors = new Color32[]
        {
            new Color32(0, 255, 255, 255), // Cyan
            new Color32(0, 0, 255, 255), // Blue
            new Color32(255, 165, 0, 255), // Orange
            new Color32(255, 255, 0, 255), // Yellow
            new Color32(0, 255, 0, 255), // Green
            new Color32(128, 0, 128, 255), // Purple
            new Color32(255, 0, 0, 255) // Red
        };

        [Tooltip("Speed of the color shifting wave")]
        public float speed = 3f;

        [Tooltip("Distance between color peaks across characters")]
        public float spread = 1f;

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            if (textComponent == null) return;

            textComponent.ForceMeshUpdate();
            var textInfo = textComponent.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                float offset = (Time.time * speed) + (i * spread);

                int colorIndex = Mathf.FloorToInt(offset) % tetrisColors.Length;
                if (colorIndex < 0) colorIndex += tetrisColors.Length;

                Color32 c = tetrisColors[colorIndex];

                int vertexIndex = charInfo.vertexIndex;
                Color32[] newVertexColors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;

                newVertexColors[vertexIndex + 0] = c;
                newVertexColors[vertexIndex + 1] = c;
                newVertexColors[vertexIndex + 2] = c;
                newVertexColors[vertexIndex + 3] = c;
            }

            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}