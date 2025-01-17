using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

// This example copies the active color texture to a new texture. This example is for API demonstrative purposes,
// so the new texture is not used anywhere else in the frame, you can use the frame debugger to verify its contents.
public class StressGpuRenderFeature : ScriptableRendererFeature
{
    class StressGpuRenderPass : ScriptableRenderPass
    {
        Material gpuStressMaterial; // Assign a material with an expensive shader
        int gpuStressOperationsCount;

        // Function used to pass data from the renderer feature to the render pass.
        public void Setup(Material mat, int count)
        {
            gpuStressMaterial = mat;
            gpuStressOperationsCount = count;

            //The pass will read the current color texture. That needs to be an intermediate texture. It's not supported to use the BackBuffer as input texture. 
            //By setting this property, URP will automatically create an intermediate texture. This has a performance cost so don't set this if you don't need it.
            //It's good practice to set it here and not from the RenderFeature. This way, the pass is selfcontaining and you can use it to directly enqueue the pass from a monobehaviour without a RenderFeature.
            requiresIntermediateTexture = true;
        }

        // This is where the renderGraph handle can be accessed.
        // Each ScriptableRenderPass can use the RenderGraph handle to add multiple render passes to the render graph
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // UniversalResourceData contains all the texture handles used by the renderer, including the active color and depth textures
            // The active color and depth textures are the main color and depth buffers that the camera renders into
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            // Create intermediate textures with same dimensions as active color texture
            var renderTextureDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            renderTextureDesc.name = "GpuStressRenderTexture1";
            TextureHandle gpuStressRenderTexture1 = renderGraph.CreateTexture(renderTextureDesc);
            renderTextureDesc.name = "GpuStressRenderTexture2";
            TextureHandle gpuStressRenderTexture2 = renderGraph.CreateTexture(renderTextureDesc);

            // Copy source to first RT
            RenderGraphUtils.BlitMaterialParameters blitParams = new(resourceData.activeColorTexture, gpuStressRenderTexture1, gpuStressMaterial, 0 /*shaderPass*/);
            renderGraph.AddBlitPass(blitParams, passName: "GPU Stress: Blit activeColor to RT1");

            // Ping-pong between render textures multiple times
            for (int i = 0; i < gpuStressOperationsCount; i++)
            {
                if (i % 2 == 0)
                {
                    blitParams = new(gpuStressRenderTexture1, gpuStressRenderTexture2, gpuStressMaterial, 0 /*shaderPass*/);
                }
                else
                {
                    blitParams = new(gpuStressRenderTexture2, gpuStressRenderTexture1, gpuStressMaterial, 0 /*shaderPass*/);
                }
                renderGraph.AddBlitPass(blitParams, passName: "GPU Stress: ping-pong Blit #" + i);
            }

            // Copy final result to destination
            if (gpuStressOperationsCount % 2 == 0)
            {
                blitParams = new(gpuStressRenderTexture1, resourceData.activeColorTexture, gpuStressMaterial, 0 /*shaderPass*/);
            }
            else
            {
                blitParams = new(gpuStressRenderTexture2, resourceData.activeColorTexture, gpuStressMaterial, 0 /*shaderPass*/);
            }
            renderGraph.AddBlitPass(blitParams, passName: "GPU Stress: Blit RT#" + (gpuStressOperationsCount-1) + " to activeColor");
        }
    }

    StressGpuRenderPass m_StressGpuRenderPass;

    [Tooltip("The material used when making the blit operations to stress GPU.")]
    public Material gpuStressMaterial;

    //[Tooltip("The number of additional blit operations to stress GPU.")]
    //public int gpuStressOperationsCount = 6;

    static public int gpuStressOperationsCount = 25;

    /// <inheritdoc/>
    public override void Create()
    {
        m_StressGpuRenderPass = new StressGpuRenderPass();

        // Configures where the render pass should be injected.
        m_StressGpuRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_StressGpuRenderPass.Setup(gpuStressMaterial, gpuStressOperationsCount);
        renderer.EnqueuePass(m_StressGpuRenderPass);
    }
}


