using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LpjGuess.Core.Utility;

/// <summary>
/// Provides helper methods for throwing exceptions while automatically logging them.
/// </summary>
public static class ExceptionHelper
{
    /// <summary>
    /// Throws and logs an exception with the specified message.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="callerMemberName">Automatically captured caller member name.</param>
    /// <param name="callerFilePath">Automatically captured source file path.</param>
    /// <param name="callerLineNumber">Automatically captured line number.</param>
    /// <typeparam name="T">The type of exception to throw.</typeparam>
    [DoesNotReturn]
    public static T Throw<T>(
        ILogger logger,
        string message,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) where T : Exception, new()
    {
        var exception = (T)Activator.CreateInstance(typeof(T), message)!;
        logger.LogError(
            exception,
            "Exception thrown at {CallerMemberName} in {CallerFilePath}:{CallerLineNumber} - {Message}",
            callerMemberName,
            callerFilePath,
            callerLineNumber,
            message);
        
        throw exception;
    }

    /// <summary>
    /// Throws and logs an exception of type T.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <param name="callerMemberName">Automatically captured caller member name.</param>
    /// <param name="callerFilePath">Automatically captured source file path.</param>
    /// <param name="callerLineNumber">Automatically captured line number.</param>
    /// <typeparam name="T">The type of exception to throw.</typeparam>
    [DoesNotReturn]
    public static T Throw<T>(
        ILogger logger,
        T exception,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) where T : Exception
    {
        logger.LogError(
            exception,
            "Exception thrown at {CallerMemberName} in {CallerFilePath}:{CallerLineNumber} - {Message}",
            callerMemberName,
            callerFilePath,
            callerLineNumber,
            exception.Message);
        
        throw exception;
    }
}
