namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public sealed record Accept() : ParserAction {
        public override string ToString() => "Accept";
    }
}