using OutSystems.Model.Parser;
using OutSystems.Model.ParserGenerator.Conditions;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    public sealed class NonTerminalRulePart : RulePart<string, GeneralLRParser<TTerminal, TResult, TContext>.NonTerminalSymbol> {

        public NonTerminalRulePart(
            string symbol, 
            string? captureName = null,
            ICondition<GeneralLRParser<TTerminal, TResult, TContext>.NonTerminalSymbol>? condition = null) : 
            base(symbol, captureName, condition) { }

        public override string ToString() => (CaptureName == null ? "" : $"{CaptureName}=") + Symbol + (Condition == null ? "" : $"[{Condition}]");
    }
}