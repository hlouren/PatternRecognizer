using System;

namespace OutSystems.Model.Parser.Classifiers;

public class PropertyGetter<TSymbol, TProperty> {

    private readonly Func<TSymbol, TProperty?> getter;
    private readonly string propertyName;

    public PropertyGetter(Func<TSymbol, TProperty?> getter, string propertyName) {
        this.getter = getter;
        this.propertyName = propertyName;
    }

    public TProperty? GetValue(TSymbol symbol) => getter(symbol);

    public override string ToString() => propertyName;
}
