using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OutSystems.Model.Parser.Classifiers;

namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public class State {
        private readonly Dictionary<TTerminal, ActionsTableEntry> actions;
        private readonly Dictionary<string, GotoTableEntry> @goto;

        public State(Dictionary<TTerminal, ActionsTableEntry> actions, Dictionary<string, GotoTableEntry> @goto) {
            this.actions = actions;
            this.@goto = @goto;
        }

        public State() {
            actions = new();
            @goto = new();
        }

        public State TerminalClassifiers(TTerminal terminal, IClassifier<TTerminal>[] classifiers) {
            actions[terminal] = new ActionsTableEntry(new(), classifiers);
            return this;
        }

        private State Add(TTerminal terminal, ParserAction action) {
            if (!actions.TryGetValue(terminal, out var entry)) {
                entry = new ActionsTableEntry(new(), Array.Empty<IClassifier<TTerminal>>());
                actions[terminal] = entry;
            }
            entry.Actions.Add(action);

            return this;
        }

        public State Shift(TTerminal terminal, ISymbolTarget<TTerminal> target) {
            // sanity check...
            if (target is ConditionalTarget<TTerminal> &&
                actions.TryGetValue(terminal, out var entry) &&
                !entry.Classifiers.Any()) {
                throw new ArgumentException($"No classifiers have been set for terminal {terminal}");
            }

            return Add(terminal, new Shift(target));
        }

        public State Reduce(TTerminal terminal, int ruleNumber) =>
            Add(terminal, new Reduce(ruleNumber));

        public State Accept(TTerminal terminal) =>
            Add(terminal, new Accept());

        public State Jump(string nonTerminal, ISymbolTarget<NonTerminalSymbol> target, IClassifier<NonTerminalSymbol>[]? classifiers = null) {
            // sanity check...
            if (target is ConditionalTarget<NonTerminalSymbol> && classifiers?.Any() != true) {
                throw new ArgumentException($"No classifiers have been provided for non terminal {nonTerminal}");
            }

            @goto[nonTerminal] = new(target, classifiers ?? Array.Empty<IClassifier<NonTerminalSymbol>>());

            return this;
        }

        public bool TryGetActions(TTerminal token, out IEnumerable<ParserAction> actions) {
            if (!this.actions.TryGetValue(token, out var entry)) {
                actions = Enumerable.Empty<ParserAction>();
                return false;
            }

            actions = entry.Actions.SelectMany(a => a.GetAvailableActions(token));
            return actions.Any();
        }

        public bool TryGetGoto(NonTerminalSymbol nonTerminal, out int targetState) {
            if (!@goto.TryGetValue(nonTerminal.NonTerminal, out var entry)) {
                targetState = default;
                return false;
            }

            return entry.Target.TryGetTarget(nonTerminal, out targetState);
        }

        internal void PrintActions(int index, ILogger logger) {
            foreach (var kvp in actions.OrderBy(kvp => kvp.Key)) {
                logger.WriteLine($"({index}, {kvp.Key}) -> {kvp.Value}");
            }
        }

        internal void PrintGoto(int index, ILogger logger) {
            foreach (var kvp in @goto.OrderBy(kvp => kvp.Key)) {
                logger.WriteLine($"({index}, {kvp.Key}) -> {kvp.Value}");
            }
        }

        public IEnumerable<(TTerminal Token, List<ParserAction>)> ConflictingActions =>
            actions.Where(kvp => kvp.Value.Actions.Count > 1).Select(kvp => (kvp.Key, kvp.Value.Actions));

        public void SortActions(IComparer<ParserAction> comparer) {
            foreach (var entry in actions.Values) {
                entry.SortActions(comparer);
            }
        }
    }
}