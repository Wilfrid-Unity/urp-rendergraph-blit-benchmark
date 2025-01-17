using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MaxQueuedFramesSwitcher : MonoBehaviour
{
    public GameObject TextCurrentMaxQueuedFrames;
    public GameObject TextCurrentGpuStress;

    void Start()
    {
        TextCurrentMaxQueuedFrames.GetComponent<TMP_Text>().text = "MaxQueuedFrames: " + QualitySettings.maxQueuedFrames;
        TextCurrentGpuStress.GetComponent<TMP_Text>().text = "Gpu Operations: " + StressGpuRenderFeature.gpuStressOperationsCount;
    }

    public void SwitchMaxQueuedFrames()
    {
        QualitySettings.maxQueuedFrames = 3 - QualitySettings.maxQueuedFrames;
        TextCurrentMaxQueuedFrames.GetComponent<TMP_Text>().text = "MaxQueuedFrames: " + QualitySettings.maxQueuedFrames;
    }

    public void ToggleDebugHud()
    {
        // TODO
    }

    public void IncreaseGpuLoad()
    {
        //UniversalRenderPipelineAsset gpuStressRpAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
        //ScriptableRenderer stressGpuRenderer = gpuStressRpAsset.GetRenderer(0);
        //ScriptableRendererData rendererData = gpuStressRpAsset.scriptableRendererData;

        StressGpuRenderFeature.gpuStressOperationsCount += 1;
        TextCurrentGpuStress.GetComponent<TMP_Text>().text = "Gpu Operations: " + StressGpuRenderFeature.gpuStressOperationsCount;
    }

    public void DecreaseGpuLoad()
    {
        StressGpuRenderFeature.gpuStressOperationsCount = Mathf.Max(StressGpuRenderFeature.gpuStressOperationsCount - 1, 0); 
        TextCurrentGpuStress.GetComponent<TMP_Text>().text = "Gpu Operations: " + StressGpuRenderFeature.gpuStressOperationsCount;
    }
}
