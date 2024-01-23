using System.Collections.Generic;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface IHeaderCellWidget : IWidget {
    IWidgetContent<IWidget> Content { get; }
}

internal class HeaderCellWidget : IHeaderCellWidget {
    private readonly WidgetContent<IWidget> content = new("Content");
    IWidgetContent<IWidget> IHeaderCellWidget.Content => content;

    IEnumerable<IBaseObject> IBaseObject.Children => new[] { content };

    public override string ToString() => $"<HeaderCellWidget> {content} </HeaderCellWidget>";
}
