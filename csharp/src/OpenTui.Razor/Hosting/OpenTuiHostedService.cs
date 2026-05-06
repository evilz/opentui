using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Hosting;

namespace OpenTui.Razor.Hosting;

internal sealed class OpenTuiHostedService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
    OpenTuiAppContext app,
    NoopComponentRenderer renderer,
    IHostApplicationLifetime lifetime) : BackgroundService where TComponent : IComponent
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await renderer.MountComponentAsync<TComponent>().ConfigureAwait(false);
            await Task.Run(() => app.Renderer.Start(), stoppingToken).ConfigureAwait(false);
        }
        finally
        {
            app.Renderer.Destroy();
            lifetime.StopApplication();
        }
    }
}
