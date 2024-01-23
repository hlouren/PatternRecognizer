using System;

namespace OutSystems.Model.PatternRecognizer.Tokens;

/// <summary>
/// Represents a terminal used in a grammar rule
/// </summary>
internal class Terminal : IComparable<Terminal>, IEquatable<Terminal> {
    public string Type { get; }
    public TerminalKind Kind { get; }

    public Terminal(string type, TerminalKind kind) {
        Type = type;
        Kind = kind;
    }

    public override string ToString() => Kind switch {
        TerminalKind.Open => $"<{Type}>",
        TerminalKind.Close => $"</{Type}>",
        TerminalKind.None => $"<{Type}/>",
        _ => throw new NotImplementedException($"Missing case for {Kind}")
    };

    public override bool Equals(object? obj) => obj is Terminal other && Equals(other);

    public bool Equals(Terminal? other) => other != null && Type == other.Type && Kind == other.Kind;

    public override int GetHashCode() => Type.GetHashCode() ^ Kind.GetHashCode();

    public int CompareTo(Terminal? other) {
        var typeCompare = Type.CompareTo(other?.Type);
        if (typeCompare != 0) {
            return typeCompare;
        }

        return Kind.CompareTo(other?.Kind);
    }
}
