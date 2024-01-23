using System;
using System.Collections.Generic;
using System.Linq;
using OutSystems.Model.PatternRecognizer.PatternItems;

namespace OutSystems.Model.PatternRecognizer;

public partial class PatternRecognizer<TObject, TResult, TContext> {

    public delegate TResult? PatternHandler(
        Pattern pattern,
        TContext context,
        ItemData[] topLevelItemsData,
        Func<string, ItemData> getCapturedData);

    public class Pattern {

        public string Name { get; }
        public string NonTerminal { get; }
        public PatternItem[] Items { get; }
        public PatternHandler? Handler { get; }
        public bool PassThrough { get; }
        private readonly Lazy<IEnumerable<PatternItem>> allItems;

        public Pattern(string name,
            string nonTerminal,
            PatternItem[] items,
            PatternHandler? handler = null,
            bool passThrough = false) {
            Name = name;
            NonTerminal = nonTerminal;
            Items = items;
            Handler = handler;
            PassThrough = passThrough;
            allItems = new(() => Items.SelectMany(i => i.AllItems).ToList());
        }

        public override string ToString() => $"[{Name}] {NonTerminal} -> {string.Join(" ", Items.Select(i => i.ToString()))}";

        public IEnumerable<PatternItem> AllItems => allItems.Value;
    }

}