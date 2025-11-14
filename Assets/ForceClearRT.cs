using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class ForceClearRT : MonoBehaviour
{
    public Color clearColor = new Color(0, 0, 0, 0);

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam != GetComponent<Camera>()) return;

        var cmd = CommandBufferPool.Get("ForceClearRT");
        cmd.ClearRenderTarget(true, true, clearColor);
        ctx.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
