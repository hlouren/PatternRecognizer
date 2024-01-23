using System;
using System.Collections.Generic;
using OutSystems.Model.Parser.Classifiers;

namespace OutSystems.Model.ParserGenerator.Conditions;

public interface ICondition<TSymbol> : IEquatable<ICondition<TSymbol>> {
    internal IClassifier<TSymbol> GetClassifier(IEnumerable<IClassifier<TSymbol>> classifiers);
    internal int GetClassifierValue(IClassifier<TSymbol> classifier);
    internal IClassifier<TSymbol> CreateOrUpdateClassifier(IEnumerable<IClassifier<TSymbol>> classifiers, out bool isNew);
}
