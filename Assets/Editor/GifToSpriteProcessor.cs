using UnityEngine;
using UnityEditor;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Color = UnityEngine.Color;
using Graphics = UnityEngine.Graphics;

public class GifToSpriteProcessor : EditorWindow
{
    private Texture2D sourceGif;
    private float whiteThreshold = 0.95f;
    private bool previewMode = true;

    [MenuItem("Tools/Process GIF to Sprites")]
    public static void ShowWindow()
    {
        GetWindow<GifToSpriteProcessor>("Process GIF to Sprites");
    }

    private void OnGUI()
    {
        GUILayout.Label("GIF to Sprite Processor", EditorStyles.boldLabel);

        sourceGif = (Texture2D)EditorGUILayout.ObjectField("Source GIF", sourceGif, typeof(Texture2D), false);
        whiteThreshold = EditorGUILayout.Slider("White Removal Threshold", whiteThreshold, 0f, 1f);

        GUILayout.Space(10);

        if (GUILayout.Button("Process GIF frames to PNG Sprites"))
        {
            if (sourceGif != null)
            {
                ProcessGif();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a Source GIF texture first.", "OK");
            }
        }
    }

    private void ProcessGif()
    {
        string assetPath = AssetDatabase.GetAssetPath(sourceGif);
        string absolutePath = Path.GetFullPath(assetPath);

        if (!absolutePath.EndsWith(".gif", System.StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("Error", "Selected file must be a .gif!", "OK");
            return;
        }

        string outputFolder = Path.Combine(Path.GetDirectoryName(absolutePath), Path.GetFileNameWithoutExtension(assetPath) + "_Frames");

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        try
        {
            EditorUtility.DisplayProgressBar("Processing GIF", "Reading frames...", 0f);

            // Load the GIF using System.Drawing
            using (Image gifImage = Image.FromFile(absolutePath))
            {
                FrameDimension dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
                int frameCount = gifImage.GetFrameCount(dimension);

                for (int i = 0; i < frameCount; i++)
                {
                    EditorUtility.DisplayProgressBar("Processing GIF", $"Extracting frame {i + 1}/{frameCount}", (float)i / frameCount);

                    gifImage.SelectActiveFrame(dimension, i);

                    // Convert System.Drawing.Image frame to a Bitmap to access pixels
                    using (Bitmap bmp = new Bitmap(gifImage))
                    {
                        // Create Unity Texture2D
                        Texture2D frameTex = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGBA32, false);
                        
                        UnityEngine.Color[] pixels = new UnityEngine.Color[bmp.Width * bmp.Height];

                        for (int y = 0; y < bmp.Height; y++)
                        {
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                System.Drawing.Color c = bmp.GetPixel(x, bmp.Height - 1 - y); // Flip Y for Unity
                                
                                float r = c.R / 255f;
                                float g = c.G / 255f;
                                float b = c.B / 255f;
                                float a = c.A / 255f;

                                float brightness = (r + g + b) / 3f;

                                // Remove white background
                                if (brightness >= whiteThreshold && a > 0f)
                                {
                                    pixels[y * bmp.Width + x] = new UnityEngine.Color(r, g, b, 0f); // Transparent
                                }
                                else
                                {
                                    pixels[y * bmp.Width + x] = new UnityEngine.Color(r, g, b, a);
                                }
                            }
                        }

                        frameTex.SetPixels(pixels);
                        frameTex.Apply();

                        // Save as PNG
                        byte[] pngBytes = frameTex.EncodeToPNG();
                        string framePath = Path.Combine(outputFolder, $"Frame_{i.ToString("D3")}.png");
                        File.WriteAllBytes(framePath, pngBytes);

                        DestroyImmediate(frameTex);
                    }
                }
            }

            AssetDatabase.Refresh();
            ConfigureImportedSprites(outputFolder);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Success", $"Extracted {sourceGif.name} frames successfully to:\n{outputFolder}", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"Error processing GIF: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Error", "An error occurred while processing the GIF. Check console for details.", "OK");
        }
    }

    private void ConfigureImportedSprites(string absoluteFolderPath)
    {
        string relativeFolderPath = "Assets" + absoluteFolderPath.Substring(Application.dataPath.Length).Replace('\\', '/');
        
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { relativeFolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

            if (ti != null)
            {
                bool needsImport = false;
                if (ti.textureType != TextureImporterType.Sprite)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    needsImport = true;
                }
                
                if (ti.spritePixelsPerUnit != 16f) // Assuming common pixel art PPU. Adjust if needed.
                {
                    ti.spritePixelsPerUnit = 16f;
                    needsImport = true;
                }

                if (ti.filterMode != FilterMode.Point)
                {
                    ti.filterMode = FilterMode.Point;
                    needsImport = true;
                }

                if (ti.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    ti.textureCompression = TextureImporterCompression.Uncompressed;
                    needsImport = true;
                }

                if(needsImport)
                {
                     EditorUtility.SetDirty(ti);
                     ti.SaveAndReimport();
                }
            }
        }
    }
}
