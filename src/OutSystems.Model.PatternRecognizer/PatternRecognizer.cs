using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using OutSystems.Model.Parser;
using OutSystems.Model.ParserGenerator;
using OutSystems.Model.PatternRecognizer.PatternItems;
using OutSystems.Model.PatternRecognizer.Tokens;

namespace OutSystems.Model.PatternRecognizer;

public partial class PatternRecognizer<TObject, TResult, TContext> {

    private delegate GeneralLRParser<Terminal, ItemData, TContext>.RuleHandler CreatePatternHandler(Pattern pattern);

    // TODO: this is assuming that $ is never used as the type of a terminal
    private static readonly Terminal Eof = new("$", TerminalKind.None);

    // TODO: this is assuming pattern names never start with this prefix
    private const string RepeaterNamePrefix = "Rep_";

    private readonly ITokenizer<TObject> tokenizer;
    private readonly PatternHandler defaultPatternHandler;
    private readonly GeneralLRParser<Terminal, ItemData, TContext> parser;

    private static ItemData? RepeaterRuleHandler(
        TContext context,
        GeneralLRParser<Terminal, ItemData, TContext>.Rule rule,
        List<GeneralLRParser<Terminal, ItemData, TContext>.Symbol> symbols,
        string[] capturedNames) {

        if (!symbols.Any()) {
            var capturedData = capturedNames.Any() ?
                capturedNames.ToDictionary(n => n, _ => new ListData(new Stack<ItemData>())) : null;
            return new RepeaterData(new Stack<ItemData>(), capturedData);
        }

        var nonTerminal = (GeneralLRParser<Terminal, ItemData, TContext>.NonTerminalSymbol)symbols.Last();
        var repeaterData = (RepeaterData)nonTerminal.Result;
        var stack = (Stack<ItemData>)repeaterData.Items;

        var relevantSymbols = GetNonTerminalsAndTopLevelTerminals(symbols.SkipLast(1));
        foreach (var symbol in relevantSymbols.Reverse()) {
            stack.Push(GetSymbolData(symbol));
        }

        for (var i = 0; i < symbols.Count - 1; i++) {
            var symbol = symbols[i];
            if (symbol.CaptureName == null) {
                continue;
            }

            var itemData = repeaterData.CapturedData![symbol.CaptureName];
            var itemStack = (Stack<ItemData>)itemData.Items;
            var relevantSubSymbols = GetNonTerminalsAndTopLevelTerminals(symbols.SkipLast(1).TakeLast(symbols.Count - 1 - i));
            foreach (var subSymbol in relevantSubSymbols.Reverse()) {
                itemStack.Push(GetSymbolData(subSymbol));
            }
        }

        return repeaterData;
    }

    private static ItemData GetSymbolData(GeneralLRParser<Terminal, ItemData, TContext>.Symbol symbol) => symbol switch {
        GeneralLRParser<Terminal, ItemData, TContext>.TerminalSymbol t =>
            new ModelItemData(((ObjectToken<TObject>)t.Terminal).Object),
        GeneralLRParser<Terminal, ItemData, TContext>.NonTerminalSymbol nt => nt.Result,
        _ => throw new NotImplementedException()
    };

    private static IEnumerable<GeneralLRParser<Terminal, ItemData, TContext>.Symbol> GetTopLevelSymbols(
        IEnumerable<GeneralLRParser<Terminal, ItemData, TContext>.Symbol> symbols) {
        var depth = 0;
        foreach (var symbol in symbols) {
            switch (symbol) {
                case GeneralLRParser<Terminal, ItemData, TContext>.TerminalSymbol terminal:
                    switch (terminal.Terminal.Kind) {
                        case TerminalKind.Open:
                            if (depth == 0) {
                                yield return symbol;
                            }
                            depth++;
                            break;

                        case TerminalKind.Close:
                            depth--;
                            break;
                    }
                    break;

                case GeneralLRParser<Terminal, ItemData, TContext>.NonTerminalSymbol:
                    if (depth == 0) {
                        yield return symbol;
                    }
                    break;
            }
        }
    }

