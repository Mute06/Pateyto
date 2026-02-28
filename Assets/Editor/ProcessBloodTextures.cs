using UnityEngine;
using UnityEditor;
using System.IO;

public class ProcessBloodTextures
{
    [MenuItem("Tools/Process Blood Textures")]
    public static void Process()
    {
        string[] files = {
            "Assets/Platformer/Sprites/GeneratedBlood/blood_splatter_raw.png",
            "Assets/Platformer/Sprites/GeneratedBlood/blood_splash_raw.png"
        };
        
        foreach (var file in files)
        {
            if (!File.Exists(file))
            {
                Debug.LogWarning("File not found: " + file);
                continue;
            }

            byte[] bytes = File.ReadAllBytes(file);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            
            Color[] pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                float brightness = (c.r + c.g + c.b) / 3f;
                // If it's almost white, make it fully transparent
                if (brightness > 0.95f)
                {
                    pixels[i] = new Color(c.r, c.g, c.b, 0f);
                }
                else
                {
                    // For darker pixels (blood), make them opaque
                    // To remove the whitish tint from the background, we can darken it and set alpha
                    float alpha = Mathf.Clamp01((1f - brightness) * 1.5f);
                    // Assume blood is deep red, so reconstruct the color
                    // Try to preserve original hue but remove white
                    pixels[i] = new Color(c.r, c.g, c.b, alpha);
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();
            
            string outPath = file.Replace("_raw", "");
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            Debug.Log("Processed: " + outPath);
        }
        
        AssetDatabase.Refresh();
        Debug.Log("Finished processing blood textures.");
    }
}
