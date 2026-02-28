using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AutoSpriteSlicer
{
    [MenuItem("Tools/Slice Room Walls")]
    public static void Slice()
    {
        string path = "Assets/Topdown/TilePalletes/Room_Builder_3d_walls_16x16.png";
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null)
        {
            Debug.LogError("No TextureImporter found at " + path);
            return;
        }
        
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
        {
            Debug.LogError("Texture not found");
            return;
        }
        
        int width = tex.width;
        int height = tex.height;
        int sliceWidth = 16;
        int sliceHeight = 16;
        
        List<SpriteMetaData> metaDataList = new List<SpriteMetaData>();
        int count = 0;
        
        // Unity Sprite Editor slicing order: top-left to bottom-right
        for (int y = height; y >= sliceHeight; y -= sliceHeight)
        {
            for (int x = 0; x <= width - sliceWidth; x += sliceWidth)
            {
                SpriteMetaData smd = new SpriteMetaData();
                smd.pivot = new Vector2(0.5f, 0.5f);
                smd.alignment = (int)SpriteAlignment.Center;
                smd.name = tex.name + "_" + count++;
                smd.rect = new Rect(x, y - sliceHeight, sliceWidth, sliceHeight);
                metaDataList.Add(smd);
            }
        }
        
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritesheet = metaDataList.ToArray();
        
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        
        Debug.Log("Sliced successfully!");
    }
}
