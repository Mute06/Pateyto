using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Changes the active Universal Render Pipeline renderer by swapping the order
/// of the renderers inside the URP Asset's renderer list.
/// </summary>
public class URPDefaultRendererSwitcher : MonoBehaviour
{
    public enum RendererType
    {
        Renderer3D = 0,
        Renderer2D = 1
    }

    [Header("Startup Settings")]
    [Tooltip("Which renderer should be actived on Awake?")]
    public RendererType startRenderer = RendererType.Renderer3D;

    private void Awake()
    {
        SetRenderer(startRenderer);
    }

    public void SetRendererTo3D()
    {
        SetRenderer(RendererType.Renderer3D);
    }

    public void SetRendererTo2D()
    {
        SetRenderer(RendererType.Renderer2D);
    }

    public void SetRenderer(RendererType type)
    {
        // Try getting render pipeline from QualitySettings or GraphicsSettings
        var pipelineAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (pipelineAsset == null)
            pipelineAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;

        if (pipelineAsset == null)
        {
            Debug.LogError("Current Render Pipeline is not a Universal Render Pipeline.");
            return;
        }

        // Use reflection to access the internal m_RendererDataList field
        FieldInfo rendererDataListField = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (rendererDataListField != null)
        {
            ScriptableRendererData[] rendererDataList = (ScriptableRendererData[])rendererDataListField.GetValue(pipelineAsset);
            
            if (rendererDataList == null || rendererDataList.Length < 2)
            {
                Debug.LogError("Not enough renderers in the URP asset to swap. Ensure you have both 2D and 3D renderers in the asset's list.");
                return;
            }

            int targetIndex = -1;
            for (int i = 0; i < rendererDataList.Length; i++)
            {
                if (rendererDataList[i] != null)
                {
                    // Renderer2DData is typically used for 2D, UniversalRendererData for 3D. 
                    // Using string matching on type name is robust enough across nested versions.
                    bool is2D = rendererDataList[i].GetType().Name.Contains("2D");
                    
                    if (type == RendererType.Renderer2D && is2D)
                    {
                        targetIndex = i;
                        break;
                    }
                    else if (type == RendererType.Renderer3D && !is2D)
                    {
                        targetIndex = i;
                        break;
                    }
                }
            }

            if (targetIndex != -1 && targetIndex != 0)
            {
                // Swap the target renderer to index 0
                ScriptableRendererData temp = rendererDataList[0];
                rendererDataList[0] = rendererDataList[targetIndex];
                rendererDataList[targetIndex] = temp;

                // Apply the modified array back
                rendererDataListField.SetValue(pipelineAsset, rendererDataList);
                Debug.Log($"[URPDefaultRendererSwitcher] Swapped {type} to index 0.");

                // Force the pipeline to reconstruct itself with the new renderer layout
                var tempQuality = QualitySettings.renderPipeline;
                if (tempQuality != null)
                {
                    QualitySettings.renderPipeline = null;
                    QualitySettings.renderPipeline = tempQuality;
                }
                else
                {
                    var graphicsTemp = GraphicsSettings.defaultRenderPipeline;
                    GraphicsSettings.defaultRenderPipeline = null;
                    GraphicsSettings.defaultRenderPipeline = graphicsTemp;
                }
            }
            else if (targetIndex == 0)
            {
                // Already the default item at index 0
                Debug.Log($"[URPDefaultRendererSwitcher] {type} is already the default (at index 0). No swap needed.");
            }
            else
            {
                Debug.LogError($"[URPDefaultRendererSwitcher] Could not find the appropriate renderer data for {type} in the renderer list.");
            }
        }
        else
        {
            Debug.LogError("[URPDefaultRendererSwitcher] Could not find m_RendererDataList field. URP internal structure might have changed.");
        }
    }
}
