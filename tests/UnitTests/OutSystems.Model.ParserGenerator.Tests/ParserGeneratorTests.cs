using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OutSystems.Model.ParserGenerator.Collections;
using OutSystems.Model.ParserGenerator.Conditions;
using OutSystems.Model.Tests.Shared;
using GLRParser = OutSystems.Model.Parser.GeneralLRParser<OutSystems.Model.Tests.Shared.Token, OutSystems.Model.Tests.Shared.ParseTreeNode, int>;
using GLRParserGenerator = OutSystems.Model.ParserGenerator.ParserGenerator<OutSystems.Model.Tests.Shared.Token, OutSystems.Model.Tests.Shared.ParseTreeNode, int>;

namespace OutSystems.Model.ParserGeneator.Tests;

internal class ParserGeneratorTests : BaseParserTest {

    public class TestCase : ParserTestCase {

        public TestCase(string name, string grammar, 
            Dictionary<string, ICondition<Token>>? terminalSymbolConditions = null,
            Dictionary<string, ICondition<GLRParser.NonTerminalSymbol>>? nonTerminalSymbolConditions = null,
            Dictionary<int, GLRParser.RuleHandler>? rulesHandlers = null) : base(
            name,
            tracer => {
                var rules = grammar.Split('\n').Select(r => r.Trim()).Where(r => r != string.Empty && !r.StartsWith("//")).ToList();
                CollectionAssert.IsNotEmpty(rules);

                // the start symbol is by convention the left hand side of the 1st rule
                var parsedRules = rules.Select((r, i) => 
                    ParseRule(r, i, terminalSymbolConditions, nonTerminalSymbolConditions, rulesHandlers)).ToArray();
                var startSymbol = parsedRules.First().LeftHandSide.Symbol;
                return GLRParserGenerator.GenerateParser(startSymbol, parsedRules, Eof, tracer);
            }) { }

        private static GLRParserGenerator.Rule ParseRule(string rule, int index,
            Dictionary<string, ICondition<Token>>? terminalSymbolConditions,
            Dictionary<string, ICondition<GLRParser.NonTerminalSymbol>>? nonTerminalSymbolConditions,
            Dictionary<int, GLRParser.RuleHandler>? rulesHandlers) {
            var parts = rule.Split(' ');
            Assert.IsTrue(parts.Length >= 2, $"Could not process rule: {rule}");
            Assert.AreEqual("->", parts[1]);

            bool IsNonTerminal(string name) => char.IsLetter(name[0]) && char.IsUpper(name[0]);

            var nonTerminal = parts[0];
            Assert.IsTrue(IsNonTerminal(nonTerminal));

            string? ruleName = null;
            var underscorePos = nonTerminal.IndexOf('_');
            if (underscorePos != -1) {
                ruleName = nonTerminal[(underscorePos + 1)..];
                nonTerminal = nonTerminal[..underscorePos];
            }

            GLRParserGenerator.IRulePart ToRulePart(string name) {
                var conditionPos = name.IndexOf("?");
                string? conditionName = null;
                if (conditionPos > 0) {
                    conditionName = name[(conditionPos + 1)..];
                    name = name[..conditionPos];
                }

                var captureNamePos = name.IndexOf("=");
                string? captureName = null;
                if (captureNamePos > 0) {
                    captureName = name[0..captureNamePos];
                    name = name[(captureNamePos + 1)..];
                }

                if (IsNonTerminal(name)) {
                    var condition = conditionName == null ? null : nonTerminalSymbolConditions?[conditionName];
                    return new GLRParserGenerator.NonTerminalRulePart(name, captureName, condition);
                } else {
                    var condition = conditionName == null ? null : terminalSymbolConditions?[conditionName];
                    return new GLRParserGenerator.TerminalRulePart(name.Tokenize(), captureName, condition);
                }
            }

            if (rulesHandlers?.TryGetValue(index, out var ruleHandler) != true) {
                ruleHandler = (_, r, s) => HandleRule(ruleName, r, s);
            }

            if (ruleHandler == null) {
                throw new InvalidOperationException("Unable to get rule handler");
            }

            return new(
                new GLRParserGenerator.NonTerminalRulePart(nonTerminal),
                parts.Skip(2).Select(ToRulePart).ToArray(),
                ruleHandler);
        }
    }

