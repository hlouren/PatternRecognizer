using System;
using System.Collections.Generic;

namespace OutSystems.Model.PatternRecognizer.PatternItems;

public abstract partial class PatternItem : IEquatable<PatternItem> {

    public string Id { get; }
    public Quantifier Quantifier { get; }
    public string? CaptureName { get; }

    protected PatternItem(string id, Quantifier quantifier, string? captureName) {
        Id = id;
        Quantifier = quantifier;
        CaptureName = captureName;
    }

    public override bool Equals(object? obj) => obj is PatternItem other && Equals(other);

    public virtual bool Equals(PatternItem? other) =>
        other != null &&
        Id == other.Id &&
        Quantifier == other.Quantifier &&
        CaptureName == other.CaptureName;

    public override int GetHashCode() => Id.GetHashCode() ^ Quantifier.GetHashCode();

    internal abstract IEnumerable<PatternItem> AllItems { get; }
}
