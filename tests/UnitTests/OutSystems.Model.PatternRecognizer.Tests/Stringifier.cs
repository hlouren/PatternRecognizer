using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using OutSystems.Model.PatternRecognizer.PatternItems;
using System.Linq;

namespace OutSystems.Model.PatternRecognizer.Tests;

internal static class Stringifier {

    public static string Stringify(this IEnumerable<VirtualWidget> widgets) {
        using var sw = new StringWriter();
        using var writer = new IndentedTextWriter(sw);

        foreach (var widget in widgets) {
            Write(writer, widget);
        }

        return sw.ToString();
    }

    private static void Write(IndentedTextWriter writer, VirtualWidget widget) {
        writer.WriteLine(widget.Type);
        writer.Indent++;

        writer.WriteLine($"NativeWidgets: {{ {string.Join(", ", widget.NativeWidgets.Select(w => w.ToString()))} }}");
        if (widget.Captures?.Any() == true) {
            writer.WriteLine($"Captures:");
            writer.Indent++;
            foreach (var capture in widget.Captures) {
                writer.WriteLine($"{capture.Name} -> {capture.Data}");
            }
            writer.Indent--;
        }

        if (widget.Children.Any()) {
            writer.WriteLine("Children:");
            writer.Indent++;
            foreach (var child in widget.Children) {
                Write(writer, child);
            }
            writer.Indent--;
        }

        writer.Indent--;
    }

    public static string Stringify<TObject, TResult, TContext>(this IEnumerable<PatternRecognizer<TObject, TResult, TContext>.Pattern> patterns) {
        using var sw = new StringWriter();
        using var writer = new IndentedTextWriter(sw);

        foreach (var pattern in patterns) {
            Write(writer, pattern);
        }

        return sw.ToString();
    }

    private static string ToString(Quantifier quantifier) => quantifier switch {
        Quantifier.One => "",
        Quantifier.ZeroOrMore => "*",
        _ => throw new NotImplementedException()
    };

    private static void Write<TObject, TResult, TContext>(IndentedTextWriter writer, PatternRecognizer<TObject, TResult, TContext>.Pattern pattern) {
        writer.WriteLine($"Pattern {pattern.Name}{(pattern.PassThrough ? "!" : "")} : {pattern.NonTerminal}");

        writer.Indent++;

        using var innerWriter = new IndentedTextWriter(writer, "|   ");
        foreach (var item in pattern.Items) {
            Write(innerWriter, (dynamic)item);
        }

        writer.Indent--;
        writer.WriteLine();
    }

    private static void Write(IndentedTextWriter writer, ModelItem item) {
        if (item.CaptureName != null) {
            writer.Write($"{item.CaptureName}=");
        }
        writer.Write($"{item.Id}{ToString(item.Quantifier)}");
        if (item.Condition != null) {
            writer.Write($" [{item.Condition}]");
        }
        writer.WriteLine();

        writer.Indent++;
        foreach (var child in item.Children) {
            Write(writer, (dynamic)child);
        }
        writer.Indent--;
    }

    private static void Write(IndentedTextWriter writer, NonTerminalItem item) {
        if (item.CaptureName != null) {
            writer.Write($"{item.CaptureName}=");
        }
        writer.WriteLine($"{item.Id}{ToString(item.Quantifier)}");
    }
}
