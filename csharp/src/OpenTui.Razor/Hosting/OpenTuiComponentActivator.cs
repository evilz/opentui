using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTui.Razor.Hosting;

internal sealed class OpenTuiComponentActivator(IServiceProvider services) : IComponentActivator
{
    public IComponent CreateInstance(Type componentType)
        => (IComponent)ActivatorUtilities.CreateInstance(services, componentType);
}
