using OutSystems.Model.PatternRecognizer.PatternItems;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace OutSystems.Model.PatternRecognizer.Tests;

internal static partial class PatternParser<TObject, TResult, TContext> {

    private const string PatternName = "Pattern";
    private const string PassthroughName = "Passthrough";
    private const string NonTerminalName = "NonTerminal";

    private const string LeadingSpaceName = "LeadingSpace";
    private const string CaptureName = "Capture";
    private const string ItemName = "Item";
    private const string QuantifierName = "Quantifier";
    private const string ConditionName = "Condition";

    [GeneratedRegex($@"^Pattern\s+(?<{PatternName}>(\w|\.)+)(?<{PassthroughName}>!)?\s+:\s+(?<{NonTerminalName}>\w+)$")]
    private static partial Regex PatternRegex();

    [GeneratedRegex($@"^(?<{LeadingSpaceName}>(\s|\|)+)((?<{CaptureName}>\w+)=)?(?<{ItemName}>\w+)(?<{QuantifierName}>\*)?\s*(\[(?<{ConditionName}>\w+)\])?$")]
    private static partial Regex PatternItemRegex();

    private sealed class StackItem {
        public string Id { get; init; }
        public List<PatternItem> Children { get; } = new();
        public Quantifier Quantifier { get; init; } = Quantifier.One;
        public string? CaptureName { get; init; }
        public PropertyValueCondition? Condition { get; init; }
        public int LeadingSpaceCount { get; init; } = 0;

        public StackItem(string id) => Id = id;
    }

    public static IEnumerable<PatternRecognizer<TObject, TResult, TContext>.Pattern> Parse(
        string definitions, 
        Dictionary<string, PatternRecognizer<TObject, TResult, TContext>.PatternHandler>? handlers = null,
        Dictionary<string, PropertyValueCondition>? conditions = null,
        params string[] nonTerminalsNames) {
        var isProcessingPattern = false;
        string patternName = string.Empty;
        bool passThrough = false;
        var items = new Stack<StackItem>();
        var nonTerminals = new HashSet<string>(nonTerminalsNames);

        PatternRecognizer<TObject, TResult, TContext>.Pattern CreatePattern() {
            while (items.Count > 1) {
                CreateModelItem();
            }
            var item = items.Pop();
            var handler =  handlers?.TryGetValue(patternName, out var auxHandler) == true ? auxHandler : null;
            var pattern = new PatternRecognizer<TObject, TResult, TContext>.Pattern(
                patternName, item.Id, item.Children.ToArray(), handler, passThrough);

            isProcessingPattern = false;
            items = new();
            return pattern;
        }

        void CreateModelItem() {
            var item = items!.Pop();
            var parentItem = items.Peek();
            parentItem.Children.Add(new ModelItem(item.Id, item.Children.ToArray(), item.Quantifier, item.CaptureName, item.Condition));
        }

        var lineNumber = 0;
        foreach (var line in definitions.Split("\n").Select(l => l.TrimEnd())) {
            lineNumber++;
            if (line == string.Empty) {
                if (isProcessingPattern) {
                    yield return CreatePattern();
                }
                continue;
            }

            if (!isProcessingPattern) {
                var match = PatternRegex().Match(line);
                if (!match.Success) {
                    throw new ArgumentException($"Cannot parse Pattern at line {lineNumber}");
                }

                patternName = match.Groups[PatternName].Value;
                passThrough = match.Groups[PassthroughName].Value != string.Empty;
                var nonTerminalName = match.Groups[NonTerminalName].Value;
                nonTerminals.Add(nonTerminalName);
                items.Push(new(nonTerminalName));
                isProcessingPattern = true;
            } else {
                var match = PatternItemRegex().Match(line);
                if (!match.Success) {
                    throw new ArgumentException($"Cannot parse PatternItem at line {lineNumber}");
                }

                var leadingSpaceCount = match.Groups[LeadingSpaceName].Value.Length;
                var captureName = match.Groups[CaptureName].Value switch {
                    "" => null,
                    string s => s
                };
                var itemName = match.Groups[ItemName].Value;
                var quantifier = match.Groups[QuantifierName].Value switch {
                    "*" => Quantifier.ZeroOrMore,
                    "" => Quantifier.One,
                    string s => throw new ArgumentException($"Unknown quantifier {s} at line {lineNumber}")
                };
                var condition = match.Groups[ConditionName].Value switch {
                    "" => null,
                    string s => conditions![s]
                };

                if (nonTerminals.Contains(itemName)) {
                    items.Peek().Children.Add(new NonTerminalItem(itemName, quantifier, captureName));
                } else {
                    // model items can contain other model items, thus we cannot immediately create the
                    // ModelItem instance because we don't know if the following line will be a child or
                    // a sibling.
                    // However, when the leading space count decreases then we must pop items until
                    // we get to the same leading space count
                    while (items.Peek().LeadingSpaceCount >= leadingSpaceCount) {
                        CreateModelItem();
                    }

                    items.Push(new StackItem(itemName) {
                        Quantifier = quantifier,
                        CaptureName = captureName,
                        LeadingSpaceCount = leadingSpaceCount,
                        Condition = condition
                    });
                }
            }
        }

        if (isProcessingPattern) {
            yield return CreatePattern();
        }
    }
}
