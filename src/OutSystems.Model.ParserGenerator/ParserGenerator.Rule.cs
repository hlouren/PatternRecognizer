using System.Linq;
using OutSystems.Model.Parser;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    public sealed record Rule(
        NonTerminalRulePart LeftHandSide,
        IRulePart[] RightHandSide,
        GeneralLRParser<TTerminal, TResult, TContext>.RuleHandler Handle,
        RulePriority Priority = RulePriority.Normal,
        string? Name = null) {

        public override string ToString() =>
            (Priority == RulePriority.High ? "!" : "") +
            (Name == null ? "" : $"[{Name}] ") +
            $"{LeftHandSide.Symbol} -> {string.Join(" ", RightHandSide.Select(p => p.ToString()))}";
    }
}