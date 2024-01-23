namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public record SingleTarget<TSymbol>(int TargetState) : ISymbolTarget<TSymbol> {
		public bool TryGetTarget(TSymbol symbol, out int targetState) {
			targetState = TargetState;
			return true;
		}

		public override string ToString() => TargetState.ToString();
	}
}