using OutSystems.Model.Parser;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    private sealed partial class RuleItemSet {

        public record AvailableTransitions(
            Transition<TTerminal, TTerminal>[] TerminalTransitions,
            Transition<string, GeneralLRParser<TTerminal, TResult, TContext>.NonTerminalSymbol>[] NonTerminalTransitions);
    }
}