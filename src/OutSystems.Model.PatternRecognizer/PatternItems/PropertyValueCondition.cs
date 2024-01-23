using System;
using OutSystems.Model.ParserGenerator.Conditions;
using OutSystems.Model.PatternRecognizer.Tokens;

namespace OutSystems.Model.PatternRecognizer.PatternItems;

public abstract class PropertyValueCondition : IEquatable<PropertyValueCondition> {
    internal abstract ICondition<Terminal> AsParserGenerationCondition { get; }

    public bool Equals(PropertyValueCondition? other) =>
        other != null && 
        AsParserGenerationCondition.Equals(other.AsParserGenerationCondition);
}

public class PropertyValueCondition<TProperty> : PropertyValueCondition {

    private readonly ICondition<Terminal> condition;

    public PropertyValueCondition(Type objectType, string propertyName, TProperty propertyValue) {
        condition = new ParserGenerator.Conditions.PropertyValueCondition<Terminal, TProperty>(
            PropertyGetters<TProperty>.Get(objectType, propertyName), propertyValue);
    }

    internal override ICondition<Terminal> AsParserGenerationCondition => condition;

    public override string? ToString() => condition.ToString();
}

public class PropertyValueCondition<TObject, TProperty> : PropertyValueCondition {

    private readonly ICondition<Terminal> condition;

    public PropertyValueCondition(Func<TObject, TProperty> getter, string propertyName, TProperty propertyValue) {
        condition = new ParserGenerator.Conditions.PropertyValueCondition<Terminal, TProperty>(
            PropertyGetters<TProperty>.Get(getter, propertyName), propertyValue);
    }

    internal override ICondition<Terminal> AsParserGenerationCondition => condition;

    public override string? ToString() => condition.ToString();
}
