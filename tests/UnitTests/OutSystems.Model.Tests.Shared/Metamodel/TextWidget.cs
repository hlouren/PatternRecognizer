using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface ITextWidget : IWidget {
    string Text { get; set; }
}

internal class TextWidget : ITextWidget {
    private string text = "";
    string ITextWidget.Text {
        get => text;
        set => text = value;
    }

    IEnumerable<IBaseObject> IBaseObject.Children => Enumerable.Empty<IBaseObject>();

    public override string ToString() => $"<TextWidget Text={text} />";
}
