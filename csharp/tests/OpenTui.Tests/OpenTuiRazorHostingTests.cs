using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTui.Razor.Hosting;

namespace OpenTui.Tests;

public class OpenTuiRazorHostingTests
{
    [Fact]
    public async Task HostedService_ShutsDownPromptly_WhenHostStops()
    {
        using var host = Host.CreateDefaultBuilder()
            .UseOpenTuiRazor<TestComponent>()
            .ConfigureServices(services =>
            {
                services.Configure<OpenTuiRazorOptions>(options => options.Testing = true);
            })
            .Build();

        await host.StartAsync();

        var stopTask = host.StopAsync();
        await stopTask.WaitAsync(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UseOpenTuiRazor_RegistersRequiredServices()
    {
        using var host = Host.CreateDefaultBuilder()
            .UseOpenTuiRazor<TestComponent>()
            .Build();

        var services = host.Services;

        Assert.NotNull(services.GetService<OpenTuiAppContext>());
        Assert.NotNull(services.GetService<NoopComponentRenderer>());
        Assert.Contains(services.GetServices<IHostedService>(), service => service.GetType().Name.Contains("OpenTuiHostedService"));
    }

    private sealed class TestComponent : IComponent
    {
        private RenderHandle _renderHandle;

        public void Attach(RenderHandle renderHandle) => _renderHandle = renderHandle;

        public Task SetParametersAsync(ParameterView parameters)
        {
            _renderHandle.Render(_ => { });
            return Task.CompletedTask;
        }
    }
}
