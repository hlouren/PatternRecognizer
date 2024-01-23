using System;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.Extensions.Logging;

namespace OutSystems.Model.Tests.Shared;

public partial class Tracer : ILogger, IDisposable {

    private readonly StringWriter sw = new();
    private readonly IndentedTextWriter writer;
    private readonly ScopeCloser scopeCloser;
    private bool isEnabled;

    public Tracer() {
        writer = new(sw);
        scopeCloser = new(writer);
        isEnabled = true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        if (isEnabled) {
            writer.WriteLine(state);
        }
        writer.Indent++;
        return scopeCloser;
    }

    public bool IsEnabled(LogLevel logLevel) => isEnabled;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        if (!IsEnabled(logLevel)) {
            return;
        }

        var message = formatter(state, exception);
        writer.WriteLine(message);
    }

    public void WriteLine(string message = "") => writer.WriteLine(message);

    public void Enable() => isEnabled = true;
    public void Disable() => isEnabled = false;

    public void IncrementIndent() => writer.Indent++;
    public void DecrementIndent() => writer.Indent--;

    public void Dispose() {
        sw.Dispose();
        writer.Dispose();
    }

    public override string ToString() => sw.ToString();
}
