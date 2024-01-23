namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public record ConditionalTarget<TSymbol>(ConditionalTransitionTable<TSymbol, int> TransitionTable) : ISymbolTarget<TSymbol> {

		public bool TryGetTarget(TSymbol symbol, out int targetState) =>
			TransitionTable.TryGetTarget(symbol, out targetState);

		public override string ToString() => TransitionTable.ToString();
	}
}