using System;
using System.Collections.Generic;

namespace OutSystems.Model.PatternRecognizer.PatternItems;

public class NonTerminalItem : PatternItem {

    public NonTerminalItem(string id, Quantifier quantifier = Quantifier.One, string? captureName = null) : base(id, quantifier, captureName) { }

    public override bool Equals(PatternItem? other) =>
        other is NonTerminalItem &&
        base.Equals(other);

    public override string ToString() {
        var result = Id;
            
        if (Quantifier == Quantifier.ZeroOrMore) {
            result = $"({result})*";
        }

        if (CaptureName != null) {
            if (!result.StartsWith("(")) {
                result = $"({result})";
            }
            result = $"{CaptureName}={result}";
        }

        return result;
    }

    internal override IEnumerable<PatternItem> AllItems => new[] { this };
}
