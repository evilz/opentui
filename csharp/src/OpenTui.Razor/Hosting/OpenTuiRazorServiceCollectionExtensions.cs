using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenTui.Razor.Hosting;

public static class OpenTuiRazorServiceCollectionExtensions
{
    public static IServiceCollection AddOpenTuiRazor(this IServiceCollection services)
    {
        services.TryAddSingleton<IComponentActivator, OpenTuiComponentActivator>();
        services.TryAddSingleton<OpenTuiAppContext>();
        services.TryAddSingleton<NoopComponentRenderer>();
        services.AddLogging(logging => logging.ClearProviders());
        return services;
    }
}
