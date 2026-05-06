using OpenTui.Core.Renderables;

namespace OpenTui.Razor.Components;

public interface IRenderableParent
{
    void AddChild(Renderable child);
    void RemoveChild(Renderable child);
}
