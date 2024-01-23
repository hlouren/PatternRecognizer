using System;

namespace OutSystems.Model.Parser.Classifiers;

/// <summary>
/// Classifies symbol of type <typeparamref name="TSymbol"/> into either false (zero) or true (one)
/// using a provided predicate.
/// </summary>
/// <typeparam name="TSymbol"></typeparam>
public class BooleanClassifier<TSymbol> : IClassifier<TSymbol> {

    public Predicate<TSymbol> Predicate { get; }
    public int ValueCount => 2;

    public BooleanClassifier(Predicate<TSymbol> predicate) => Predicate = predicate;

    public int GetCategory(TSymbol symbol) => Predicate(symbol) ? 1 : 0;

    public string ToString(int category) => category == 0 ? "f" : "t";
}
