using Microsoft.AspNetCore.Components;
using OpenTui.Core.Renderables;
using OpenTui.Core.Rendering;

namespace OpenTui.Razor.Components;

public sealed class Textarea : RenderableComponentBase<TextareaRenderable>
{
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public EventCallback<string> OnInput { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string WrapMode { get; set; } = "word";
    [Parameter] public string? Fg { get; set; }
    [Parameter] public string? Bg { get; set; }
    [Parameter] public bool ShowCursor { get; set; } = true;
    [Parameter] public string? CursorColor { get; set; }

    protected override TextareaRenderable CreateRenderable(CliRenderer renderer) => new(renderer);

    protected override void OnRenderableCreated(TextareaRenderable renderable)
    {
        renderable.On("input", async data =>
        {
            var value = data?.ToString() ?? string.Empty;
            await InvokeAsync(async () =>
            {
                if (ValueChanged.HasDelegate)
                    await ValueChanged.InvokeAsync(value);
                if (OnInput.HasDelegate)
                    await OnInput.InvokeAsync(value);
            });
        });
    }

    protected override void ApplyParameters(TextareaRenderable renderable)
    {
        renderable.Value = Value;
        renderable.Placeholder = Placeholder;
        renderable.WrapMode = WrapMode;
        renderable.Fg = Fg;
        renderable.Bg = Bg;
        renderable.ShowCursor = ShowCursor;
        renderable.CursorColor = CursorColor;
    }
}
