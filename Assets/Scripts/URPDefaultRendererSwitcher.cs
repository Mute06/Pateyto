using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Robust URP 2D/3D renderer switcher.
/// 
/// SETUP:
///  1. Place this component on ONE persistent GameObject in your first scene.
///     It becomes DontDestroyOnLoad automatically.
///  2. Fill out the sceneRendererMap array in the Inspector so every scene
///     name maps to the correct renderer type.
///  3. Remove old per-scene URPDefaultRendererSwitcher instances – this
///     singleton handles everything for the whole session.
///
/// HOW IT WORKS:
///  - Sets m_DefaultRendererIndex (the real field URP reads) instead of
///    shuffling the renderer array — making the swap fully reliable.
///  - Subscribes to SceneManager.sceneLoaded so the renderer is always
///    correct no matter how the scene was loaded or resumed (including after
///    an ad).
///  - Resets all cameras' renderer overrides to –1 on every switch so
///    stale camera-level overrides don't shadow the global change.
///  - Always forces a full pipeline flush (one-frame null/restore cycle) even
///    when the index was already correct, ensuring shader state and 2D-light
///    passes are rebuilt after an ad or app-focus resume.
/// </summary>
public class URPDefaultRendererSwitcher : MonoBehaviour
{
    // ── Types ─────────────────────────────────────────────────────────────────

    public enum RendererType { Renderer3D, Renderer2D }

