using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
#pragma warning disable BL0006

namespace OpenTui.Razor.Hosting;

internal sealed class NoopComponentRenderer(IServiceProvider services, ILoggerFactory loggerFactory) : Renderer(services, loggerFactory)
{
    private sealed class SerializedDispatcher : Dispatcher
    {
        private readonly object _sync = new();
        private readonly AsyncLocal<bool> _isExecuting = new();
        private Task _tail = Task.CompletedTask;

        public override bool CheckAccess() => _isExecuting.Value;

        public override Task InvokeAsync(Action workItem)
        {
            ArgumentNullException.ThrowIfNull(workItem);
            return InvokeAsync(() =>
            {
                workItem();
                return Task.CompletedTask;
            });
        }

        public override Task InvokeAsync(Func<Task> workItem)
        {
            ArgumentNullException.ThrowIfNull(workItem);

            if (CheckAccess())
                return workItem();

            lock (_sync)
            {
                var task = _tail.ContinueWith(_ => ExecuteAsync(workItem), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
                _tail = task.ContinueWith(static _ => { }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                return task;
            }
        }

        public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
        {
            ArgumentNullException.ThrowIfNull(workItem);
            return InvokeAsync(() => Task.FromResult(workItem()));
        }

        public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
        {
            ArgumentNullException.ThrowIfNull(workItem);

            if (CheckAccess())
                return workItem();

            lock (_sync)
            {
                var task = _tail.ContinueWith(_ => ExecuteAsync(workItem), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
                _tail = task.ContinueWith(static _ => { }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                return task;
            }
        }

        private async Task ExecuteAsync(Func<Task> workItem)
        {
            _isExecuting.Value = true;
            try
            {
                await workItem().ConfigureAwait(false);
            }
            finally
            {
                _isExecuting.Value = false;
            }
        }

        private async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> workItem)
        {
            _isExecuting.Value = true;
            try
            {
                return await workItem().ConfigureAwait(false);
            }
            finally
            {
                _isExecuting.Value = false;
            }
        }
    }

    private readonly Dispatcher _dispatcher = new SerializedDispatcher();

    public override Dispatcher Dispatcher => _dispatcher;

    public Task MountComponentAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>()
        where TComponent : IComponent
    {
        return Dispatcher.InvokeAsync(async () =>
        {
            var component = (TComponent)InstantiateComponent(typeof(TComponent));
            var componentId = AssignRootComponentId(component);
            await RenderRootComponentAsync(componentId).ConfigureAwait(false);
        });
    }

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;

    protected override void HandleException(Exception exception)
    {
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
    }
}
