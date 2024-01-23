using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OutSystems.Model.Parser.Classifiers;
using OutSystems.Model.Tests.Shared;
using GLRParser = OutSystems.Model.Parser.GeneralLRParser<OutSystems.Model.Tests.Shared.Token, OutSystems.Model.Tests.Shared.ParseTreeNode, int>;

namespace OutSystems.Model.Parser.Tests;

internal class GeneralLRParserTests : BaseParserTest {

    private const string OneOrMoreATestCaseName = "OneOrMoreA";

    internal class WrappedState {
        public GLRParser.State State { get; }

        public WrappedState() => State = new();
        private IClassifier<Token>[]? terminalClassifiers;

        internal WrappedState TerminalPredicates(string terminal, params Predicate<Token>[] predicates) {
            terminalClassifiers = predicates.Select(p => new BooleanClassifier<Token>(p)).ToArray();
            State.TerminalClassifiers(terminal.Tokenize(), terminalClassifiers);
            return this;
        }

        internal WrappedState TerminalClassifiers(string terminal, params IClassifier<Token>[] classifiers) {
            terminalClassifiers = classifiers;
            State.TerminalClassifiers(terminal.Tokenize(), terminalClassifiers);
            return this;
        }

        internal WrappedState Shift(string terminal, GLRParser.ISymbolTarget<Token> target) {
            State.Shift(terminal.Tokenize(), target);
            return this;
        }

        internal WrappedState Shift(string terminal, int targetState) =>
            Shift(terminal, new GLRParser.SingleTarget<Token>(targetState));

        internal WrappedState Shift(string terminal, Action<ConditionalTargetBuilder<Token>> setupConditionalTarget) {
            if (terminalClassifiers == null) {
                throw new InvalidOperationException("No classifiers or predicates have been setup");
            }

            var targetBuilder = new ConditionalTargetBuilder<Token>(terminalClassifiers);
            setupConditionalTarget(targetBuilder);
            return Shift(terminal, targetBuilder.Build());
        }

        internal WrappedState Jump(string nonTerminal, int targetState) {
            State.Jump(nonTerminal, new GLRParser.SingleTarget<GLRParser.NonTerminalSymbol>(targetState));
            return this;
        }

        internal WrappedState Jump(string nonTerminal, Action<ConditionalTargetBuilder<GLRParser.NonTerminalSymbol>> setupConditionalTarget, params Predicate<GLRParser.NonTerminalSymbol>[] predicates) {
            var classifiers = predicates.Select(p => new BooleanClassifier<GLRParser.NonTerminalSymbol>(p)).ToArray();
            var targetBuilder = new ConditionalTargetBuilder<GLRParser.NonTerminalSymbol>(classifiers);
            setupConditionalTarget(targetBuilder);
            State.Jump(nonTerminal, targetBuilder.Build(), classifiers);
            return this;
        }

        internal WrappedState Accept() {
            State.Accept(Eof);
            return this;
        }

        internal WrappedState Reduce(string terminal, int ruleNumber) {
            State.Reduce(terminal.Tokenize(), ruleNumber);
            return this;
        }
    }

    internal class StateBuilder {

        private readonly List<WrappedState> states = new();

        public StateBuilder State(Action<WrappedState> initializer) {
            var state = new WrappedState();
            initializer(state);
            states.Add(state);

            return this;
        }

        public GLRParser.State[] Build() => states.Select(s => s.State).ToArray();
    }

    internal class RuleBuilder {

        private readonly List<GLRParser.Rule> rules = new();

        public RuleBuilder Rule(string rule) {
            var parts = rule.Split(' ');
            Assert.IsTrue(parts.Length >= 3, $"Could not process rule: {rule}");
            Assert.AreEqual("->", parts[1]);

            var nonTerminal = parts[0];
            string? ruleName = null;
            var underscorePos = nonTerminal.IndexOf('_');
            if (underscorePos != -1) {
                ruleName = nonTerminal[(underscorePos + 1)..];
                nonTerminal = nonTerminal[..underscorePos];
            }

            var symbolCount = parts.Length - 2;
            var ruleNumber = rules.Count + 1;

            var captureNames = parts.Skip(2).Select(p => p.IndexOf("=") switch {
                <0 => null,
                int i => p[..i]
            }).ToArray();
            if (captureNames.All(c => c is null)) {
                captureNames = null;
            }

            rules.Add(new(nonTerminal, symbolCount, (c, r, s) => HandleRule(ruleName, r, s), captureNames, rule));

            return this;
        }

