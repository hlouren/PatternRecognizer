using System.Collections.Generic;

namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public sealed record Shift(ISymbolTarget<TTerminal> Target) : ParserAction {
        public override IEnumerable<ParserAction> GetAvailableActions(TTerminal token) {
            if (Target.TryGetTarget(token, out var targetState)) {
                yield return new SingleTargetShift(targetState);
            }
        }

        public override string ToString() => $"Shift({Target})";
    }
}