    private static IEnumerable<GeneralLRParser<Terminal, ItemData, TContext>.Symbol> GetNonTerminalsAndTopLevelTerminals(
        IEnumerable<GeneralLRParser<Terminal, ItemData, TContext>.Symbol> symbols) {
        var depth = 0;
        foreach (var symbol in symbols) {
            switch (symbol) {
                case GeneralLRParser<Terminal, ItemData, TContext>.TerminalSymbol terminal:
                    switch (terminal.Terminal.Kind) {
                        case TerminalKind.Open:
                            if (depth == 0) {
                                yield return symbol;
                            }
                            depth++;
                            break;

                        case TerminalKind.Close:
                            depth--;
                            if (depth < 0) {
                                yield break;
                            }
                            break;
                    }
                    break;

                case GeneralLRParser<Terminal, ItemData, TContext>.NonTerminalSymbol:
                    yield return symbol;
                    break;
            }
        }
    }

    private static Lazy<Dictionary<string, ItemData>> GetCapturedData(
        IEnumerable<GeneralLRParser<Terminal, ItemData, TContext>.Symbol> symbols) => new(() => {
            var result = new Dictionary<string, ItemData>();
            foreach (var symbol in symbols) {
                if (symbol is GeneralLRParser<Terminal, ItemData, TContext>.NonTerminalSymbol nt &&
                    nt.Result is RepeaterData repeaterData &&
                    repeaterData.CapturedData != null) {
                    foreach (var kvp in repeaterData.CapturedData) {
                        result[kvp.Key] = kvp.Value;
                    }
                } else if (symbol.CaptureName != null) {
                    result[symbol.CaptureName] = GetSymbolData(symbol);
                }
            }

            return result;
        });

    private GeneralLRParser<Terminal, ItemData, TContext>.RuleHandler CreateDefaultPatternHandler(Pattern pattern) =>
        (context, rule, symbols) => {
            var topLevelSymbols = GetTopLevelSymbols(symbols);
            var data = topLevelSymbols.Select(GetSymbolData).ToArray();
            var capturedData = GetCapturedData(symbols);

            if (pattern.PassThrough) {
                return data.First();
            }

            ItemData GetItemData(string itemCaptureName) {
                if (!capturedData.Value.TryGetValue(itemCaptureName, out var data)) {
                    throw new InvalidOperationException($"Pattern {pattern.Name} does not define a capture group named {itemCaptureName}");
                }
                return data;
            }

            var ruleResult = (pattern.Handler ?? defaultPatternHandler)(pattern, context, data, GetItemData);
            if (ruleResult == null) {
                return null;
            }

            return new MatchedPatternData(pattern, ruleResult);
        };

    public PatternRecognizer(
        string startSymbol, 
        IEnumerable<Pattern> patterns, 
        ITokenizer<TObject> tokenizer,
        PatternHandler defaultPatternHandler,
        ILogger? logger) {
        this.tokenizer = tokenizer;
        this.defaultPatternHandler = defaultPatternHandler;
        parser = ParserGenerator<Terminal, ItemData, TContext>.GenerateParser(
            startSymbol, 
            Convert(patterns, CreateDefaultPatternHandler), 
            Eof, 
            logger);
    }

    public IEnumerable<TResult>? Parse(IEnumerable<TObject> objects, TContext context, CancellationToken cancellationToken) =>
        parser.Parse(tokenizer.Tokenize(objects), context, cancellationToken) switch {
            null => null,
            MatchedPatternData r => new[] { r.Result },
            ListData l => l.Items.OfType<MatchedPatternData>().Select(n => n.Result).ToList(),
            _ => throw new NotImplementedException()
        };

    private static ParserGenerator<Terminal, ItemData, TContext>.TerminalRulePart AsRulePart(
        ModelItem item, TerminalKind terminalKind) =>
        new(new Terminal(item.Id, terminalKind),
            terminalKind == TerminalKind.Close ? null : item.CaptureName,
            terminalKind == TerminalKind.Close ? null : item.Condition?.AsParserGenerationCondition);

    private static ParserGenerator<Terminal, ItemData, TContext>.NonTerminalRulePart AsRulePart(
        NonTerminalItem item) => new(item.Id, item.CaptureName);

    private static ParserGenerator<Terminal, ItemData, TContext>.NonTerminalRulePart AsRulePart(
        string nonTerminal, string? captureName) => new(nonTerminal, captureName);

    // used by tests
    internal static IEnumerable<string> Convert(IEnumerable<Pattern> patterns) =>
        Convert(patterns,
                getRuleHandler: p => ((c, r, s) => new ListData(Array.Empty<ItemData>()))).
        Select(r => r.ToString());

    private static IEnumerable<ParserGenerator<Terminal, ItemData, TContext>.Rule> Convert(
        IEnumerable<Pattern> patterns,
        CreatePatternHandler getRuleHandler) {

        var repeaters = new Dictionary<PatternItem, string>();
        return patterns.SelectMany(p => Convert(p, getRuleHandler, repeaters));
    }