        public GLRParser.Rule[] Build() => rules.ToArray();
    }

    internal class ConditionalTargetBuilder<TSymbol> {

        private readonly IClassifier<TSymbol>[] classifiers;
        private readonly ConditionalTransitionTable<TSymbol, int> transitionTable;

        public ConditionalTargetBuilder(IClassifier<TSymbol>[] classifiers) {
            this.classifiers = classifiers;
            transitionTable = new(classifiers, -1);
        }

        public ConditionalTargetBuilder<TSymbol> Goto(int targetState, params object[] categoryValues) {
            var index = 0;
            var radix = 1;
            for (int i = 0; i < classifiers.Length; i++) {
                var classifier = classifiers[i];
                var value = (classifier, categoryValues[i]) switch {
                    (BooleanClassifier<TSymbol>, bool b) => b ? 1 : 0,
                    (SymbolPropertyClassifier<TSymbol, string> c, string s) => c.GetCategory(s),
                    _ => throw new ArgumentException("Invalid configuration")
                };
                index += value * radix;
                radix *= classifier.ValueCount;
            }
            transitionTable[index] = targetState; 
            return this;
        }

        public GLRParser.ConditionalTarget<TSymbol> Build() => new(transitionTable);
    }

    public class TestCase : ParserTestCase {

        public TestCase(string name, Action<StateBuilder> stateBuilderAction, Action<RuleBuilder> ruleBuilderAction) : base(
            name,
            tracer => {
                var stateBuilder = new StateBuilder();
                stateBuilderAction(stateBuilder);

                var ruleBuilder = new RuleBuilder();
                ruleBuilderAction(ruleBuilder);

                return new GLRParser(stateBuilder.Build(), ruleBuilder.Build(), Eof, tracer);
            }) { }
    }