    private static IEnumerable<ParserTestCase> TestCases {
        get {

            yield return new TestCase("SingleA",
                    "S -> a").
                OK("a").
                Error("a b", "", "a a");

            yield return new TestCase("Capture_SingleA",
                    "S -> c=a").
                OK("a").
                Error("a b", "", "a a");

            yield return new TestCase("Capture_OneOrTwoA",
                    """
                    S -> c=a
                    S -> c1=a c2=a
                    """).
                OK("a", "a a").
                Error("a b", "");

            yield return new TestCase("ZeroOrMoreA",
                    """
                    S -> a S
                    S ->
                    """).
                OK("", "a", "a a", "a a a").
                Error("a b", "b a");

            yield return new TestCase("ZeroOrMoreA_V2",
                    """
                    S -> S a
                    S ->
                    """).
                OK("", "a", "a a", "a a a").
                Error("a b", "b a");

            yield return new TestCase("OneOrMoreA",
                    """
                    S -> a S
                    S -> a
                    """).
                OK("a", "a a", "a a a").
                Error("a b", "", "b a");

            yield return new TestCase("OneOrMoreA_V2",
                    """
                    S -> S a
                    S -> a
                    """).
                OK("a", "a a", "a a a").
                Error("a b", "", "b a");

            yield return new TestCase("MathExpr",
                    """
                    E -> E + T
                    E -> T
                    T -> T * F
                    T -> F
                    F -> ( E )
                    F -> n
                    """).
                OK("n", "( n )", "n + n", "n + n * n", "( n + n ) * n").
                Error("n n", "n )");

            yield return new TestCase("Capture_MathExpr",
                    """
                    E -> e1=E + e2=T
                    E -> T
                    T -> e1=T * e2=F
                    T -> F
                    F -> ( E )
                    F -> n
                    """).
                OK("n", "( n )", "n + n", "n + n * n", "( n + n ) * n").
                Error("n n", "n )");

            yield return new TestCase("Questions",
                    """
                    Session -> Facts Question
                    Session -> ( Session ) Session
                    Facts -> Fact Facts
                    Facts ->
                    Fact -> ! s
                    Question -> ? s
                    """).
                OK("? s", "! s ? s", "! s ! s ? s", "( ? s ) ? s", "( ! s ? s ) ! s ? s").
                Error("! s", "? s ( ? s )");

            yield return new TestCase("Backtracking",
                    """
                    S -> a a A
                    S -> a a B
                    A -> A2?p
                    B -> B2
                    A2 -> a
                    B2 -> a
                    """,
                    nonTerminalSymbolConditions: new() {
                        // reject "A -> A2" so that "B -> B2" is accepted instead
                        // (reduce-reduce conflicts are by default resolved choosing the lowest numbered rule)
                        { "p", new BooleanCondition<GLRParser.NonTerminalSymbol>(s => false) }
                    }).
                OK("a a a").
                Error("a a", "b", "a a a a");

            yield return new TestCase("Predicates",
                    """
                    S_R.1 -> <b> <t/>?p </b>
                    S_R.2 -> <b> <t/> </b>
                    """,
                    terminalSymbolConditions: new() {
                        { "p", new PropertyValueCondition<Token, string>(TokenValueGetter, "xpto") }
                    }).
                OK("<b> <t/>[aaa] </b>", "<b> <t/>[xpto] </b>", "<b> <t/>[hello] </b>");

            yield return new TestCase("RulePredicates", 
                    """
                    S_R.1 -> <b> <t/> </b>
                    S_R.2 -> <b> <t/> </b>
                    """,
                    rulesHandlers: new() {
                        { 0, (c, r, s) => 
                            ((GLRParser.TerminalSymbol)s[1]).Terminal.Value == "xpto" ? 
                                HandleRule("R.1", r, s) : default
                        }
                    }).
                OK("<b> <t/> </b>", "<b> <t/>[xpto] </b>", "<b> <t/>[hello] </b>");

            yield return new TestCase("Predicates2",
                    """
                    S_R.1 -> id?p
                    S_R.2 -> id
                    """,
                    terminalSymbolConditions: new() {
                        { "p", new PropertyValueCondition<Token, string>(TokenValueGetter, "xpto") }
                    }).
                OK("id[aaa]", "id[xpto]", "id[hello]");

            yield return new TestCase("Predicates3",
                    """
                    S_R1 -> A?p
                    S_R2 -> A
                    A -> id
                    """,
                    nonTerminalSymbolConditions: new() {
                        { "p", new BooleanCondition<GLRParser.NonTerminalSymbol>(s => s.Result?.Children?.Single().Type == "xpto") }
                    }).
                OK("id", "id[xpto]", "id[hello]");

            yield return new TestCase("MultipleValues",
                    """
                    S_R.1 -> <b> <t/>?p1 </b>
                    S_R.2 -> <b> <t/>?p2 </b>
                    S_R.3 -> <b> <t/>?p3 </b>
                    S_R.4 -> <b> <t/> </b>
                    """,
                    terminalSymbolConditions: new() {
                        { "p1", new PropertyValueCondition<Token, string>(TokenValueGetter, "abc") },
                        { "p2", new PropertyValueCondition<Token, string>(TokenValueGetter, "def") },
                        { "p3", new PropertyValueCondition<Token, string>(TokenValueGetter, "ghi") }
                    }).
                OK("<b> <t/>[aaa] </b>", "<b> <t/>[abc] </b>", "<b> <t/>[def] </b>", "<b> <t/>[ghi] </b>");

            yield return new TestCase("MixedConditions",
                    """
                    S_R.1 -> <b> <t/>?p1 </b>
                    S_R.2 -> <b> <t/>?p2 </b>
                    S_R.3 -> <b> <t/>?p3 </b>
                    S_R.4 -> <b> <t/> </b>
                    """,
                    terminalSymbolConditions: new() {
                        { "p1", new PropertyValueCondition<Token, string>(TokenValueGetter, "abc") },
                        { "p2", new PropertyValueCondition<Token, string>(TokenValueGetter, "def") },
                        { "p3", new BooleanCondition<Token>(s => s.Value == "ghi") }
                    }).
                OK("<b> <t/>[aaa] </b>", "<b> <t/>[abc] </b>", "<b> <t/>[def] </b>", "<b> <t/>[ghi] </b>");

            // just a poc that we can use the parser to virtualize widgets
            // (in this case with some hand-coded rules; we will generate them from the virtual widgets
            // specifications for the real case)
            yield return new TestCase("Virtualization",
                    """
                    Screen -> VirtualWidgets
                    VirtualWidgets ->
                    VirtualWidgets -> VirtualWidget VirtualWidgets
                    VirtualWidget_ExprField.0 -> <text/> <expression/>
                    VirtualWidget_ExprField.1 -> <label> <text/> </label> <expression/>
                    VirtualWidget_InputField.0 -> <text/> <input/>
                    VirtualWidget_InputField.1 -> <label> <text/> </label> <input/>

                    // these two rules overlap on purpose, with the 2nd one being more specific
                    VirtualWidget_List.0 -> <list> VirtualWidgets </list>
                    VirtualWidget_List.1 -> <list> <text/> <expression/> </list>

                    // we need a 'any' to avoid specifying all native widgets...
                    VirtualWidget -> NativeWidget
                    NativeWidget -> <text/>
                    NativeWidget -> <label> VirtualWidgets </label>
                    NativeWidget -> <expression/>
                    NativeWidget -> <input/>
                    NativeWidget -> <list> VirtualWidgets </list>
                    """).
                OK(
                    "<text/>",
                    "<text/> <expression/>",
                    "<text/> <text/> <expression/>",
                    "<text/> <expression/> <expression/>",
                    "<text/> <expression/> <text/> <input/>",
                    "<label> <text/> </label> <expression/>",
                    "<label> <text/> <text/> </label> <expression/>",
                    "<list> <text/> <expression/> </list>",
                    "<list> </list>",
                    "<list> <text/> <expression/> <text/> <input/> </list>",
                    "<list> <text/> </list>",
                    "<list> <expression/> </list>");

            yield return new TestCase("VirtualLabels",
                    """
                    Screen -> VirtualWidgets

                    VirtualWidgets ->
                    VirtualWidgets -> VirtualWidget VirtualWidgets
                    VirtualWidget_ExprField.0 -> <text/> <expression/>
                    VirtualWidget_ExprField.1 -> <label> <text/> </label> <expression/>
                    VirtualWidget -> NativeWidget

                    // it would be helpful to use embed 'Headers' here...
                    VirtualWidget_Table.0 -> <table> <hr> TableHeaders </hr> <dr> TableDatas </dr> </table>
                    TableHeaders ->
                    TableHeaders -> TableHeader TableHeaders
                    TableHeader -> <hc> LabelWidget </hc>
                    TableDatas ->
                    TableDatas -> TableData TableDatas
                    TableData -> <dc> DataWidget </dc>

                    LabelWidgets ->
                    LabelWidgets -> LabelWidget LabelWidgets
                    LabelWidget_LText.0 -> <text/>
                    LabelWidget_Label.0 -> <label> <text/> </label>

                    DataWidgets ->
                    DataWidgets -> DataWidget DataWidgets
                    DataWidget_DataExpr.0 -> <expression/>

                    NativeWidgets ->
                    NativeWidgets -> NativeWidget NativeWidgets
                    NativeWidget -> <text/>
                    NativeWidget -> <expression/>
                    NativeWidget -> <label> VirtualWidgets </label>
                    NativeWidget -> <table> <hr> NativeTableHeaders </hr> <dr> NativeTableDatas </dr> </table>
                    NativeTableHeaders ->
                    NativeTableHeaders -> NativeTableHeader NativeTableHeaders
                    NativeTableHeader -> <hc> VirtualWidget </hc>
                    NativeTableDatas ->
                    NativeTableDatas -> NativeTableData NativeTableDatas
                    NativeTableData -> <dc> VirtualWidget </dc>
                    """).
                OK("<table> <hr> <hc> <text/> </hc> </hr> <dr> <dc> <expression/> </dc> </dr> </table>").
                OK("<table> <hr> <hc> <expression/> </hc> </hr> <dr> <dc> <text/> </dc> </dr> </table>").
                OK("<table> <hr> <hc> <label> <text/> </label> </hc> </hr> <dr> <dc> <expression/> </dc> </dr> </table>");
        }
    }

