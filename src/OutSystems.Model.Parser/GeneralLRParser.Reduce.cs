namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public sealed record Reduce(int RuleNumber) : ParserAction {

        public override string ToString() => $"Reduce({RuleNumber})";
    }
}