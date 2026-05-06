using Microsoft.AspNetCore.Components;
using OpenTui.Core.Renderables;
using OpenTui.Core.Rendering;

namespace OpenTui.Razor.Components;

public sealed class Select : RenderableComponentBase<SelectRenderable>
{
    [Parameter] public List<SelectOption> Options { get; set; } = [];
    [Parameter] public int SelectedIndex { get; set; }
    [Parameter] public EventCallback<int> SelectedIndexChanged { get; set; }
    [Parameter] public EventCallback<int> OnSelectionChanged { get; set; }
    [Parameter] public EventCallback<SelectOption> OnItemSelected { get; set; }
    [Parameter] public bool ShowDescription { get; set; } = true;
    [Parameter] public bool ShowScrollIndicator { get; set; } = true;
    [Parameter] public string? SelectedBg { get; set; } = "#0055aa";
    [Parameter] public string? Fg { get; set; }

    protected override SelectRenderable CreateRenderable(CliRenderer renderer) => new(renderer);

    protected override void OnRenderableCreated(SelectRenderable renderable)
    {
        renderable.On("selectionChanged", async data =>
        {
            var value = Convert.ToInt32(data ?? 0);
            await InvokeAsync(async () =>
            {
                if (SelectedIndexChanged.HasDelegate)
                    await SelectedIndexChanged.InvokeAsync(value);
                if (OnSelectionChanged.HasDelegate)
                    await OnSelectionChanged.InvokeAsync(value);
            });
        });

        renderable.On("itemSelected", async data =>
        {
            if (data is not SelectOption option)
                return;

            await InvokeAsync(async () =>
            {
                if (OnItemSelected.HasDelegate)
                    await OnItemSelected.InvokeAsync(option);
            });
        });
    }

    protected override void ApplyParameters(SelectRenderable renderable)
    {
        renderable.Options = Options;
        renderable.SelectedIndex = SelectedIndex;
        renderable.ShowDescription = ShowDescription;
        renderable.ShowScrollIndicator = ShowScrollIndicator;
        renderable.SelectedBg = SelectedBg;
        renderable.Fg = Fg;
    }
}
