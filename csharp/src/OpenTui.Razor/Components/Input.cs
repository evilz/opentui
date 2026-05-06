using Microsoft.AspNetCore.Components;
using OpenTui.Core.Renderables;
using OpenTui.Core.Rendering;

namespace OpenTui.Razor.Components;

public sealed class Input : RenderableComponentBase<InputRenderable>
{
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public EventCallback<string> OnInput { get; set; }
    [Parameter] public EventCallback<string> OnChange { get; set; }
    [Parameter] public EventCallback<string> OnEnter { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string? PlaceholderColor { get; set; }
    [Parameter] public string? CursorColor { get; set; }
    [Parameter] public string? Fg { get; set; }
    [Parameter] public string? Bg { get; set; }
    [Parameter] public int? MaxLength { get; set; }

    protected override InputRenderable CreateRenderable(CliRenderer renderer) => new(renderer);

    protected override void OnRenderableCreated(InputRenderable renderable)
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

        renderable.On("change", async data =>
        {
            var value = data?.ToString() ?? string.Empty;
            await InvokeAsync(async () =>
            {
                if (ValueChanged.HasDelegate)
                    await ValueChanged.InvokeAsync(value);
                if (OnChange.HasDelegate)
                    await OnChange.InvokeAsync(value);
            });
        });

        renderable.On("enter", async data =>
        {
            var value = data?.ToString() ?? string.Empty;
            await InvokeAsync(async () =>
            {
                if (OnEnter.HasDelegate)
                    await OnEnter.InvokeAsync(value);
            });
        });
    }

    protected override void ApplyParameters(InputRenderable renderable)
    {
        renderable.Value = Value ?? string.Empty;
        renderable.Placeholder = Placeholder;
        renderable.PlaceholderColor = PlaceholderColor;
        renderable.CursorColor = CursorColor;
        renderable.Fg = Fg;
        renderable.Bg = Bg;
        renderable.MaxLength = MaxLength;
    }
}
