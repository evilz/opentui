using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace OpenTui.Razor.Hosting;

internal sealed class NoopComponentRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : Renderer(services, loggerFactory)
{
    private sealed class ImmediateDispatcher : Dispatcher
    {
        public override bool CheckAccess() => true;
        public override Task InvokeAsync(Action workItem)
        {
            workItem();
            return Task.CompletedTask;
        }

        public override Task InvokeAsync(Func<Task> workItem) => workItem();

        public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem) => Task.FromResult(workItem());

        public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem) => workItem();
    }

    private static readonly Dispatcher DispatcherInstance = new ImmediateDispatcher();

    public override Dispatcher Dispatcher => DispatcherInstance;

    public Task MountComponentAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>()
        where TComponent : IComponent
    {
        var component = (TComponent)InstantiateComponent(typeof(TComponent));
        var componentId = AssignRootComponentId(component);
        return RenderRootComponentAsync(componentId);
    }

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;

    protected override void HandleException(Exception exception)
    {
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
    }
}
