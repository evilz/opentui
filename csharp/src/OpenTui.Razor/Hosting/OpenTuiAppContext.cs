using Microsoft.Extensions.Options;
using OpenTui.Core.Events;
using OpenTui.Core.Rendering;

namespace OpenTui.Razor.Hosting;

public sealed class OpenTuiRazorOptions
{
    public bool ExitOnCtrlC { get; set; }
    public int TargetFps { get; set; } = 60;
    public string BackgroundColor { get; set; } = "#000000";
}

public sealed class OpenTuiAppContext : IDisposable
{
    public CliRenderer Renderer { get; }
    public EventEmitter KeyEvents => Renderer.KeyInput;

    public OpenTuiAppContext(IOptions<OpenTuiRazorOptions> options)
    {
        var settings = options.Value;
        Renderer = new CliRenderer(new CliRendererConfig
        {
            ExitOnCtrlC = settings.ExitOnCtrlC,
            TargetFps = settings.TargetFps,
            BackgroundColor = settings.BackgroundColor
        });
    }

    public void RequestRender() => Renderer.RequestRender();

    public void Dispose() => Renderer.Destroy();
}
