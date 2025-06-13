using GLib;

namespace LpjGuess.Frontend.Utility;

/// <summary>
/// Utility class for main thread operations.
/// </summary>
public static class MainThreadHelper
{
    /// <summary>
    /// Executes the specified action on the main GTK thread and returns a Task
    /// that completes when the action has finished.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>A Task that represents the asynchronous execution of the action on the main thread.</returns>
    public static Task RunOnMainThreadAsync(
        Action action,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        Functions.IdleAdd(0, () =>
        {
            try
            {
                if (ct.IsCancellationRequested)
                {
                    tcs.SetCanceled(ct);
                    return false;
                }
                action();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return false; // Ensures the handler runs only once
        });
        return tcs.Task;
    }

    /// <summary>
    /// Executes the specified function on the main GTK thread and returns a Task
    /// whose result is the return value of the function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>A Task that represents the asynchronous execution of the function on the main thread,
    /// yielding the function's result.</returns>
    public static Task<TResult> RunOnMainThreadAsync<TResult>(
        Func<TResult> func,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<TResult>();
        Functions.IdleAdd(0, () =>
        {
            try
            {
                if (ct.IsCancellationRequested)
                {
                    tcs.SetCanceled(ct);
                    return false;
                }
                tcs.SetResult(func());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return false; // Ensures the handler runs only once
        });
        return tcs.Task;
    }
}
