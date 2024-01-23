using System.Collections.Generic;
using System.Linq;
using OutSystems.Model.Parser;
using OutSystems.Model.ParserGenerator.Collections;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    private sealed partial class RuleItemSet : FastBitArray {

        private readonly ParserGenerator<TTerminal, TResult, TContext> owner;
        private readonly HashSet<RuleItem> items;

        public RuleItemSet(ParserGenerator<TTerminal, TResult, TContext> owner) : base(owner.rulesItems.Length) {
            this.owner = owner;
            items = new();
        }

        public RuleItemSet(RuleItemSet other) : base(other) {
            owner = other.owner;
            items = new(other.items);
        }

        public bool Contains(RuleItem item) => this[item.Number];

        public bool Add(RuleItem item) {
            if (this[item.Number]) {
                return false;
            }

            this[item.Number] = true;
            items.Add(item);
            return true;
        }

        public IEnumerable<RuleItem> Items => items;

        // the ItemSet is an "accepting" state if it contains the
        //  BigBang -> StartSymbol .
        // rule item, whose position is fixed by construction
        public bool IsAccepting => Items.Any(i => i.Number == AcceptingRuleItemIndex);

        public void EpsilonClose() {
            var processedItems = new HashSet<RuleItem>();
            var itemsToVisit = new Queue<RuleItem>();
            foreach (var item in items) {
                itemsToVisit.Enqueue(item);
            }

            while (itemsToVisit.Any()) {
                var currItem = itemsToVisit.Dequeue();
                if (!processedItems.Add(currItem)) {
                    continue;
                }

                foreach (var newItem in GetEpsilonReachableItems(currItem).Except(items)) {
                    Add(newItem);
                    itemsToVisit.Enqueue(newItem);
                }
            }
        }

        private IEnumerable<RuleItem> GetEpsilonReachableItems(RuleItem fromItem) {
            if (!fromItem.IsReadyToReduce && fromItem.Rule.RightHandSide[fromItem.DotPosition] is NonTerminalRulePart nonTerminal) {
                return owner.nonTerminalInitialItems[nonTerminal.Symbol];
            }

            return Enumerable.Empty<RuleItem>();
        }

        public AvailableTransitions GetAvailableTransitions() {

            var allItemSets = new List<RuleItemSet>();
            var terminalRuleParts = new List<TerminalRulePart>();
            var nonTerminalRuleParts = new List<NonTerminalRulePart>();

            // collect rule items
            var allRuleItems = items.Where(i => !i.IsReadyToReduce).ToList();

            var terminalTransitions = new TransitionData<TTerminal, TTerminal>(this, allRuleItems, allItemSets);
            var nonTerminalTransitions = new TransitionData<string, GeneralLRParser<TTerminal, TResult, TContext>.NonTerminalSymbol>(this, allRuleItems, allItemSets);

            foreach (var itemSet in allItemSets) {
                itemSet.EpsilonClose();
            }

            return new(terminalTransitions.AvailableTransitions, nonTerminalTransitions.AvailableTransitions);
        }

        public override string ToString() => $"{{ {string.Join(", ", Items.OrderBy(i => i.Number))} }}";
    }
}