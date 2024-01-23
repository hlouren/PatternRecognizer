using System;
using System.Collections.Generic;
using System.Linq;
using OutSystems.Model.Parser.Classifiers;

namespace OutSystems.Model.ParserGenerator.Conditions;

public record BooleanCondition<TSymbol>(Predicate<TSymbol> Predicate) : ICondition<TSymbol> {

    IClassifier<TSymbol> ICondition<TSymbol>.GetClassifier(IEnumerable<IClassifier<TSymbol>> classifiers) {
        var classifier = classifiers.OfType<BooleanClassifier<TSymbol>>().FirstOrDefault(c => c.Predicate == Predicate);
        if (classifier == null) {
            throw new InvalidOperationException("Missing classifier");
        }
        return classifier;
    }

    IClassifier<TSymbol> ICondition<TSymbol>.CreateOrUpdateClassifier(IEnumerable<IClassifier<TSymbol>> classifiers, out bool isNew) {
        isNew = false;
        var classifier = classifiers.OfType<BooleanClassifier<TSymbol>>().FirstOrDefault(c => c.Predicate == Predicate);
        if (classifier == null) {
            classifier = new(Predicate);
            isNew = true;
        }

        return classifier;
    }

    // the classifier for a predicate returns 1 when the predicate is true
    int ICondition<TSymbol>.GetClassifierValue(IClassifier<TSymbol> classifier) => 1;

    public bool Equals(ICondition<TSymbol>? other) =>
        other is BooleanCondition<TSymbol> booleanCondition && Predicate == booleanCondition.Predicate;

    public override string ToString() => "Predicate";
}
