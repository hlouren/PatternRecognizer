using System;
using OutSystems.Model.Parser;
using OutSystems.Model.Parser.Classifiers;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    partial class RuleItemSet {

        public record ConditionalTargetTransition<TSymbolId, TSymbol>(
            TSymbolId Token,
            IClassifier<TSymbol>[] Classifiers,
            ConditionalTransitionTable<TSymbol, RuleItemSet> TransitionTable) : Transition<TSymbolId, TSymbol>(Token) {

            public override GeneralLRParser<TTerminal, TResult, TContext>.ISymbolTarget<TSymbol> GetTarget(
                Func<ParserGenerator<TTerminal, TResult, TContext>.RuleItemSet, ParserGenerator<TTerminal, TResult, TContext>.ItemSetInfo> getState) {

                var stateTransitionTable = new ConditionalTransitionTable<TSymbol, int>(Classifiers, i => getState(TransitionTable[i]).Number);
                return new GeneralLRParser<TTerminal, TResult, TContext>.ConditionalTarget<TSymbol>(stateTransitionTable);
            }
        }
    }
}