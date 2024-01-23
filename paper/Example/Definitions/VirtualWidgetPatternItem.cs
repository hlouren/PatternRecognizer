using OutSystems.Model.PatternRecognizer.PatternItems;
using System;
using System.CodeDom.Compiler;
using System.Linq;

namespace Example.Definitions;

internal abstract class VirtualWidgetPatternItem : ModelItem {

    private readonly PropertyValue[]? defaultValues;

    public VirtualWidgetPatternItem(
        Type type,
        PatternItem[]? children = null,
        PropertyValueCondition? condition = null,
        bool repeats = false,
        string? captureName = null,
        PropertyValue[]? defaultValues = null)
        : base(type.Name, children, repeats ? Quantifier.ZeroOrMore : Quantifier.One, captureName, condition) {
        this.defaultValues = defaultValues;
    }

    public void Print(IndentedTextWriter writer) {
        writer.Write($"<{Id}");
        if (CaptureName != null) {
            writer.Write($" Id=\"{CaptureName}\"");
        }
        if (Condition != null) {
            writer.Write($" {Condition.ToString()?.Replace(" = ", "=")}");
        }

        if (defaultValues != null) {
            foreach (var defaultValue in defaultValues) {
                writer.Write($" Default.{defaultValue.Name}=\"{defaultValue.Value}\"");
            }
        }

        if (Children?.Any() != true) {
            writer.WriteLine("/>");
            return;
        }

        writer.WriteLine(">");
        writer.Indent++;
        foreach (var item in Children) {
            // not correct, but currently we've no examples with nonterminals
            ((VirtualWidgetPatternItem)item).Print(writer);
        }
        writer.Indent--;

        writer.WriteLine($"</{Id}>");
    }
}

internal class VirtualWidgetPatternItem<T> : VirtualWidgetPatternItem {

    public VirtualWidgetPatternItem(
        PatternItem[]? children = null,
        PropertyValueCondition? condition = null,
        bool repeats = false,
        string? captureName = null,
        PropertyValue[]? defaultValues = null) : 
        base(typeof(T), children, condition, repeats, captureName, defaultValues) {
    }
}