using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OutSystems.Model.Tests.Shared;
using OutSystems.Model.Tests.Shared.Metamodel;
using Virtualizer = OutSystems.Model.PatternRecognizer.PatternRecognizer<OutSystems.Model.Tests.Shared.Metamodel.IBaseObject, OutSystems.Model.PatternRecognizer.Tests.VirtualWidget, int>;

namespace OutSystems.Model.PatternRecognizer.Tests;

internal class PatternTests : BasePatternRecognizerTest {

    public record ParserTestCase(string Name, IScreen Screen, string StartSymbol, Lazy<Virtualizer.Pattern[]> Patterns) {
        public override string ToString() => Name;
    }

    private static IEnumerable<ParserTestCase> ParserTestCases =>
        TestCases.Where(c => c.ScreenConstructors != null).
        SelectMany(c => c.ScreenConstructors!().Select((s, i) => 
            new ParserTestCase($"{c.Name}.{i+1}", s, c.StartSymbol, new(() => c.Patterns))));

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void CheckDefinition(TestCase testCase) {
        var info = testCase.Patterns.Stringify();
        TestUtils.Asserts.AssertTestResult(info, "txt", testCaseName: testCase.Name);
    }

    [Test]
    [TestCaseSource(nameof(ParserTestCases))]
    public void Parse(ParserTestCase testCase) {
        var tracer = new Tracer();
        var parser = new Virtualizer(
            testCase.StartSymbol, testCase.Patterns.Value, Tokenizer.Instance, CreateVirtualWidget, tracer);

        try {
            tracer.WriteLine();
            tracer.WriteLine("*********************************************");
            tracer.WriteLine($"Processing screen");

            var result = parser.Parse(testCase.Screen.Widgets, 0, CancellationToken.None);
            var logFile = TestUtils.Files.SaveObtainedResult(tracer.ToString(), "log.txt", testCaseName: testCase.Name);
            Assert.IsNotNull(result);
            TestUtils.Asserts.AssertTestResult(result!.Stringify(), "txt", testCaseName: testCase.Name);

            File.Delete(logFile);
        } catch {
            TestUtils.Files.SaveObtainedResult(tracer.ToString(), "log.txt", testCaseName: testCase.Name);
            throw;
        }
    }

    protected static VirtualWidget? CreateVirtualWidget(
        Virtualizer.Pattern pattern,
        int context,
        Virtualizer.ItemData[] topLevelItemsData,
        Func<string, Virtualizer.ItemData> getItemData) =>
        CreateVirtualWidget(pattern, topLevelItemsData, getItemData, Array.Empty<string>());
}
