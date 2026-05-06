using OpenTui.Core.Rendering;

namespace OpenTui.Razor.Hosting;

public sealed class OpenTuiAppContext : IDisposable
{
    public CliRenderer Renderer { get; }

    public OpenTuiAppContext()
    {
        Renderer = new CliRenderer(new CliRendererConfig
        {
            ExitOnCtrlC = false,
            TargetFps = 60,
            BackgroundColor = "#000000"
        });
    }

    public void RequestRender() => Renderer.RequestRender();

    public void Dispose() => Renderer.Destroy();
}