    private static IEnumerable<ParserGenerator<Terminal, ItemData, TContext>.Rule> Convert(
        Pattern pattern,
        CreatePatternHandler createPatternHandler,
        Dictionary<PatternItem, string> repeaters) {

        // sanity checks
        if (pattern.PassThrough) {
            if (pattern.Handler != null) {
                throw new ArgumentException($"Pattern {pattern.Name} cannot define a {nameof(Pattern.Handler)} because {nameof(Pattern.PassThrough)} is true");
            }

            if (pattern.Items.Length != 1) {
                throw new ArgumentException($"Pattern {pattern.Name} must contain exactly one item because {nameof(Pattern.PassThrough)} is true");
            }
        }

        var result = new List<ParserGenerator<Terminal, ItemData, TContext>.Rule>();

        string GetOrCreateRepeater(PatternItem item) {
            if (!repeaters.TryGetValue(item, out var repeaterName)) {
                repeaterName = $"{RepeaterNamePrefix}{repeaters.Count}";
                repeaters.Add(item, repeaterName);

                var repeaterNonTerminal = AsRulePart(repeaterName, captureName: null);
                var capturedNames = item.AllItems.Where(i => i.CaptureName != null).Select(i => i.CaptureName!).ToArray();

                result.Add(new ParserGenerator<Terminal, ItemData, TContext>.Rule(
                    repeaterNonTerminal,
                    Array.Empty<ParserGenerator<Terminal, ItemData, TContext>.IRulePart>(),
                    (c, r, s) => RepeaterRuleHandler(c, r, s, capturedNames)
                ));

                var ruleParts = new List<ParserGenerator<Terminal, ItemData, TContext>.IRulePart>();
                CreateRuleParts(pattern, item, GetOrCreateRepeater, ruleParts, processingRepeater: true);
                result.Add(new ParserGenerator<Terminal, ItemData, TContext>.Rule(
                    repeaterNonTerminal,
                    ruleParts.Concat(new[] { repeaterNonTerminal }).ToArray(),
                    (c, r, s) => RepeaterRuleHandler(c, r, s, capturedNames)
                ));
            }
            return repeaterName;
        }

        var ruleParts = new List<ParserGenerator<Terminal, ItemData, TContext>.IRulePart>();
        CreateRuleParts(pattern, pattern.Items, GetOrCreateRepeater, ruleParts);

        result.Add(new ParserGenerator<Terminal, ItemData, TContext>.Rule(
            AsRulePart(pattern.NonTerminal, captureName: null),
            ruleParts.ToArray(),
            createPatternHandler(pattern),
            pattern.PassThrough ? RulePriority.High : RulePriority.Normal,
            pattern.Name
        ));

        return result;
    }

    private static void CreateRuleParts(
        Pattern pattern,
        IEnumerable<PatternItem> items,
        Func<PatternItem, string> getOrCreateRepeater,
        List<ParserGenerator<Terminal, ItemData, TContext>.IRulePart> ruleParts) {
        foreach (var item in items) {
            CreateRuleParts(pattern, item, getOrCreateRepeater, ruleParts, processingRepeater: false);
        }
    }

    private static void CreateRuleParts(
        Pattern pattern,
        PatternItem item,
        Func<PatternItem, string> getOrCreateRepeater,
        List<ParserGenerator<Terminal, ItemData, TContext>.IRulePart> ruleParts,
        bool processingRepeater) {

        if (item.Quantifier == Quantifier.ZeroOrMore && !processingRepeater) {
            var repeaterName = getOrCreateRepeater(item);
            ruleParts.Add(AsRulePart(repeaterName, item.CaptureName));
            return;
        }

        switch (item) {
            case ModelItem t:
                ruleParts.Add(AsRulePart(t, TerminalKind.Open));
                foreach (var child in t.Children) {
                    CreateRuleParts(pattern, child, getOrCreateRepeater, ruleParts, processingRepeater: false);
                }
                ruleParts.Add(AsRulePart(t, TerminalKind.Close));
                break;

            case NonTerminalItem nt:
                ruleParts.Add(AsRulePart(nt));
                break;

            default:
                throw new NotImplementedException($"Missing case for type {item.GetType().Name}");
        }
    }

    public void Print(ILogger logger, bool fullDetails) =>
        parser.Print(logger, fullDetails);
}