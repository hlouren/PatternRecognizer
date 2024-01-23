using Example.Definitions;
using OutSystems.Model.PatternRecognizer;
using OutSystems.Model.PatternRecognizer.PatternItems;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Example;

internal static class PrintExtensions {

    public static void Print(this object obj, IndentedTextWriter writer) {
        // properties of array type are considered to be children
        var type = obj.GetType();
        var props = type.GetProperties().OrderBy(p => p.Name);
        var normalProps = props.Where(p => !p.PropertyType.IsArray);
        var childProps = props.Where(p => p.PropertyType.IsArray);

        writer.Write($"<{type.Name}");
        foreach (var prop in normalProps) {
            var value = prop.GetValue(obj);
            if (value == null) {
                continue;
            }

            writer.Write($" {prop.Name}=\"{Convert(value)}\"");
        }

        var childCount = 0;

        foreach (var prop in childProps) {
            if (prop.GetValue(obj) is not Array children) {
                continue;
            }

            if (childCount == 0) {
                writer.WriteLine(">");
                writer.Indent++;
            }
            childCount++;

            // not outputting the collection name (only include them if we change the grammar to include
            // the collection names; for space reasons we're omitting them)
            //writer.WriteLine($"<{prop.Name}>");
            //writer.Indent++;

            foreach (var child in children) {
                child.Print(writer);
            }

            //writer.Indent--;
            //writer.WriteLine($"</{prop.Name}>");
        }

        if (childCount > 0) {
            writer.Indent--;
            writer.WriteLine($"</{type.Name}>");
        } else {
            writer.WriteLine("/>");
        }
    }

    private static object? Convert(object? value) {
        var idProp = value?.GetType().GetProperty("Id");
        if (idProp != null) {
            return idProp.GetValue(value);
        }

        return value?.ToString();
    }

    public static void PrintGrammar(
        this IEnumerable<ParserPattern> patterns,
        IndentedTextWriter writer) {

        writer.WriteLine();
        foreach (var group in patterns.GroupBy(p => p.Kind).OrderBy(g => g.Key)) {
            writer.Write($"{group.Key} ::= ");

            writer.Indent++;
            var firstPattern = true;
            foreach (var pattern in group) {
                if (!firstPattern) {
                    writer.Write("    | ");
                }
                WritePatternProduction(writer, pattern);

                writer.WriteLine();
                firstPattern = false;
            }
            writer.Indent--;
        }
    }

    public static void WritePatternProduction<TObject, TResult, TContext>(
        IndentedTextWriter writer,
        PatternRecognizer<TObject, TResult, TContext>.Pattern pattern) {
        var firstItem = true;
        foreach (var item in pattern.Items) {
            WriteItemProduction(writer, (dynamic)item, firstItem);
            firstItem = false;
        }

        if (pattern.Name != string.Empty) {
            writer.Write($" [{pattern.Name}]");
        }
    }

    private static void WriteItemProduction(IndentedTextWriter writer, ModelItem item, bool firstItem) {
        if (!firstItem) {
            writer.Write(" ");
        }

        if (item.Quantifier == Quantifier.ZeroOrMore) {
            writer.Write("(");
        }

        if (item.Children.Any()) {
            writer.Write($"<{item.Id}>");

            if (item.CaptureName != null) {
                writer.Write($"_{item.CaptureName}");
            }

            if (item.Condition != null) {
                writer.Write($"^({item.Condition})");
            }

            foreach (var child in item.Children) {
                WriteItemProduction(writer, (dynamic)child, false);
            }

            writer.Write($" </{item.Id}>");
        } else {
            writer.Write($"<{item.Id}/>");

            if (item.CaptureName != null) {
                writer.Write($"_{item.CaptureName}");
            }

            if (item.Condition != null) {
                writer.Write($"^{item.Condition}");
            }
        }

        if (item.Quantifier == Quantifier.ZeroOrMore) {
            writer.Write(" )*");
        }
    }

    private static void WriteItemProduction(IndentedTextWriter writer, NonTerminalItem item, bool firstItem) {
        if (!firstItem) {
            writer.Write(" ");
        }
        /*
        if (item.CaptureName != null) {
            writer.Write($"{item.CaptureName}=");
        }
        */
        writer.Write($"{item.Id}");
        if (item.Quantifier == Quantifier.ZeroOrMore) {
            writer.Write("*");
        }
    }

    public static void Print<TObject, TResult, TContext>(
        this IEnumerable<PatternRecognizer<TObject, TResult, TContext>.Pattern> patterns,
        IndentedTextWriter writer) {
        foreach (var pattern in patterns) {
            Write(writer, pattern);
        }
    }

    private static string ToString(Quantifier quantifier) => quantifier switch {
        Quantifier.One => "",
        Quantifier.ZeroOrMore => "*",
        _ => throw new NotImplementedException()
    };

    private static void Write<TObject, TResult, TContext>(IndentedTextWriter writer, PatternRecognizer<TObject, TResult, TContext>.Pattern pattern) {
        writer.WriteLine($"Pattern {pattern.Name}");

        writer.Indent++;

        foreach (var item in pattern.Items) {
            Write(writer, (dynamic)item);
        }

        writer.Indent--;
        writer.WriteLine();
    }

    private static void Write(IndentedTextWriter writer, ModelItem item) {
        if (item.CaptureName != null) {
            writer.Write($"{item.CaptureName}:");
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
