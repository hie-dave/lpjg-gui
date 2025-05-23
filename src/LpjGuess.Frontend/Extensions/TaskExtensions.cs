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
}

