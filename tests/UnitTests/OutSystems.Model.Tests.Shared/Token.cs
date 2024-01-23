using System;

namespace OutSystems.Model.Tests.Shared;

public class Token : IComparable<Token>, IEquatable<Token> {
    public string Type { get; }
    public string? Value { get; }

    public Token(string type, string? value) {
        Type = type;
        Value = value;
    }

    public override string ToString() => Type;
    
    public string ToString(bool includeValue) => Value == null ? Type : $"({Type},{Value})";

    public override bool Equals(object? obj) => obj is Token other && Equals(other);

    public bool Equals(Token? other) => other != null && Type == other.Type;

    public override int GetHashCode() => Type.GetHashCode();

    public int CompareTo(Token? other) => Type.CompareTo(other?.Type);
}