    private static IEnumerable<TestCaseWithInput> TestCasesWithInputs => GetTestCasesWithInputs(TestCases);

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void InspectParserCreation(TestCase testCase) {
        using var tracer = new Tracer();
        testCase.CreateParser(tracer);
        TestUtils.Asserts.AssertTestResult(tracer.ToString(), "txt", testCaseName: testCase.Name);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void PrintParser(TestCase testCase) {
        var parser = testCase.CreateParser(null);
        Assert.IsNotNull(parser);
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

    [Test]
    [TestCaseSource(nameof(TestCasesWithInputs))]
    public void Tokenize(TestCaseWithInput testCase) {
        var tokens = string.Join("\n", testCase.Input.String.Tokenize(' ').Select(t => t.ToString(includeValue: true)));
        TestUtils.Asserts.AssertTestResult(tokens, $"{testCase.Index}.txt", testCaseName: testCase.TestCase.Name);
    }

    [Test]
    [TestCase(1, 1)]
    [TestCase(31, 1)]
    [TestCase(32, 1)]
    [TestCase(63, 1)]
    [TestCase(64, 1)]
    [TestCase(65, 2)]
    [TestCase(100, 2)]
    [TestCase(128, 2)]
    [TestCase(129, 3)]
    public void TestFastBitArray(int bitCount, int expectedVectorCount) {
        Assert.AreEqual(expectedVectorCount, FastBitArray.CalculateVectorCount(bitCount));

        var a1 = new FastBitArray(bitCount);
        for (int i = 0; i < bitCount; i++) {
            Assert.IsFalse(a1[i]);

            a1[i] = true;
            Assert.IsTrue(a1[i]);
            for (int j = 0; j < bitCount; j++) {
                Assert.AreEqual(i == j, a1[j]);
            }

            a1[i] = false;
            Assert.IsFalse(a1[i]);
        }

        var a2 = new FastBitArray(bitCount);
        Assert.AreEqual(a1, a2);
        Assert.AreEqual(a1.GetHashCode(), a2.GetHashCode());

        a2[0] = true;
        Assert.AreNotEqual(a1, a2);
        Assert.AreNotEqual(a1.GetHashCode(), a2.GetHashCode());
    }
}
