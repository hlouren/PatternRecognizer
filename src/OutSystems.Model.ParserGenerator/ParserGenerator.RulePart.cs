using OutSystems.Model.ParserGenerator.Conditions;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    public class RulePart<TSymbolId, TSymbol> : IRulePart where TSymbolId : notnull {

        public TSymbolId Symbol { get; }
        public string? CaptureName { get; }
        public ICondition<TSymbol>? Condition { get; }

        public RulePart(TSymbolId symbol, string? captureName = null, ICondition<TSymbol>? condition = null) {
            Symbol = symbol;
            CaptureName = captureName;
            Condition = condition;
        }

        public override bool Equals(object? obj) => obj is RulePart<TSymbolId, TSymbol> other && Symbol.Equals(other.Symbol);
        public override int GetHashCode() => Symbol.GetHashCode();
        public bool HasCondition => Condition != null;
    }
}