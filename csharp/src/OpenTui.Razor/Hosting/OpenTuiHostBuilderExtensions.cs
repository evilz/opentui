using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenTui.Razor.Hosting;

public static class OpenTuiHostBuilderExtensions
{
    public static IHostBuilder UseOpenTuiRazor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(this IHostBuilder hostBuilder)
        where TComponent : IComponent
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.AddOpenTuiRazor();
            services.AddHostedService<OpenTuiHostedService<TComponent>>();
        });

        return hostBuilder;
    }
}
