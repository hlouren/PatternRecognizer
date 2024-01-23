using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OutSystems.Model.Parser;
using OutSystems.Model.ParserGenerator.Collections;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> where TTerminal : notnull, IEquatable<TTerminal> {

    private const string ConventionalStartSymbol = "BigBang";

    private readonly List<NumberedRule> rules = new();
    private readonly TTerminal eofTerminal;
    private readonly ILogger? logger = null;

    private readonly RuleItem[] rulesItems;
    private const int AcceptingRuleItemIndex = 1;

    private readonly Dictionary<string, ExtendedSet<TTerminal>> first = new();
    private readonly Dictionary<string, ExtendedSet<TTerminal>> follow = new();
    private readonly Dictionary<RuleItemSet, ItemSetInfo> itemSets = new();
    private readonly Dictionary<string, RuleItem[]> nonTerminalInitialItems;

    private void Trace(string message = "") => logger?.LogTrace(message);

    private ParserGenerator(
        string startSymbol,
        IEnumerable<Rule> rules,
        TTerminal eofTerminal,
        ILogger? logger) {

        // TODO: nameclashing...
        if (startSymbol == ConventionalStartSymbol) {
            throw new ArgumentException("Reserved value", nameof(startSymbol));
        }

        this.rules.Add(new NumberedRule(0,
            LeftHandSide: new NonTerminalRulePart(ConventionalStartSymbol),
            new IRulePart[] {
                new NonTerminalRulePart(startSymbol),
            },
            (_, _, symbols) => ((GeneralLRParser<TTerminal, TResult, TContext>.NonTerminalSymbol)symbols.First()).Result,
            RulePriority.Normal));
        this.rules.AddRange(rules.Select((r, i) => new NumberedRule(i + 1, r.LeftHandSide, r.RightHandSide, r.Handle, r.Priority, r.Name)));

        this.eofTerminal = eofTerminal;
        this.logger = logger;

        rulesItems = this.rules.SelectMany(GetItems).Select((p, i) => new RuleItem(i, p.Rule, p.DotPosition)).ToArray();

        // sanity check...
        var acceptingRuleItem = rulesItems[AcceptingRuleItemIndex];
        var ruleItemIsOK =
            acceptingRuleItem.Rule.LeftHandSide.Symbol == ConventionalStartSymbol &&
            acceptingRuleItem.Rule.RightHandSide.Length == 1 &&
            acceptingRuleItem.DotPosition == 1;
        if (!ruleItemIsOK) {
            throw new InvalidOperationException("Rules items have not been calculated properly");
        }

        nonTerminalInitialItems =
            rulesItems.Where(i => i.DotPosition == 0).GroupBy(i => i.Rule.LeftHandSide.Symbol).
            ToDictionary(g => g.Key, g => g.ToArray());
    }

    private GeneralLRParser<TTerminal, TResult, TContext> GenerateParser() {
        var nonTerminals = rules.Select(r => r.LeftHandSide.Symbol).ToHashSet();
        foreach (var nonTerminal in nonTerminals) {
            first[nonTerminal] = new();
            follow[nonTerminal] = new();
        }

        CalculateFirst();
        CalculateFollow();

        if (logger != null) {
            Trace("Rules");
            Trace("----------");
            foreach (var rule in rules) {
                Trace($"{rule.Number}: {rule}");
            }
            Trace();

            Trace("RuleItems");
            Trace("----------");
            foreach (var item in rulesItems) {
                Trace($"{item.Number}: {item}");
            }
            Trace();

            Trace("First");
            Trace("----------");
            foreach (var kvp in first.OrderBy(kvp => kvp.Key)) {
                Trace($"{kvp.Key}: {kvp.Value}");
            }
            Trace();

            Trace("Follow");
            Trace("----------");
            foreach (var kvp in follow.OrderBy(kvp => kvp.Key)) {
                Trace($"{kvp.Key}: {kvp.Value}");
            }
            Trace();
        }

        var states = new List<ItemSetInfo>();
        var statesToProcess = new Queue<ItemSetInfo>();

        ItemSetInfo GetState(RuleItemSet itemSet) {
            var targetState = GetOrCreateState(itemSet, out var isNewState);
            if (isNewState) {
                statesToProcess.Enqueue(targetState);
                states.Add(targetState);
            }

            return targetState;
        }

        var itemSet0 = GetEpsilonClosure(rulesItems[0]);
        var state0 = GetState(itemSet0);

        void ProcessTransition<TSymbolId, TSymbol>(
            ItemSetInfo sourceState, 
            RuleItemSet.Transition<TSymbolId, TSymbol> transition,
            Action<ItemSetInfo, RuleItemSet.Transition<TSymbolId, TSymbol>, GeneralLRParser<TTerminal, TResult, TContext>.ISymbolTarget<TSymbol>> addTransition) {

            var token = transition.Token;

            using var traceScope = logger?.BeginScope($"Token {token}");

            var target = transition.GetTarget(GetState);
            addTransition(sourceState, transition, target);

            Trace($"Created transition {sourceState.Number} -[{token}]-> {target}");
        }

        void AddTerminalTransition(
            ItemSetInfo sourceState, 
            RuleItemSet.Transition<TTerminal, TTerminal> transition, 
            GeneralLRParser<TTerminal, TResult, TContext>.ISymbolTarget<TTerminal> target) {
            if (!transition.Token.Equals(eofTerminal) || !sourceState.CanonicalSet.IsAccepting) {
                Trace($"Created shift({target}) on {transition.Token}");
                sourceState.LRState.Shift(transition.Token, target);
            }
        }

        void AddNonTerminalTransition(
            ItemSetInfo sourceState, 
            RuleItemSet.Transition<string, GeneralLRParser<TTerminal, TResult, TContext>.NonTerminalSymbol> transition, 
            GeneralLRParser<TTerminal, TResult, TContext>.ISymbolTarget<GeneralLRParser<TTerminal, TResult, TContext>.NonTerminalSymbol> target) {
            Trace($"Created goto({target}) on {transition.Token}");
            if (transition is RuleItemSet.ConditionalTargetTransition<string, GeneralLRParser<TTerminal, TResult, TContext>.NonTerminalSymbol> conditionalTransition) {
                sourceState.LRState.Jump(transition.Token, target, conditionalTransition.Classifiers);
            } else {
                sourceState.LRState.Jump(transition.Token, target);
            }
        }

        while (statesToProcess.Any()) {
            Trace();
            var sourceState = statesToProcess.Dequeue();
            using var traceScope = logger?.BeginScope($"Processing state {sourceState.Number}: {sourceState.CanonicalSet}");

            var transitions = sourceState.CanonicalSet.GetAvailableTransitions();
            foreach (var transition in transitions.TerminalTransitions.OrderBy(t => t.Token)) {
                if (transition is RuleItemSet.ConditionalTargetTransition<TTerminal, TTerminal> conditionalTransition) {
                    sourceState.LRState.TerminalClassifiers(transition.Token, conditionalTransition.Classifiers);
                }
                ProcessTransition(sourceState, transition, AddTerminalTransition);
            }
            foreach (var transition in transitions.NonTerminalTransitions.OrderBy(t => t.Token)) {
                ProcessTransition(sourceState, transition, AddNonTerminalTransition);
            }

            if (sourceState.CanonicalSet.IsAccepting) {
                Trace("Created accept");
                sourceState.LRState.Accept(eofTerminal);
            } else {
                foreach (var rule in sourceState.CanonicalSet.Items.Where(i => i.IsReadyToReduce).Select(i => i.Rule)) {
                    var nonTerminal = rule.LeftHandSide.Symbol;
                    var followSet = follow[nonTerminal].Items;
                    if (!followSet.Any()) {
                        continue;
                    }
                    if (logger != null) {
                        Trace($"Created reduce({rule.Number}) on {string.Join(", ", followSet)}");
                    }
                    foreach (var terminal in followSet) {
                        sourceState.LRState.Reduce(terminal, rule.Number);
                    }
                }
            }
        }

        // until we solve ambiguity, in the case of transitions with multiple action sort them
        // (the parser will chose the first, so here when creating it we've control over how it will behave)
        var rulesPriorities = new Dictionary<int, RulePriority>();
        for (var i = 0; i < rules.Count; i++) {
            rulesPriorities[i] = rules[i].Priority;
        }
        var actionComparer = new ActionComparer(rulesPriorities);
        foreach (var state in states) {
            state.LRState.SortActions(actionComparer);
        }

        var parserRules = rules.
            Select(r => new GeneralLRParser<TTerminal, TResult, TContext>.Rule(
                r.LeftHandSide.Symbol,
                r.RightHandSide.Length,
                r.Handle,
                r.RightHandSide.Select(p => p.CaptureName).ToArray(),
                Description: r.ToString(),
                Name: r.Name)).
                ToArray();

        var parser = new GeneralLRParser<TTerminal, TResult, TContext>(
            states.Select(s => s.LRState).ToArray(),
            parserRules,
            eofTerminal,
            logger);

        if (logger != null) {
            Trace();
            Trace("Parser");
            Trace("------");
            using var traceScope = logger.BeginScope(string.Empty);
            parser.Print(logger);
        }

        return parser;
    }

    private void CalculateFirst() {
        bool changed;
        do {
            changed = false;

            foreach (var rule in rules) {
                var firstSet = first[rule.LeftHandSide.Symbol];

                if (rule.RightHandSide.Length == 0) {
                    changed |= firstSet.AddEpsilon();
                } else {
                    var lastIndex = rule.RightHandSide.Length - 1;
                    for (int i = 0; i < rule.RightHandSide.Length; i++) {
                        var part = rule.RightHandSide[i];

                        if (part is TerminalRulePart t) {
                            changed |= firstSet.Add(t.Symbol);
                            break;
                        } else if (part is NonTerminalRulePart nt) {
                            var ntSet = first[nt.Symbol];

                            // if epsilon is present it is only added to the destination if this is non-terminal
                            // appears as the last part of the rule
                            changed |= firstSet.AddRange(ntSet, addEpsilonIfPresent: i == lastIndex);

                            if (!ntSet.ContainsEpsilon) {
                                break;
                            }
                        } else {
                            throw new InvalidOperationException($"Missing case for {part}");
                        }
                    }
                }
            }
        } while (changed);
    }

    private void CalculateFollow() {
        bool changed;
        follow[ConventionalStartSymbol].Add(eofTerminal);

        do {
            changed = false;

            foreach (var rule in rules) {
                var leftHandSideFollow = follow[rule.LeftHandSide.Symbol];

                var lastIndex = rule.RightHandSide.Length - 1;
                for (int i = 0; i < rule.RightHandSide.Length; i++) {
                    if (rule.RightHandSide[i] is not NonTerminalRulePart nt) {
                        continue;
                    }

                    var ntFollow = follow[nt.Symbol];

                    if (i == lastIndex) {
                        changed |= ntFollow.AddRange(leftHandSideFollow);
                        break;
                    }

                    switch (rule.RightHandSide[i + 1]) {
                        case TerminalRulePart t:
                            changed |= ntFollow.Add(t.Symbol);
                            break;

                        case NonTerminalRulePart nextNt:
                            var nextNtFirst = first[nextNt.Symbol];
                            changed |= ntFollow.AddRange(nextNtFirst, addEpsilonIfPresent: false);
                            if (nextNtFirst.ContainsEpsilon) {
                                changed |= ntFollow.AddRange(leftHandSideFollow);
                            }
                            break;

                        default:
                            throw new InvalidOperationException("Missing case");
                    }
                }
            }
        } while (changed);
    }

    private RuleItemSet GetEpsilonClosure(RuleItem item) {
        var set = new RuleItemSet(this);
        set.Add(item);
        set.EpsilonClose();
        return set;
    }

    private ItemSetInfo GetOrCreateState(RuleItemSet set, out bool isNewState) {
        isNewState = false;
        if (!itemSets.TryGetValue(set, out var stateInfo)) {
            var stateNumber = itemSets.Count;
            var lrState = new GeneralLRParser<TTerminal, TResult, TContext>.State();
            stateInfo = new ItemSetInfo(lrState, stateNumber, set);
            itemSets[set] = stateInfo;
            isNewState = true;

            if (logger != null) {
                logger.WriteLine($"Created set {stateNumber}: {set}");
            }
        }

        return stateInfo;
    }

    private IEnumerable<(NumberedRule Rule, int DotPosition)> GetItems(NumberedRule rule) =>
        Enumerable.Range(0, rule.RightHandSide.Length + 1).Select(i => (rule, i));


    public static GeneralLRParser<TTerminal, TResult, TContext> GenerateParser(
        string startSymbol,
        IEnumerable<Rule> rules,
        TTerminal eofTerminal,
        ILogger? logger = null) =>
        new ParserGenerator<TTerminal, TResult, TContext>(startSymbol, rules, eofTerminal, logger).GenerateParser();
}