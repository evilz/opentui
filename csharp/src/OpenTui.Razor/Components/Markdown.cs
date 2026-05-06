using OpenTui.Core.Renderables;
using OpenTui.Core.Rendering;

namespace OpenTui.Razor.Components;

public sealed class Markdown : RenderableComponentBase<MarkdownRenderable>
{
    [Parameter] public string Content { get; set; } = string.Empty;

    protected override MarkdownRenderable CreateRenderable(CliRenderer renderer) => new(renderer);

    protected override void ApplyParameters(MarkdownRenderable renderable)
    {
        renderable.Content = Content;
    }
}
