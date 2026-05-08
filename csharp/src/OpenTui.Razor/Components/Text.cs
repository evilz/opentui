using OpenTui.Core.Renderables;
using OpenTui.Core.Rendering;

namespace OpenTui.Razor.Components;

public sealed class Text : RenderableComponentBase<TextRenderable>
{
    [Parameter] public string Content { get; set; } = string.Empty;
    [Parameter] public string? Fg { get; set; }
    [Parameter] public string? Bg { get; set; }
    [Parameter] public string TextAlign { get; set; } = "left";
    [Parameter] public bool Wrap { get; set; } = true;

    protected override TextRenderable CreateRenderable(CliRenderer renderer) => new(renderer);

    protected override void ApplyParameters(TextRenderable renderable)
    {
        renderable.Content = Content;
        renderable.Fg = Fg;
        renderable.Bg = Bg;
        renderable.TextAlign = TextAlign;
        renderable.Wrap = Wrap;
    }
}
