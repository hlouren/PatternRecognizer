using System;
using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.PatternRecognizer.PatternItems;

public class ModelItem : PatternItem {

    public PatternItem[] Children { get; }

    public PropertyValueCondition? Condition { get; }

    public ModelItem(string id,
        PatternItem[]? children = null,
        Quantifier quantifier = Quantifier.One,
        string? captureName = null,
        PropertyValueCondition? condition = null)
        : base(id, quantifier, captureName) {
        Children = children ?? Array.Empty<PatternItem>();
        Condition = condition;
    }

    public override bool Equals(PatternItem? other) =>
        other is ModelItem modelItem &&
        base.Equals(other) &&
        Children.Length == modelItem.Children.Length &&
        Children.Zip(modelItem.Children).All(p => p.First.Equals(p.Second)) &&
        (Condition, modelItem.Condition) switch {
            (null, null) => true,
            (null, _) => false,
            (_, null) => false,
            _ => Condition.Equals(modelItem.Condition)
        };

    public override int GetHashCode() => base.GetHashCode() ^ Children.Length;

    public override string ToString() {
        string result;
        if (Children.Any()) {
            result = $"<{Id}> {string.Join(" ", Children.Select(c => c.ToString()))} </{Id}>";
        } else {
            result = $"<{Id}/>";
        }

        if (Quantifier == Quantifier.ZeroOrMore) {
            result = $"({result})*";
        }

        if (Condition != null) {
            result += $"[{Condition}]";
        }

        if (CaptureName != null) {
            if (!result.StartsWith("(")) {
                result = $"({result})";
            }
            result = $"{CaptureName}={result}";
        }

        return result;
    }

    internal override IEnumerable<PatternItem> AllItems => new[] { this }.Concat(Children.SelectMany(c => c.AllItems));
}
