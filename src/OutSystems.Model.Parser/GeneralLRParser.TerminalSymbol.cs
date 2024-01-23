namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public sealed record TerminalSymbol(TTerminal Terminal) : Symbol {

        public override string ToString() => (CaptureName == null ? "" : $"{CaptureName}=") + Terminal.ToString();
    }
}