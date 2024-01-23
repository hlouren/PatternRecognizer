using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;
using OutSystems.Model.Parser.Classifiers;
using GLRParser = OutSystems.Model.Parser.GeneralLRParser<OutSystems.Model.Tests.Shared.Token, OutSystems.Model.Tests.Shared.ParseTreeNode, int>;

namespace OutSystems.Model.Tests.Shared;

public abstract partial class BaseParserTest {

    protected static readonly Token Eof = new("$", null);
    protected static readonly PropertyGetter<Token, string> TokenValueGetter = new(t => t.Value ?? "", "Token.Value");

    public record TestCaseInput(string String, bool MustParse);

    public abstract class ParserTestCase {

        public string Name { get; }
        public Func<Tracer?, GLRParser> CreateParser { get; }
        public List<TestCaseInput> Inputs { get; } = new();

        public ParserTestCase(string name, Func<Tracer?, GLRParser> createParser) {
            Name = name;
            CreateParser = createParser;
        }

        public ParserTestCase OK(params string[] cases) {
            Inputs.AddRange(cases.Select(c => new TestCaseInput(c, true)));
            return this;
        }

        public ParserTestCase Error(params string[] cases) {
            Inputs.AddRange(cases.Select(c => new TestCaseInput(c, false)));
            return this;
        }

        public override string ToString() => Name;
    }

    protected void RunParser(TestCaseWithInput testCase, [CallerFilePath] string testFilePath = "", [CallerMemberName] string methodName = "") {
        var parser = testCase.TestCase.CreateParser(null);
        Assert.IsNotNull(parser);

        var result = parser.Parse(testCase.Input.String.Tokenize(' '), 0, CancellationToken.None);
        Assert.AreEqual(testCase.Input.MustParse, result != null);
        if (result != null) {
            var output = $"{testCase.Input.String}\n\n{result}";
            TestUtils.Asserts.AssertTestResult(output, $"{testCase.Index}.txt", testFilePath, methodName, testCaseName: testCase.TestCase.Name);
        }
    }

    protected void TraceParser(TestCaseWithInput testCase, [CallerFilePath] string testFilePath = "", [CallerMemberName] string methodName = "") {
        using var tracer = new Tracer();
        tracer.Disable();
        var parser = testCase.TestCase.CreateParser(tracer);
        tracer.Enable();
        Assert.IsNotNull(parser);

        tracer.WriteLine();
        tracer.WriteLine($"Testing \"{testCase.Input.String}\" -> {(testCase.Input.MustParse ? "OK" : "Error")}");
        tracer.WriteLine("------------------");
        tracer.IncrementIndent();

        var result = parser.Parse(testCase.Input.String.Tokenize(' '), 0, CancellationToken.None);
        if (result != null) {
            tracer.WriteLine();
            tracer.WriteLine("Parse result:");
            result.Print(tracer);
        }

        TestUtils.Asserts.AssertTestResult(tracer.ToString(), $"{testCase.Index}.txt", testFilePath, methodName, testCaseName: testCase.TestCase.Name);
        Assert.AreEqual(testCase.Input.MustParse, result != null);
    }

    protected static ParseTreeNode HandleRule(string? ruleName, GLRParser.Rule rule, IEnumerable<GLRParser.Symbol> symbols) =>
        new($"{ruleName ?? rule.NonTerminal}", CaptureName: null, symbols.Select(s => s switch {
            GLRParser.TerminalSymbol t => new(t.Terminal.Value ?? t.Terminal.Type, s.CaptureName),
            GLRParser.NonTerminalSymbol nt => nt.Result with {
                CaptureName = nt.CaptureName
            },
            _ => throw new InvalidOperationException($"Unexpected symbol {s}")
        }).ToArray());

    public record TestCaseWithInput(int Index, ParserTestCase TestCase, TestCaseInput Input) {
        public override string ToString() => $"{TestCase.Name}.{Index}";
    }

    protected static IEnumerable<TestCaseWithInput> GetTestCasesWithInputs(IEnumerable<ParserTestCase> testCases) =>
        testCases.SelectMany(c => c.Inputs.Select((input, i) => new TestCaseWithInput(i + 1, c, input)));
}