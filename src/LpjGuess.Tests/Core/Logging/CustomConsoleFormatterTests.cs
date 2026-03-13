using System.Text.RegularExpressions;
using LpjGuess.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LpjGuess.Tests.Core.Logging;

public class CustomConsoleFormatterTests
{
    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        private readonly List<Action<T, string?>> listeners = new();

        public TestOptionsMonitor(T initial)
        {
            CurrentValue = initial;
        }

        public T CurrentValue { get; private set; }

        public T Get(string? name) => CurrentValue;

        public IDisposable OnChange(Action<T, string?> listener)
        {
            listeners.Add(listener);
            return new ActionDisposable(() => listeners.Remove(listener));
        }

        public void Update(T value)
        {
            CurrentValue = value;
            foreach (var listener in listeners.ToArray())
                listener(value, null);
        }

        private sealed class ActionDisposable : IDisposable
        {
            private readonly Action action;
            public ActionDisposable(Action action) => this.action = action;
            public void Dispose() => action();
        }
    }

    [Fact]
    public void Write_DoesNothing_WhenFormatterReturnsNullMessage()
    {
        var monitor = new TestOptionsMonitor<CustomConsoleFormatterOptions>(new CustomConsoleFormatterOptions
        {
            TimestampFormat = "HH:mm:ss ",
            IncludeScopes = false
        });
        var formatter = new CustomConsoleFormatter(monitor);
        using var writer = new StringWriter();

        var entry = new LogEntry<string>(
            LogLevel.Information,
            "Category",
            0,
            "state",
            null,
            (_, _) => null!);

        formatter.Write(in entry, null, writer);

        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void Write_IncludesScopeMessageAndException_WhenConfigured()
    {
        var monitor = new TestOptionsMonitor<CustomConsoleFormatterOptions>(new CustomConsoleFormatterOptions
        {
            TimestampFormat = "HH:mm:ss ",
            IncludeScopes = true
        });
        var formatter = new CustomConsoleFormatter(monitor);
        using var writer = new StringWriter();

        var scopeProvider = new LoggerExternalScopeProvider();
        using (scopeProvider.Push("scope-1"))
        {
            var entry = new LogEntry<string>(
                LogLevel.Error,
                "My.Category",
                0,
                "state",
                new InvalidOperationException("boom"),
                (_, ex) => $"message {ex!.Message}");

            formatter.Write(in entry, scopeProvider, writer);
        }

        string output = writer.ToString();
        Assert.Contains("My.Category:", output);
        Assert.Contains("[scope-1]", output);
        Assert.Contains("message boom", output);
        Assert.Contains("System.InvalidOperationException: boom", output);
        Assert.Contains("fail", output);
    }

    [Fact]
    public void Write_UsesValidatedTimestampFormat_WhenOptionsChange()
    {
        var monitor = new TestOptionsMonitor<CustomConsoleFormatterOptions>(new CustomConsoleFormatterOptions
        {
            TimestampFormat = "HH:mm:ss ",
            IncludeScopes = false
        });
        var formatter = new CustomConsoleFormatter(monitor);

        monitor.Update(new CustomConsoleFormatterOptions
        {
            TimestampFormat = "HH:mm:ss",
            IncludeScopes = false
        });

        using var writer = new StringWriter();
        var entry = new LogEntry<string>(
            LogLevel.Information,
            "Cat",
            0,
            "state",
            null,
            (_, _) => "hello");

        formatter.Write(in entry, null, writer);

        string firstLine = writer.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)[0];
        Assert.Matches(new Regex(@"\[\d{2}:\d{2}:\d{2} info\] Cat: hello"), firstLine);
    }

    [Fact]
    public void Write_UsesUnknownLevelToken_ForUnsupportedLogLevel()
    {
        var monitor = new TestOptionsMonitor<CustomConsoleFormatterOptions>(new CustomConsoleFormatterOptions
        {
            TimestampFormat = "HH:mm:ss ",
            IncludeScopes = false
        });
        var formatter = new CustomConsoleFormatter(monitor);
        using var writer = new StringWriter();

        var entry = new LogEntry<string>(
            (LogLevel)999,
            "Cat",
            0,
            "state",
            null,
            (_, _) => "hello");

        formatter.Write(in entry, null, writer);

        Assert.Contains("????", writer.ToString());
    }
}
