using OutSystems.Model.Parser;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    private sealed record NumberedRule(
        int Number,
        NonTerminalRulePart LeftHandSide,
        IRulePart[] RightHandSide,
        GeneralLRParser<TTerminal, TResult, TContext>.RuleHandler Handle,
        RulePriority Priority,
        string? Name = null) {

        public override string ToString() => ToString(false);

        public string ToString(bool includeNumber) =>
            (includeNumber ? $"{Number}: " : "") +
            (Priority == RulePriority.High ? "!" : "") +
            $"{LeftHandSide} -> {string.Join<IRulePart>(" ", RightHandSide)}";
    }
}