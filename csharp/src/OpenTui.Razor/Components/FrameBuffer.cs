using OpenTui.Core.Renderables;
using OpenTui.Core.Rendering;

namespace OpenTui.Razor.Components;

public sealed class FrameBuffer : RenderableComponentBase<FrameBufferRenderable>
{
    [Parameter] public int PixelWidth { get; set; } = 120;
    [Parameter] public int PixelHeight { get; set; } = 40;
    [Parameter] public Action<FrameBufferRenderable>? Draw { get; set; }

    protected override FrameBufferRenderable CreateRenderable(CliRenderer renderer) => new(renderer, PixelWidth, PixelHeight);

    protected override void ApplyParameters(FrameBufferRenderable renderable)
    {
        Draw?.Invoke(renderable);
    }
}