    private static IEnumerable<ParserTestCase> TestCases {
        get {

            // S -> a
            yield return new TestCase("SingleA",
                    b => b. 
                        State(s => s.Shift("a", 2).Jump("S", 1)).
                        State(s => s.Accept()).
                        State(s => s.Reduce("$", 0)),
                    b => b.
                        Rule("S -> a")).
                OK("a").
                Error("a b", "", "a a");

            // S -> a
            yield return new TestCase("Capture_SingleA",
                    b => b.
                        State(s => s.Shift("a", 2).Jump("S", 1)).
                        State(s => s.Accept()).
                        State(s => s.Reduce("$", 0)),
                    b => b.
                        Rule("S -> c1=a")).
                OK("a").
                Error("a b", "", "a a");

            // BigBang -> S
            // S_R1 -> id?p
            // S_R2 -> id
            yield return new TestCase("Predicates2",
                    b => b.
                        State(s => s.
                            TerminalPredicates("id", p => p.Value == "xpto").
                            Shift("id", b => b.Goto(2, true)).
                            Shift("id", 3).
                            Jump("S", 1)).
                        State(s => s.Accept()).
                        State(s => s.Reduce("$", 1)).
                        State(s => s.Reduce("$", 2)),
                    b => b.
                        Rule("BigBang -> S").
                        Rule("S_R1 -> id?p").
                        Rule("S_R2 -> id")).
                OK("id", "id[xpto]", "id[hello]").
                Error("abcd");

            // BigBang -> S
            // S_R1 -> A?p
            // S_R2 -> A
            // A -> id
            yield return new TestCase("Predicates3",
                    b => b.
                        State(s => s.
                            Shift("id", 2).
                            Jump("S", 1).
                            Jump("A", b => b.Goto(3, true).Goto(4, false), p => p.Result.Children?.First().Type == "xpto")).
                        State(s => s.Accept()).
                        State(s => s.Reduce("$", 3)).
                        State(s => s.Reduce("$", 1).Reduce("$", 2)).
                        State(s => s.Reduce("$", 2)),
                    b => b.
                        Rule("BigBang -> S").
                        Rule("S_R1 -> A?p").
                        Rule("S_R2 -> A").
                        Rule("A -> id")).
                OK("id", "id[xpto]", "id[hello]").
                Error("abcd");

            // BigBang -> S
            // S_R1 -> id?p
            // S_R2 -> id
            yield return new TestCase("Predicates4",
                    b => b.
                        State(s => s.
                            TerminalClassifiers("id", new SymbolPropertyClassifier<Token, string>(TokenValueGetter).Store("xpto")).
                            Shift("id", b => b.Goto(2, "xpto")).
                            Shift("id", 3).
                            Jump("S", 1)).
                        State(s => s.Accept()).
                        State(s => s.Reduce("$", 1)).
                        State(s => s.Reduce("$", 2)),
                    b => b.
                        Rule("BigBang -> S").
                        Rule("S_R1 -> id?p").
                        Rule("S_R2 -> id")).
                OK("id[aaa]", "id[xpto]", "id[hello]").
                Error("abcd");

            // S -> a S | a
            yield return new TestCase(OneOrMoreATestCaseName,
                    b => b.
                        State(s => s.Shift("a", 2).Jump("S", 1)).
                        State(s => s.Accept()).
                        State(s => s.Reduce("$", 1).Shift("a", 2).Jump("S", 3)).
                        State(s => s.Reduce("$", 0)),
                    b => b.
                        Rule("S -> S a").
                        Rule("S -> a")).
                OK("a", "a a", "a a a").
                Error("a b", "", "b a");

            // 0. E -> E + T
            // 1. E -> T
            // 2. T -> T * F
            // 3. T -> F
            // 4. F -> ( E )
            // 5. F -> n
            yield return new TestCase("MathExpr",
                    b => b.
                        /*0*/ State(s => s.Shift("n", 5).Shift("(", 4).Jump("E", 1).Jump("T", 2).Jump("F", 3)).
                        /*1*/ State(s => s.Shift("+", 6).Accept()).
                        /*2*/ State(s => s.Reduce("+", 1).Shift("*", 7).Reduce(")", 1).Reduce("$", 1)).
                        /*3*/ State(s => s.Reduce("+", 3).Reduce("*", 3).Reduce(")", 3).Reduce("$", 3)).
                        /*4*/ State(s => s.Shift("n", 5).Shift("(", 4).Jump("E", 8).Jump("T", 2).Jump("F", 3)).
                        /*5*/ State(s => s.Reduce("+", 5).Reduce("*", 5).Reduce(")", 5).Reduce("$", 5)).
                        /*6*/ State(s => s.Shift("n", 5).Shift("(", 4).Jump("T", 9).Jump("F", 3)).
                        /*7*/ State(s => s.Shift("n", 5).Shift("(", 4).Jump("F", 10)).
                        /*8*/ State(s => s.Shift("+", 6).Shift(")", 11)).
                        /*9*/ State(s => s.Reduce("+", 0).Shift("*", 7).Reduce(")", 0).Reduce("$", 0)).
                        /*10*/ State(s => s.Reduce("+", 2).Reduce("*", 2).Reduce(")", 2).Reduce("$", 2)).
                        /*11*/ State(s => s.Reduce("+", 4).Reduce("*", 4).Reduce(")", 4).Reduce("$", 4)),
                    b => b.
                        Rule("E -> E + T").
                        Rule("E -> T").
                        Rule("T -> T * F").
                        Rule("T -> F").
                        Rule("F -> ( E )").
                        Rule("F -> n")).
                OK("n", "( n )", "n + n", "n + n * n", "( n + n ) * n").
                Error("n n", "n )");
        }
    }

    private static IEnumerable<TestCaseWithInput> TestCasesWithInputs => GetTestCasesWithInputs(TestCases);

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void PrintParser(TestCase testCase) {
        var parser = testCase.CreateParser(null);
        using var tracer = new Tracer();
        parser.Print(tracer);
        TestUtils.Asserts.AssertTestResult(tracer.ToString(), "txt", testCaseName: testCase.Name);
    }

    [Test]
    [TestCaseSource(nameof(TestCasesWithInputs))]
    public void RunParser(TestCaseWithInput testCase) => base.RunParser(testCase);

    [Test]
    [TestCaseSource(nameof(TestCasesWithInputs))]
    public void TraceParser(TestCaseWithInput testCase) => base.TraceParser(testCase);

    [Test, Timeout(10000)]
    public void ParserCanBeCancelled() {
        var testCase = TestCases.Single(t => t.Name == OneOrMoreATestCaseName);
        var parser = testCase.CreateParser(null);
        Assert.IsNotNull(parser);

        static IEnumerable<Token> InfiniteSequence() {
            while (true) {
                yield return new("a", null);
            }
        }

        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(TimeSpan.FromMilliseconds(10));
        Assert.Throws<OperationCanceledException>(() => parser.Parse(InfiniteSequence(), 0, tokenSource.Token));
    }
}
