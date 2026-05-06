using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OpenTui.Razor.Hosting;

public static class OpenTuiRazorServiceCollectionExtensions
{
    public static IServiceCollection AddOpenTuiRazor(this IServiceCollection services)
    {
        services.AddOptions<OpenTuiRazorOptions>();
        services.TryAddSingleton<IComponentActivator, OpenTuiComponentActivator>();
        services.TryAddSingleton<OpenTuiAppContext>();
        services.TryAddSingleton<NoopComponentRenderer>();
        return services;
    }
}
