namespace OutSystems.Model.Parser.Classifiers;

/// <summary>
/// Classifies a symbol of type <typeparamref name="TSymbol"/> into an integer category in
/// the range 0 to <see cref="ValueCount"/> (exclusive)
/// </summary>
/// <typeparam name="TSymbol"></typeparam>
public interface IClassifier<TSymbol> {

    int GetCategory(TSymbol symbol);
    int ValueCount { get; }

    // used for debug purposes
    string ToString(int category);
}
