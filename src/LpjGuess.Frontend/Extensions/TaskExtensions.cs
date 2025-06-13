using LpjGuess.Frontend.Utility;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Core.Extensions;

/// <summary>
/// Extension methods for task objects.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Continue with the specified action on the main thread.
    /// </summary>
    /// <param name="task">The task to continue.</param>
    /// <param name="action">The action to perform.</param>
    public static Task ContinueWithOnMainThread(this Task task, Action action)
    {
        return task.ContinueWith(_ => MainView.RunOnMainThread(action));
    }

    /// <summary>
    /// Continue with the specified action on the main thread.
    /// </summary>
    /// <param name="task">The task to continue.</param>
    /// <param name="action">The action to perform.</param>
    public static Task ContinueWithOnMainThread<T>(this Task<T> task, Action<T> action)
    {
        return task.ContinueWith(r => MainView.RunOnMainThread(() => action(r.Result)));
    }

    /// <summary>
    /// Continue on the main thread.
    /// </summary>
    /// <param name="task">The task to continue.</param>
    /// <param name="ct">The cancellation token.</param>
    public static async Task ContinueOnMainThread(this Task task, CancellationToken ct = default)
    {
        // 1. Await the original task, allowing continuation on a thread pool thread
        await task.ConfigureAwait(false);

        // 2. Now, await a new task that is known to complete ON the main thread.
        //    MainThreadHelper.RunOnMainThreadAsync uses IdleAdd, and its TaskCompletionSource
        //    is set from the IdleAdd callback (which is on the main thread).
        //    When 'tcs.SetResult()' is called from the main thread, SynchronizationContext.Current
        //    is the main thread's context. The 'await' here captures that.
        await MainThreadHelper.RunOnMainThreadAsync(() => { /* no-op */ }, ct);

        // Because the above await completed on the main thread (due to SynchronizationContext capture),
        // if this method were to continue, it would be on the main thread.
        // More importantly, the Task returned by THIS ContinueOnMainThread method
        // will transition to its completed state while the SynchronizationContext is the main thread's.
    }

    /// <summary>
    /// Continue on the main thread.
    /// </summary>
    /// <param name="task">The task to continue.</param>
    /// <param name="ct">The cancellation token.</param>
    public static async Task<TResult> ContinueOnMainThread<TResult>(this Task<TResult> task, CancellationToken ct = default)
    {
        // 1. Await original task, get result, continue on thread pool.
        TResult result = await task.ConfigureAwait(false);
        
        // 2. Await a new task that completes ON the main thread and returns the result.
        //    Same logic as above: tcs.SetResult() in MainThreadHelper happens on the main thread,
        //    capturing the main thread's SynchronizationContext for this await.
        return await MainThreadHelper.RunOnMainThreadAsync(() => result, ct);
        
        // The Task<TResult> returned by THIS ContinueOnMainThread method
        // will transition to its completed state (with the result) while the
        // SynchronizationContext is the main thread's.
    }
}

