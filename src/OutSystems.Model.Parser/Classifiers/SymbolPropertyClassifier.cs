using System;
using System.Collections.Generic;

namespace OutSystems.Model.Parser.Classifiers;

/// <summary>
/// Classifies a symbol based on the value of one of its properties.
/// The stored known values (of type <typeparamref name="TProperty"/>) are sequentially classified into integer values.
/// Non-stored values are mapped to zero.
/// </summary>
/// <typeparam name="TSymbol"></typeparam>
/// <typeparam name="TProperty"></typeparam>
public class SymbolPropertyClassifier<TSymbol, TProperty> : IClassifier<TSymbol> {

    public PropertyGetter<TSymbol, TProperty> PropertyGetter { get; }

    // CS8714: The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    // However, we'll be explicitly managing null values and never use them as keys for the dictionary
#pragma warning disable CS8714
    private readonly Dictionary<TProperty, int> mapping = new();
#pragma warning restore CS8714
    private int? nullValueCategory;

    // used for debug purposes
    private readonly Dictionary<int, TProperty> reverseMapping = new();

    public int ValueCount => mapping.Count + 1 + (nullValueCategory.HasValue ? 1 : 0);

    public SymbolPropertyClassifier(PropertyGetter<TSymbol, TProperty> propertyGetter) =>
        PropertyGetter = propertyGetter;

    public SymbolPropertyClassifier<TSymbol, TProperty> Store(TProperty value) {
        if (value == null) {
            nullValueCategory ??= ValueCount;
        } else if (!mapping.ContainsKey(value)) {
            var intValue = ValueCount;
            mapping[value] = intValue;
            reverseMapping[intValue] = value;
        }

        return this;
    }

    public int GetCategory(TSymbol symbol) => GetCategory(PropertyGetter.GetValue(symbol));

    // used by tests
    public int GetCategory(TProperty? propValue) {
        // the default category is 0 (zero), used for values that haven't been explicitly stored
        if (propValue == null) {
            return nullValueCategory ?? 0;
        } else {
            return mapping.GetValueOrDefault(propValue);
        }
    }

    public string ToString(int category) {
        if (category == nullValueCategory) {
            return "null";
        }

        if (reverseMapping.TryGetValue(category, out var value)) {
            if (value == null) {
                throw new InvalidOperationException("Something went wrong... Null values are not supposed to be stored in the dictionaries");
            }
            return value.ToString() ?? "";
        }

        return "unknown";
    }
}
