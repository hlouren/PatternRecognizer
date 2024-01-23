using System;
using System.Collections.Generic;
using OutSystems.Model.Parser;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    private class ActionComparer : IComparer<GeneralLRParser<TTerminal, TResult, TContext>.ParserAction> {

        private readonly Dictionary<int, RulePriority> rulesPriority;

        public ActionComparer(Dictionary<int, RulePriority> rulesPriority) =>
            this.rulesPriority = rulesPriority;

        public int Compare(GeneralLRParser<TTerminal, TResult, TContext>.ParserAction? x, GeneralLRParser<TTerminal, TResult, TContext>.ParserAction? y) =>
            GetSortCriteria(x).CompareTo(GetSortCriteria(y));

        private (int, int) GetSortCriteria(GeneralLRParser<TTerminal, TResult, TContext>.ParserAction? action) => action switch {
            GeneralLRParser<TTerminal, TResult, TContext>.Reduce r when rulesPriority[r.RuleNumber] == RulePriority.High =>
                (0, r.RuleNumber),
            GeneralLRParser<TTerminal, TResult, TContext>.Shift => (1, 0),
            GeneralLRParser<TTerminal, TResult, TContext>.Reduce r => (2, r.RuleNumber),
            GeneralLRParser<TTerminal, TResult, TContext>.Accept => (3, 0),
            _ => throw new InvalidOperationException($"Missing case for {action}")
        };
    }
}