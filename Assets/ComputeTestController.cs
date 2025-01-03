using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

[ExecuteInEditMode]
public sealed partial class ComputeTestController : MonoBehaviour
{
    [SerializeField, HideInInspector] ComputeShader _compute = null;
    [SerializeField, HideInInspector] Texture2D _texture = null;

    void Start()
      => Shader.SetGlobalTexture("_ComputeTestGlobalTexture", _texture);

    public void ExecutePass(ComputeGraphContext context,
                            TextureHandle source, TextureHandle dest,
                            in TextureDesc desc)
    {
        var cmd = context.cmd;
        cmd.SetComputeTextureParam(_compute, 0, "Source", source);
        cmd.SetComputeTextureParam(_compute, 0, "Destination", dest);

        var xc = (desc.width  - 1) / 8 + 1;
        var yc = (desc.height - 1) / 8 + 1;
        cmd.DispatchCompute(_compute, 0, xc, yc, 1);
    }
}
