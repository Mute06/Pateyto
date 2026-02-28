using UnityEngine;
using UnityEditor;
using System.IO;

public class ApplyBloodTextures
{
    [MenuItem("Tools/Apply New Blood Textures")]
    public static void Apply()
    {
        string splatterPath = "Assets/Platformer/Sprites/GeneratedBlood/blood_splatter.png";
        string splashPath = "Assets/Platformer/Sprites/GeneratedBlood/blood_splash.png";

        // Convert texture type to Sprite if not already
        MakeAssetSprite(splatterPath);
        MakeAssetSprite(splashPath);

        Sprite splatterSprite = AssetDatabase.LoadAssetAtPath<Sprite>(splatterPath);
        Sprite splashSprite = AssetDatabase.LoadAssetAtPath<Sprite>(splashPath);

        if (splatterSprite == null || splashSprite == null)
        {
            Debug.LogError("Could not find the processed transparent sprites! Did you run 'Tools -> Process Blood Textures' first?");
            return;
        }

        // Apply to ground splatter
        string groundPrefabPath = "Assets/Platformer/Prefabs/BloodGroundSplatter.prefab";
        GameObject groundObj = AssetDatabase.LoadAssetAtPath<GameObject>(groundPrefabPath);
        if (groundObj != null)
        {
            var bv = groundObj.GetComponent<BloodVisual>();
            if (bv != null)
            {
                bv.randomSprites = new Sprite[] { splatterSprite };
            }
            var sr = groundObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = splatterSprite;
            }
            EditorUtility.SetDirty(groundObj);
        }

        // Apply to background splash
        string splashPrefabPath = "Assets/Platformer/Prefabs/BloodBackgroundSplash.prefab";
        GameObject splashObj = AssetDatabase.LoadAssetAtPath<GameObject>(splashPrefabPath);
        if (splashObj != null)
        {
            var bv = splashObj.GetComponent<BloodVisual>();
            if (bv != null)
            {
                bv.randomSprites = new Sprite[] { splashSprite };
            }
            var sr = splashObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = splashSprite;
            }
            EditorUtility.SetDirty(splashObj);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Successfully applied the new Stylized Blood Sprites to the Prefabs!");
    }

    private static void MakeAssetSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
    }
}
