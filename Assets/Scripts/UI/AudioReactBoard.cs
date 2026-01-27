using UnityEngine;

namespace UI
{
    public class AudioReactBoard : MonoBehaviour
    {
        [Header("Audio Settings")] [Tooltip("Size of the spectrum analysis. Must be power of 2 (e.g., 64, 128, 256).")]
        public int sampleSize = 64;

        public float sensitivity = 2.0f;
        public float smoothSpeed = 10.0f;

        [Header("Visual Settings")] public Transform boardContainer; // Assign the parent object of your board/grid
        public float minScale = 1.0f;
        public float maxScale = 1.05f;

        private float[] spectrumData;
        private float currentScale;

        private void Start()
        {
            spectrumData = new float[sampleSize];
            currentScale = minScale;

            if (boardContainer == null)
            {
                // If not assigned, try to use this object's transform
                boardContainer = transform;
            }
        }

        private void Update()
        {
            // Get audio spectrum data
            AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);

            // Calculate average amplitude (bass usually sits in lower frequencies)
            float averageAmp = 0;
            // Check first few bands for bass reaction
            int bandsToCheck = sampleSize / 4;
            for (int i = 0; i < bandsToCheck; i++)
            {
                averageAmp += spectrumData[i];
            }

            averageAmp /= bandsToCheck;

            // Calculate target scale based on amplitude
            float targetScale = minScale + (averageAmp * sensitivity);
            targetScale = Mathf.Clamp(targetScale, minScale, maxScale);

            // Smoothly interpolate current scale
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * smoothSpeed);

            // Apply scale to the board
            if (boardContainer != null)
            {
                boardContainer.localScale = new Vector3(currentScale, currentScale, 1f);
            }
        }
    }
}