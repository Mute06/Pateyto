using UnityEngine;
using UnityEditor;
using System.IO;

public class ImageDarkener
{
    [MenuItem("Tools/Darken Dungeon Doors")]
    public static void Darken()
    {
        string sourcePath = @"C:/Users/Beray/.gemini/antigravity/brain/tempmediaStorage/media__1772240157245.png";
        string destPath = "Assets/Topdown/TilePalletes/Dungeon_Doors.png";

        if (!File.Exists(sourcePath))
        {
            Debug.LogError("Source image not found at " + sourcePath);
            return;
        }

        byte[] fileData = File.ReadAllBytes(sourcePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);

        // Darken the pixels for a dungeon vibe
        // Shift a bit towards cool colors, reduce brightness and saturation
        Color32[] pixels = tex.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a > 0)
            {
                Color.RGBToHSV(pixels[i], out float h, out float s, out float v);
                
                // Keep the hue but pull it slightly toward a moldy or cold purple/blue tone 
                // Alternatively, just reduce Value and maybe increase S slightly
                v *= 0.6f; // Darken by 40%
                s *= 0.8f; // Desaturate a bit to look older/duskier
                
                Color newColor = Color.HSVToRGB(h, s, v);
                
                // Add a very subtle blueish tint if it's very warm
                newColor.r *= 0.9f;
                newColor.g *= 0.95f;
                newColor.b *= 1.1f;
                
                newColor.a = pixels[i].a / 255f;
                pixels[i] = newColor;
            }
        }
        
        tex.SetPixels32(pixels);
        tex.Apply();

        byte[] newPng = tex.EncodeToPNG();
        File.WriteAllBytes(destPath, newPng);
        
        AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);
        
        // Auto setup slicing and settings
        TextureImporter ti = AssetImporter.GetAtPath(destPath) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.spritePixelsPerUnit = 16;
            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            
            // Generate basic slicing (assuming it's a horizontal strip of doors)
            // Wait, we don't know the size of the doors yet. We have width/height.
            // But we can just set it as a sprite and let the user slice or we can slice by 1/4th if 4 doors.
            int sliceCount = 4;
            int singleWidth = tex.width / sliceCount;
            int height = tex.height;
            var metaDataList = new System.Collections.Generic.List<SpriteMetaData>();
            
            for(int i = 0; i < sliceCount; i++) {
                SpriteMetaData smd = new SpriteMetaData();
                smd.pivot = new Vector2(0.5f, 0.5f);
                smd.alignment = (int)SpriteAlignment.Center;
                smd.name = "Dungeon_Door_" + i;
                smd.rect = new Rect(i * singleWidth, 0, singleWidth, height);
                metaDataList.Add(smd);
            }
            ti.spritesheet = metaDataList.ToArray();
            
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
        }
        
        Debug.Log("Darkened dungeon doors successfully!");
    }
}
