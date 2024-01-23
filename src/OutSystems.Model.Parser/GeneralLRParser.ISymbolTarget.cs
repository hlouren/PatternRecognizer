namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public interface ISymbolTarget<TSymbol> {
		bool TryGetTarget(TSymbol symbol, out int targetState);
	}
}