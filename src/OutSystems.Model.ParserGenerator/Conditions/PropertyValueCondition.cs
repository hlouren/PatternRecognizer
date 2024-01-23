using OutSystems.Model.Parser.Classifiers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.ParserGenerator.Conditions;

public record PropertyValueCondition<TSymbol, TProperty>(PropertyGetter<TSymbol, TProperty> PropertyGetter, TProperty Value) : ICondition<TSymbol> {

    IClassifier<TSymbol> ICondition<TSymbol>.GetClassifier(IEnumerable<IClassifier<TSymbol>> classifiers) {
        var classifier = classifiers.OfType<SymbolPropertyClassifier<TSymbol, TProperty>>().FirstOrDefault(c => c.PropertyGetter == PropertyGetter);
        if (classifier == null) {
            throw new InvalidOperationException("Missing classifier");
        }
        return classifier;
    }

    IClassifier<TSymbol> ICondition<TSymbol>.CreateOrUpdateClassifier(IEnumerable<IClassifier<TSymbol>> classifiers, out bool isNew) {
        isNew = false;
        var classifier = classifiers.OfType<SymbolPropertyClassifier<TSymbol, TProperty>>().FirstOrDefault(c => c.PropertyGetter == PropertyGetter);
        if (classifier == null) {
            isNew = true;
            classifier = new(PropertyGetter);
        }

        classifier.Store(Value);
        return classifier;
    }

    int ICondition<TSymbol>.GetClassifierValue(IClassifier<TSymbol> classifier) =>
        ((SymbolPropertyClassifier<TSymbol, TProperty>)classifier).GetCategory(Value);

    public override string ToString() => $"{PropertyGetter} = {FormattedValue}";

    public bool Equals(ICondition<TSymbol>? other) =>
        other is PropertyValueCondition<TSymbol, TProperty> valueCondition &&
        PropertyGetter == valueCondition.PropertyGetter &&
        (Value, valueCondition.Value) switch {
            (null, null) => true,
            (null, _) => false,
            (_, null) => false,
            _ => Value.Equals(valueCondition.Value)
        };

    private string FormattedValue => Value switch {
        string s => $"\"{s.Replace("\"", "\\\"")}\"",
        _ => Value?.ToString() ?? ""
    };

}
