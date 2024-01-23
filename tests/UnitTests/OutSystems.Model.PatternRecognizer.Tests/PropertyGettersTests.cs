using NUnit.Framework;
using OutSystems.Model.PatternRecognizer.PatternItems;
using OutSystems.Model.PatternRecognizer.Tokens;
using OutSystems.Model.Tests.Shared.Metamodel;
using PropertyGetter = OutSystems.Model.Parser.Classifiers.PropertyGetter<OutSystems.Model.PatternRecognizer.Tokens.Terminal, string>;

namespace OutSystems.Model.PatternRecognizer.Tests;

internal class PropertyGettersTests {
    [Test]
    public void CanCreatePropertyGetter() {

        var screen = ModelFactory.CreateScreen();
        screen.Name = "MyScreen";

        var listWidget = screen.CreateWidget<IListWidget>();

        var textWidget = listWidget.ListItem.CreateWidget<ITextWidget>();
        textWidget.Text = "My text";

        PropertyGetter? stringNameGetter = null, textValueGetter = null;

        Assert.DoesNotThrow(() => stringNameGetter = PropertyGetters<string>.Get(typeof(IScreen), nameof(IScreen.Name)));
        Assert.IsNotNull(stringNameGetter);

        Assert.DoesNotThrow(() => textValueGetter = PropertyGetters<string>.Get<ITextWidget>(t => t.Text, nameof(ITextWidget.Text)));
        Assert.IsNotNull(textValueGetter);

        var screenTerminal = new ObjectToken<IBaseObject>("type", TerminalKind.Open, screen, screen);
        var textTerminal = new ObjectToken<IBaseObject>("type", TerminalKind.Open, textWidget, textWidget);

        Assert.DoesNotThrow(() => stringNameGetter!.GetValue(screenTerminal));
        Assert.AreEqual("MyScreen", stringNameGetter!.GetValue(screenTerminal));

        Assert.DoesNotThrow(() => textValueGetter!.GetValue(textTerminal));
        Assert.AreEqual("My text", textValueGetter!.GetValue(textTerminal));
    }
}
