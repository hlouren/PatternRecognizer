namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    private sealed record StateSymbol(int State) : Symbol {

        public override string ToString() => State.ToString();
    }
}