using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OpenTui.Razor.Hosting;

public static class OpenTuiRazorServiceCollectionExtensions
{
    extension(IServiceProvider services)
    {
        public bool HasOpenTuiRazorServices =>
            services.GetService<IServiceProviderIsService>() is { } serviceChecker &&
            serviceChecker.IsService(typeof(OpenTuiAppContext)) &&
            serviceChecker.IsService(typeof(NoopComponentRenderer));
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection AddOpenTuiRazor()
        {
            services.AddOptions<OpenTuiRazorOptions>();
            services.TryAddSingleton<IComponentActivator, OpenTuiComponentActivator>();
            services.TryAddSingleton<OpenTuiAppContext>();
            services.TryAddSingleton<NoopComponentRenderer>();
            return services;
        }
    }
}
