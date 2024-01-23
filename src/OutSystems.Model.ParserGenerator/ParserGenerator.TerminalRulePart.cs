using OutSystems.Model.ParserGenerator.Conditions;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    public sealed class TerminalRulePart : RulePart<TTerminal, TTerminal> {

        public TerminalRulePart(TTerminal symbol, string? captureName = null, ICondition<TTerminal>? condition = null) :
            base(symbol, captureName, condition) { }

        public override string ToString() => (CaptureName == null ? "" : $"{CaptureName}=") + Symbol.ToString() + (Condition == null ? "" : $"[{Condition}]");
    }
}