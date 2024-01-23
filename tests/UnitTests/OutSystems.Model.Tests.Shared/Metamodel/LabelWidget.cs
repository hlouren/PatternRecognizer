using System.Collections.Generic;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface ILabelWidget : IWidget {
    IWidgetContent<IWidget> Content { get; }
}

internal class LabelWidget : ILabelWidget {
    private readonly WidgetContent<IWidget> content = new("Content");
    IWidgetContent<IWidget> ILabelWidget.Content => content;

    IEnumerable<IBaseObject> IBaseObject.Children => new[] { content };

    public override string ToString() => $"<LabelWidget> {content} </LabelWidget>";
}
