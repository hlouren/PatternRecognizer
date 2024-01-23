using System;
using System.Collections.Generic;
using System.Linq;
using OutSystems.Model.Parser.Classifiers;
using OutSystems.Model.ParserGenerator.Conditions;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    partial class RuleItemSet {

        private class TransitionData<TSymbolId, TSymbol> where TSymbolId : notnull {

            private readonly Dictionary<TSymbolId, List<IClassifier<TSymbol>>> classifiersBySymbol = new();
            private readonly Dictionary<TSymbolId, Transition<TSymbolId, TSymbol>> transitionsBySymbol = new();

            public TransitionData(RuleItemSet sourceState, IEnumerable<RuleItem> items, List<RuleItemSet> allItemSets) {
                var allSymbols = new HashSet<TSymbolId>();

                RuleItemSet CreateItemSet() {
                    var result = new RuleItemSet(sourceState.owner);
                    allItemSets.Add(result);
                    return result;
                }

                foreach (var item in items) {
                    if (item.Rule.RightHandSide[item.DotPosition] is not RulePart<TSymbolId, TSymbol> part) {
                        continue;
                    }

                    var (symbol, condition) = (part.Symbol, part.Condition);
                    allSymbols.Add(symbol);
                    if (condition == null) {
                        continue;
                    }

                    if (!classifiersBySymbol.TryGetValue(symbol, out var classifiers)) {
                        classifiers = new();
                        classifiersBySymbol[symbol] = classifiers;
                    }

                    var classifier = condition.CreateOrUpdateClassifier(classifiers, out var isNew);
                    if (isNew) {
                        classifiers.Add(classifier);
                    }
                }

                foreach (var symbol in allSymbols) {
                    if (classifiersBySymbol.TryGetValue(symbol, out var classifiers)) {
                        var classifiersArray = classifiers.ToArray();
                        transitionsBySymbol[symbol] = new ConditionalTargetTransition<TSymbolId, TSymbol>(
                            symbol, classifiersArray, new(classifiersArray, _ => CreateItemSet()));
                    } else {
                        transitionsBySymbol[symbol] = new SingleTargetTransition<TSymbolId, TSymbol>(
                            symbol, CreateItemSet());
                    }
                }

                foreach (var item in items) {
                    if (item.Rule.RightHandSide[item.DotPosition] is not RulePart<TSymbolId, TSymbol> part) {
                        continue;
                    }

                    // by construction, the RuleItem resulting from moving item's dot one position
                    // to the right is stored as the next item in the rulesItems table
                    var targetItem = sourceState.owner.rulesItems[item.Number + 1];

                    var (symbol, condition) = (part.Symbol, part.Condition);
                    switch (transitionsBySymbol[symbol]) {
                        case SingleTargetTransition<TSymbolId, TSymbol> t:
                            t.TargetState.Add(targetItem);
                            break;

                        case ConditionalTargetTransition<TSymbolId, TSymbol> t:
                            if (condition == null) {
                                for (int i = 0; i < t.TransitionTable.Length; i++) {
                                    t.TransitionTable[i].Add(targetItem);
                                }
                            } else {
                                var classifier = condition.GetClassifier(t.Classifiers);
                                var classifierValue = condition.GetClassifierValue(classifier);

                                for (int i = 0; i < t.TransitionTable.Length; i++) {
                                    if (t.TransitionTable.GetClassifierValueAtIndex(classifier, i) == classifierValue) {
                                        t.TransitionTable[i].Add(targetItem);
                                    }
                                }
                            }
                            break;

                        default:
                            throw new NotImplementedException("Missing case");
                    }
                }
            }

            public Transition<TSymbolId, TSymbol>[] AvailableTransitions => transitionsBySymbol.Values.ToArray();
        }
    }
}