using System;
using OutSystems.Model.Parser;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    partial class RuleItemSet {

        public abstract record Transition<TSymbolId, TSymbol>(TSymbolId Token) {

            public abstract GeneralLRParser<TTerminal, TResult, TContext>.ISymbolTarget<TSymbol> GetTarget(Func<RuleItemSet, ItemSetInfo> getState);
        }
    }
}