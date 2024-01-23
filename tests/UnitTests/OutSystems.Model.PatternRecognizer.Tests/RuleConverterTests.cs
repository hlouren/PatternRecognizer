using NUnit.Framework;
using OutSystems.Model.Tests.Shared;
using Virtualizer = OutSystems.Model.PatternRecognizer.PatternRecognizer<OutSystems.Model.Tests.Shared.Metamodel.IBaseObject, OutSystems.Model.PatternRecognizer.Tests.VirtualWidget, int>;

namespace OutSystems.Model.PatternRecognizer.Tests;

internal class RuleConverterTests : BasePatternRecognizerTest {

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void TestPatternConversion(TestCase testCase) {
        var rules = Virtualizer.Convert(testCase.Patterns);
        var result = string.Join("\n", rules);
        TestUtils.Asserts.AssertTestResult(result, "txt", testCaseName: testCase.Name);
    }
}
