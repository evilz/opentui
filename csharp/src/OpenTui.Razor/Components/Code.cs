using OpenTui.Core.Renderables;
using OpenTui.Core.Rendering;

namespace OpenTui.Razor.Components;

public sealed class Code : RenderableComponentBase<CodeRenderable>
{
    [Parameter] public string Content { get; set; } = string.Empty;
    [Parameter] public string Language { get; set; } = string.Empty;
    [Parameter] public bool ShowLineNumbers { get; set; } = true;

    protected override CodeRenderable CreateRenderable(CliRenderer renderer) => new(renderer);

    protected override void ApplyParameters(CodeRenderable renderable)
    {
        renderable.Content = Content;
        renderable.Language = Language;
        renderable.ShowLineNumbers = ShowLineNumbers;
    }
}
