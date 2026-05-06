using Microsoft.AspNetCore.Components;
using OpenTui.Core.Layout;
using OpenTui.Core.Renderables;
using OpenTui.Razor.Hosting;

namespace OpenTui.Razor.Components;

public abstract class RenderableComponentBase<TRenderable> : ComponentBase, IDisposable where TRenderable : Renderable
{
    private bool _initialized;

    [Inject] protected OpenTuiAppContext App { get; set; } = null!;
    [CascadingParameter] protected IRenderableParent? Parent { get; set; }

    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Width { get; set; }
    [Parameter] public string? Height { get; set; }
    [Parameter] public FlexDirection? FlexDirection { get; set; }
    [Parameter] public AlignItems? AlignItems { get; set; }
    [Parameter] public JustifyContent? JustifyContent { get; set; }
    [Parameter] public float? FlexGrow { get; set; }
    [Parameter] public float? FlexShrink { get; set; }
    [Parameter] public string? LayoutPosition { get; set; }
    [Parameter] public int? Top { get; set; }
    [Parameter] public int? Left { get; set; }
    [Parameter] public int? Right { get; set; }
    [Parameter] public int? Bottom { get; set; }
    [Parameter] public int? PaddingTop { get; set; }
    [Parameter] public int? PaddingRight { get; set; }
    [Parameter] public int? PaddingBottom { get; set; }
    [Parameter] public int? PaddingLeft { get; set; }
    [Parameter] public int? MarginTop { get; set; }
    [Parameter] public int? MarginRight { get; set; }
    [Parameter] public int? MarginBottom { get; set; }
    [Parameter] public int? MarginLeft { get; set; }
    [Parameter] public int? ZIndex { get; set; }
    [Parameter] public bool? Visible { get; set; }
    [Parameter] public float? Opacity { get; set; }
    [Parameter] public bool? Focusable { get; set; }
    [Parameter] public bool AutoFocus { get; set; }

    public TRenderable Renderable { get; private set; } = null!;

    protected override void OnInitialized()
    {
        Renderable = CreateRenderable(App.Renderer);
        OnRenderableCreated(Renderable);

        if (Parent != null)
            Parent.AddChild(Renderable);
        else
            App.Renderer.Root.Add(Renderable);

        ApplyCommonParameters(Renderable);
        ApplyParameters(Renderable);
        _initialized = true;
        App.RequestRender();
    }

    protected override void OnParametersSet()
    {
        if (!_initialized)
            return;

        ApplyCommonParameters(Renderable);
        ApplyParameters(Renderable);
        App.RequestRender();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && AutoFocus)
        {
            Renderable.Focus();
            App.RequestRender();
        }
    }

    protected virtual void OnRenderableCreated(TRenderable renderable)
    {
    }

    protected abstract TRenderable CreateRenderable(OpenTui.Core.Rendering.CliRenderer renderer);

    protected abstract void ApplyParameters(TRenderable renderable);

    protected void InvokeRender() => App.RequestRender();

    private void ApplyCommonParameters(TRenderable renderable)
    {
        if (Id != null) renderable.Id = Id;
        if (Width != null) renderable.SetWidth(Width);
        if (Height != null) renderable.SetHeight(Height);
        if (FlexDirection.HasValue) renderable.FlexDirection = FlexDirection.Value;
        if (AlignItems.HasValue) renderable.AlignItems = AlignItems.Value;
        if (JustifyContent.HasValue) renderable.JustifyContent = JustifyContent.Value;
        if (FlexGrow.HasValue) renderable.FlexGrow = FlexGrow.Value;
        if (FlexShrink.HasValue) renderable.FlexShrink = FlexShrink.Value;
        if (LayoutPosition != null) renderable.Position = LayoutPosition;
        if (Top.HasValue) renderable.Top = Top.Value;
        if (Left.HasValue) renderable.Left = Left.Value;
        if (Right.HasValue) renderable.Right = Right.Value;
        if (Bottom.HasValue) renderable.Bottom = Bottom.Value;
        if (PaddingTop.HasValue) renderable.PaddingTop = PaddingTop.Value;
        if (PaddingRight.HasValue) renderable.PaddingRight = PaddingRight.Value;
        if (PaddingBottom.HasValue) renderable.PaddingBottom = PaddingBottom.Value;
        if (PaddingLeft.HasValue) renderable.PaddingLeft = PaddingLeft.Value;
        if (MarginTop.HasValue) renderable.MarginTop = MarginTop.Value;
        if (MarginRight.HasValue) renderable.MarginRight = MarginRight.Value;
        if (MarginBottom.HasValue) renderable.MarginBottom = MarginBottom.Value;
        if (MarginLeft.HasValue) renderable.MarginLeft = MarginLeft.Value;
        if (ZIndex.HasValue) renderable.ZIndex = ZIndex.Value;
        if (Visible.HasValue) renderable.Visible = Visible.Value;
        if (Opacity.HasValue) renderable.Opacity = Opacity.Value;
        if (Focusable.HasValue) renderable.Focusable = Focusable.Value;
    }

    public virtual void Dispose()
    {
        if (!_initialized)
            return;

        if (Parent != null)
            Parent.RemoveChild(Renderable);
        else
            App.Renderer.Root.Remove(Renderable.Id);

        Renderable.Destroy();
        App.RequestRender();
    }
}
