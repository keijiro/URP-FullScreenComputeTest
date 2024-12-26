using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

sealed class ComputeTestPass : ScriptableRenderPass
{
    public override void RecordRenderGraph(RenderGraph graph, ContextContainer context)
    {
        // ComputeTestController component reference retrieval
        var camera = context.Get<UniversalCameraData>().camera;
        var ctrl = camera.GetComponent<ComputeTestController>();
        if (ctrl == null || !ctrl.enabled) return;

        // Unsupported case: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Destination texture allocation
        var source = resource.activeColorTexture;
        var desc = graph.GetTextureDesc(source);
        desc.name = "ComputeTest";
        desc.enableRandomWrite = true;
        desc.clearBuffer = false;
        desc.depthBufferBits = 0;
        var dest = graph.CreateTexture(desc);

        // Compute pass registration
        using (var builder = graph.AddComputePass("ComputeTest", out object _))
        {
            builder.UseTexture(source);
            builder.UseTexture(dest, AccessFlags.Write);
            builder.SetRenderFunc((object _, ComputeGraphContext ctx)
                                    => ctrl.ExecutePass(ctx, source, dest, desc));
        }

        // Destination texture as the camera texture
        resource.cameraColor = dest;
    }
}

public sealed class ComputeTestFeature : ScriptableRendererFeature
{
    ComputeTestPass _pass;

    public override void Create()
      => _pass = new ComputeTestPass
           { renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing };

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
      => renderer.EnqueuePass(_pass);
}
