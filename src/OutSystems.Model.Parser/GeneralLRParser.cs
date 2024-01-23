using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using OutSystems.Model.Parser.Collections;

namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> where TTerminal : notnull, IEquatable<TTerminal> {

    private readonly State[] states;
    private readonly Rule[] rules;
    private readonly TTerminal eofTerminal;
    private readonly ILogger? logger = null;

    // TODO: get rid of eofTerminal parameter
    public GeneralLRParser(State[] states, Rule[] rules, TTerminal eofTerminal, ILogger? logger = null) {
        this.states = states;
        this.rules = rules;
        this.eofTerminal = eofTerminal;
        this.logger = logger;
    }

    public TResult? Parse(IEnumerable<TTerminal> tokens, TContext context, CancellationToken cancellationToken) {
        logger.WriteLine();
        logger.WriteLine("Starting parser on token sequence:");
        logger.WriteLine(string.Join(" ", tokens));

        var tokenStream = new LazyList<TTerminal>(tokens);

        var initialStack = new TreeStructuredStack<Symbol>(new StateSymbol(0));

        var configurations = new Stack<Configuration>();
        configurations.Push(new(this, tokenStream));

        var first = true;
        while (configurations.Any()) {
            var configuration = configurations.Pop();
            if (first) {
                first = false;
            } else {
                logger.WriteLine();
                logger.WriteLine("Backtracking...");
            }

            while (true) {
                if (cancellationToken.IsCancellationRequested) {
                    throw new OperationCanceledException("Cancellation was request");
                }

                var token = configuration.Token;
                var state = configuration.GetTopState();

                if (logger != null) {
                    logger.WriteLine();
                    logger.WriteLine($"{(configurations.Count > 0 ? $"configurationCount={configurations.Count} " : "")}{configuration}");
                }

                ParserAction action;
                if (configuration.Action == null) {
                    if (!state.TryGetActions(token, out var actions)) {
                        logger.WriteLine($"Unexpected token {token}");
                        break;
                    }

                    // if there are multiple actions then we must explore each of them. We'll explore the first
                    // action in the first place, and store the others in the configuration stack
                    var isFirstAction = true;
                    foreach (var altAction in actions.Skip(1)) {
                        if (isFirstAction) {
                            if (logger != null) {
                                logger.WriteLine($"Multiple actions found: {string.Join(", ", actions)}");
                            }
                            isFirstAction = false;
                        }
                        configurations.Push(configuration.Fork(altAction));
                    }
                    action = actions.First();
                } else {
                    action = configuration.Action;
                    configuration.Action = null;
                }

                logger.WriteLine($"Action {action}");
                if (action is SingleTargetShift s) {
                    configuration.PushTerminal(token);
                    configuration.PushState(s.TargetState);
                    configuration.MoveNext();
                } else if (action is Reduce r) {
                    var rule = rules[r.RuleNumber];
                    logger.WriteLine($"Reducing {rule.SymbolCount} symbols with rule {rule}");
                    var symbols = configuration.Pop(rule.SymbolCount);
                    for (var  i = 0; i < symbols.Count; i++) {
                        symbols[i].CaptureName = rule.CaptureNames?[i];
                    }

                    var result = rule.Handle(context, rule, symbols);
                    if (result == null) {
                        logger.WriteLine("Rule rejected reduce");
                        break;
                    }

                    var nonTerminalSymbol = new NonTerminalSymbol(rule.NonTerminal, result);

                    if (!configuration.GetTopState().TryGetGoto(nonTerminalSymbol, out var targetState)) {
                        logger.WriteLine("Transition not found or inactive");
                        break;
                    }
                    configuration.Push(nonTerminalSymbol);
                    configuration.PushState(targetState);
                } else if (action is Accept) {
                    var topSymbol = configuration.Pop(1).FirstOrDefault();
                    if (topSymbol is NonTerminalSymbol nonTerminalSymbol) {
                        return nonTerminalSymbol.Result;
                    } else {
                        throw new InvalidOperationException($"Wrong parser state. Top symbol is {topSymbol}");
                    }
                } else {
                    throw new NotImplementedException($"Missing case for action {action}");
                }
            }
        }

        return default;
    }

    public void Print(ILogger logger, bool fullDetails = true) {
        if (logger == null) {
            return;
        }

        if (fullDetails) {
            var actionScope = logger.BeginScope("Actions");
            for (var i = 0; i < states.Length; i++) {
                var state = states[i];
                state.PrintActions(i, logger);
            }
            actionScope?.Dispose();
            logger.WriteLine();

            var gotoScope = logger.BeginScope("Goto");
            for (var i = 0; i < states.Length; i++) {
                var state = states[i];
                state.PrintGoto(i, logger);
            }
            gotoScope?.Dispose();
            logger.WriteLine();
        } else {
            logger.WriteLine($"{states.Length} states");
            logger.WriteLine(); ;
        }

        var ruleScope = logger.BeginScope("Rules");
        for (int i = 0; i < rules.Length; i++) {
            logger.WriteLine($"{i}: {rules[i]}");
        }
        ruleScope?.Dispose();

        IDisposable? conflictsScope = null;
        for (int i = 0; i < states.Length; i++) {
            var state = states[i];
            foreach (var (token, actions) in state.ConflictingActions) {
                if (conflictsScope == null) {
                    logger.WriteLine();
                    conflictsScope = logger.BeginScope("Conflicts");
                }
                logger.WriteLine($"({i}, {token}) -> {string.Join(", ", actions)}");
            }
        }
        conflictsScope?.Dispose();
    }
}
