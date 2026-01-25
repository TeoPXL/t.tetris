using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class AssetGenerator : EditorWindow
    {
        [MenuItem("Tetris/Generate Assets")]
        public static void GenerateAssets()
        {
            CreateBlockTexture();
            CreateBackgroundTexture();

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Asset Generation", "Basic assets generated in Assets/Resources/Textures!",
                "OK");
        }

        private static void CreateBlockTexture()
        {
            EnsureDirectory("Assets/Resources/Textures");

            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }

            texture.SetPixels(colors);
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes("Assets/Resources/Textures/Block.png", bytes);
        }

        private static void CreateBackgroundTexture()
        {
            EnsureDirectory("Assets/Resources/Textures");

            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            Color darkGray = new Color(0.1f, 0.1f, 0.1f);
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = darkGray;
            }

            texture.SetPixels(colors);
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes("Assets/Resources/Textures/Background.png", bytes);
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}