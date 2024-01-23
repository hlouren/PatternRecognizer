using System.CodeDom.Compiler;
using System.IO;
using NUnit.Framework;
using OutSystems.Model.PatternRecognizer.Tokens;
using OutSystems.Model.Tests.Shared;
using OutSystems.Model.Tests.Shared.Metamodel;

namespace OutSystems.Model.PatternRecognizer.Tests;

internal class TokenizerTests {

    private static string Tokenize(IScreen screen) {
        var tokens = Tokenizer.Instance.Tokenize(screen.Widgets);
        var sw = new StringWriter();
        var writer = new IndentedTextWriter(sw);

        foreach (var token in tokens) {
            switch (token.Kind) {
                case TerminalKind.Open:
                    writer.WriteLine(token);
                    writer.Indent++;
                    break;

                case TerminalKind.Close:
                    writer.Indent--;
                    writer.WriteLine(token);
                    break;

                default:
                    writer.WriteLine(token);
                    break;
            }
        }

        return sw.ToString();
    }

    [Test]
    public void TestTokenizer() {
        var screen = ModelFactory.CreateScreen();

        screen.CreateWidget<ITextWidget>();

        var list = screen.CreateWidget<IListWidget>();
        list.ListItem.CreateWidget<IExpressionWidget>();

        var table = screen.CreateWidget<ITableWidget>();
        var headerCell = table.HeaderRow.CreateWidget<IHeaderCellWidget>();
        headerCell.Content.CreateWidget<ITextWidget>();
        var dataCell = table.DataRow.CreateWidget<IDataCellWidget>();
        dataCell.Content.CreateWidget<IExpressionWidget>();

        TestUtils.Asserts.AssertTestResult(Tokenize(screen), "txt");
    }
}
