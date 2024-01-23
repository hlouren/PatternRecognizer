namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public sealed record NonTerminalSymbol(string NonTerminal, TResult Result) : Symbol {
        public override string ToString() => (CaptureName == null ? "" : $"{CaptureName}=") + NonTerminal;
    }
}