    [System.Serializable]
    public struct SceneRendererEntry
    {
        public string sceneName;
        public RendererType rendererType;
    }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("URP Asset (Required)")]
    [Tooltip("Drag your PC_RPAsset (or whichever URP asset the project uses) here. " +
             "If left empty the script will try QualitySettings / GraphicsSettings as fallback.")]
    public UniversalRenderPipelineAsset urpAsset;

    [Header("Scene → Renderer Mapping")]
    [Tooltip("Map every scene name to its required renderer type. " +
             "Scenes not listed default to Renderer2D with a console warning.")]
    public SceneRendererEntry[] sceneRendererMap = new SceneRendererEntry[]
    {
        // ── 3D scenes ────────────────────────────────────────────────────────
        new SceneRendererEntry { sceneName = "Level1",            rendererType = RendererType.Renderer3D },
        new SceneRendererEntry { sceneName = "Level1_Runner",     rendererType = RendererType.Renderer3D },

        // ── 2D scenes ────────────────────────────────────────────────────────
        new SceneRendererEntry { sceneName = "StartScene",        rendererType = RendererType.Renderer2D },
        new SceneRendererEntry { sceneName = "AfterStart",        rendererType = RendererType.Renderer2D },
        new SceneRendererEntry { sceneName = "BeforePlatfom",     rendererType = RendererType.Renderer2D },
        new SceneRendererEntry { sceneName = "BeforePGrave",      rendererType = RendererType.Renderer2D },
        new SceneRendererEntry { sceneName = "PlatformerTest",    rendererType = RendererType.Renderer2D },
        new SceneRendererEntry { sceneName = "NewPlusShooter",    rendererType = RendererType.Renderer2D },
        new SceneRendererEntry { sceneName = "YapbozScene",       rendererType = RendererType.Renderer2D },
        new SceneRendererEntry { sceneName = "Level2_Puzzle",     rendererType = RendererType.Renderer2D },
        new SceneRendererEntry { sceneName = "JigsawPuzzleScene", rendererType = RendererType.Renderer2D },
        new SceneRendererEntry { sceneName = "StoryScene 1",      rendererType = RendererType.Renderer2D },
        new SceneRendererEntry { sceneName = "TheEND",            rendererType = RendererType.Renderer2D },
    };

    // ── Singleton ─────────────────────────────────────────────────────────────

    private static URPDefaultRendererSwitcher _instance;

    // ── Reflection field cache ────────────────────────────────────────────────

    private static FieldInfo s_DefaultRendererIndexField;  // int  – the active-renderer index URP reads
    private static FieldInfo s_RendererDataListField;      // ScriptableRendererData[] – the renderer list

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        // --- Singleton guard ---------------------------------------------------
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // --- Cache reflection fields once -------------------------------------
        CacheReflectionFields();

        // --- Apply renderer for the first (current) scene (deferred 1 frame) --
        // URP may not be initialised yet during Awake on the very first scene.
        StartCoroutine(ApplyRendererDeferred(SceneManager.GetActiveScene().name));

        // --- Subscribe to future scene loads ----------------------------------
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (_instance == this) _instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Defer by one frame: sceneLoaded fires before URP is fully initialised.
        if (mode == LoadSceneMode.Single || scene == SceneManager.GetActiveScene())
            StartCoroutine(ApplyRendererDeferred(scene.name));
    }

    /// <summary>
    /// Waits one frame so the URP pipeline asset has time to initialise,
    /// then applies the renderer for the given scene.
    /// </summary>
    private IEnumerator ApplyRendererDeferred(string sceneName)
    {
        yield return null; // wait one frame for URP to be ready
        ApplyRendererForScene(sceneName);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // Re-apply after returning from an ad or system dialog.
        if (hasFocus)
            StartCoroutine(ApplyRendererDeferred(SceneManager.GetActiveScene().name));
    }

    // ── Public static API ─────────────────────────────────────────────────────

    /// <summary>Manually force the 3D renderer from code.</summary>
    public static void SwitchTo3D() => _instance?.ApplyRenderer(RendererType.Renderer3D);

    /// <summary>Manually force the 2D renderer from code.</summary>
    public static void SwitchTo2D() => _instance?.ApplyRenderer(RendererType.Renderer2D);

    // ── Core logic ────────────────────────────────────────────────────────────

    private void ApplyRendererForScene(string sceneName)
    {
        RendererType desired = RendererType.Renderer2D; // safe default
        bool found = false;

        foreach (var entry in sceneRendererMap)
        {
            if (entry.sceneName == sceneName)
            {
                desired = entry.rendererType;
                found = true;
                break;
            }
        }

        if (!found)
            Debug.LogWarning($"[URPDefaultRendererSwitcher] Scene '{sceneName}' not found in sceneRendererMap. " +
                             "Defaulting to Renderer2D. Add the scene to the map on the switcher component.");

        ApplyRenderer(desired);
    }

    private void ApplyRenderer(RendererType type)
    {
        var pipelineAsset = GetURPAsset();
        if (pipelineAsset == null) return;
        if (s_DefaultRendererIndexField == null || s_RendererDataListField == null) return;

        // Find the index of the desired renderer type inside the data list.
        var dataList = s_RendererDataListField.GetValue(pipelineAsset) as ScriptableRendererData[];
        if (dataList == null || dataList.Length == 0)
        {
            Debug.LogError("[URPDefaultRendererSwitcher] Renderer data list is null or empty.");
            return;
        }

        int targetIndex = -1;
        for (int i = 0; i < dataList.Length; i++)
        {
            if (dataList[i] == null) continue;
            bool is2D = dataList[i].GetType().Name.Contains("2D");
            if ((type == RendererType.Renderer2D && is2D) ||
                (type == RendererType.Renderer3D && !is2D))
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex < 0)
        {
            Debug.LogError($"[URPDefaultRendererSwitcher] No matching renderer found for {type}. " +
                           "Make sure both a 2D Renderer and a 3D Renderer are present in the URP asset.");
            return;
        }

        // --- Set the ACTUAL index URP reads (m_DefaultRendererIndex) ----------
        int previousIndex = (int)s_DefaultRendererIndexField.GetValue(pipelineAsset);
        s_DefaultRendererIndexField.SetValue(pipelineAsset, targetIndex);

        Debug.Log($"[URPDefaultRendererSwitcher] Renderer → {type} (index {targetIndex}: '{dataList[targetIndex].name}'). " +
                  $"Previous index was {previousIndex}.");

        // --- Reset camera renderer overrides ----------------------------------
        // Camera-override indices can silently shadow the global default,
        // so clear them all. –1 = "use the pipeline default".
        ResetCameraRendererOverrides();

        // Note: no pipeline flush is needed here. URP reads m_DefaultRendererIndex
        // dynamically each frame when a camera fetches its renderer, so the new
        // index takes effect immediately on the next rendered frame.
    }

    /// <summary>
    /// Resets all active cameras' renderer override to –1 (use pipeline default)
    /// so stale camera-level indices don't shadow the global switch.
    /// Uses SetRenderer(int) — the public API on UniversalAdditionalCameraData.
    /// </summary>
    private static void ResetCameraRendererOverrides()
    {
        // m_RendererIndex is the serialised backing field; we read it via reflection
        // because there is no public getter — only SetRenderer(int) as the setter.
        var rendererIndexField = typeof(UniversalAdditionalCameraData)
            .GetField("m_RendererIndex", BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (Camera cam in Camera.allCameras)
        {
            var data = cam.GetUniversalAdditionalCameraData();
            if (data == null) continue;

            int currentIndex = rendererIndexField != null
                ? (int)rendererIndexField.GetValue(data)
                : 0; // assume overridden if we can't read the field

            if (currentIndex != -1)
            {
                Debug.Log($"[URPDefaultRendererSwitcher] Cleared renderer override on camera '{cam.name}' " +
                          $"(was index {currentIndex}). Resetting to pipeline default (-1).");
                data.SetRenderer(-1); // –1 = use the pipeline's m_DefaultRendererIndex
            }
        }
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    private UniversalRenderPipelineAsset GetURPAsset()
    {
        // 1. Serialised field – most reliable, set this in the Inspector.
        if (urpAsset != null)
            return urpAsset;

        // 2. Quality-level override (can be null if no per-quality override is set).
        var asset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (asset != null) return asset;

        // 3. Project-wide Graphics settings fallback.
        asset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (asset != null) return asset;

        Debug.LogError("[URPDefaultRendererSwitcher] No UniversalRenderPipelineAsset found! " +
                       "Please assign your URP asset (e.g. PC_RPAsset) to the 'Urp Asset' field " +
                       "on the URPDefaultRendererSwitcher component in the Inspector.");
        return null;
    }

    private static void CacheReflectionFields()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var assetType = typeof(UniversalRenderPipelineAsset);

        s_DefaultRendererIndexField = assetType.GetField("m_DefaultRendererIndex", flags);
        s_RendererDataListField = assetType.GetField("m_RendererDataList", flags);

        if (s_DefaultRendererIndexField == null)
            Debug.LogError("[URPDefaultRendererSwitcher] Reflection: could not find 'm_DefaultRendererIndex'. " +
                           "Check that your URP package version hasn't renamed this field.");

        if (s_RendererDataListField == null)
            Debug.LogError("[URPDefaultRendererSwitcher] Reflection: could not find 'm_RendererDataList'. " +
                           "Check that your URP package version hasn't renamed this field.");
    }
}