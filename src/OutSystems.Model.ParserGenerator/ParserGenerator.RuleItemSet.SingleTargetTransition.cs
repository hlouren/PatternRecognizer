using System;
using OutSystems.Model.Parser;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    partial class RuleItemSet {

        public record SingleTargetTransition<TSymbolId, TSymbol>(
            TSymbolId Token,
            RuleItemSet TargetState) : Transition<TSymbolId, TSymbol>(Token) {

            public override GeneralLRParser<TTerminal, TResult, TContext>.ISymbolTarget<TSymbol> GetTarget(Func<RuleItemSet, ItemSetInfo> getState) =>
                new GeneralLRParser<TTerminal, TResult, TContext>.SingleTarget<TSymbol>(getState(TargetState).Number);
        }
    }
}