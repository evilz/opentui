using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using OpenTui.Core.Renderables;

namespace OpenTui.Razor.Components;

public abstract class ContainerRenderableComponentBase<TRenderable> : RenderableComponentBase<TRenderable>, IRenderableParent where TRenderable : Renderable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent == null)
            return;

        builder.OpenComponent<CascadingValue<IRenderableParent>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<IRenderableParent>.Value), this);
        builder.AddAttribute(2, nameof(CascadingValue<IRenderableParent>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<IRenderableParent>.ChildContent), (RenderFragment)(childBuilder => childBuilder.AddContent(4, ChildContent)));
        builder.CloseComponent();
    }

    public void AddChild(Renderable child)
    {
        if (Renderable.GetChildren().All(existing => existing.Id != child.Id))
            Renderable.Add(child);
    }

    public void RemoveChild(Renderable child)
    {
        Renderable.Remove(child.Id);
    }
}
