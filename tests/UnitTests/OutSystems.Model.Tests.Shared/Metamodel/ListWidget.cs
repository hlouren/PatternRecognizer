using System.Collections.Generic;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface IListWidget : IWidget {
    IWidgetContent<IWidget> ListItem { get; }
}

internal class ListWidget : IListWidget {
    private readonly WidgetContent<IWidget> listItem = new("ListItem");
    IWidgetContent<IWidget> IListWidget.ListItem => listItem;

    IEnumerable<IBaseObject> IBaseObject.Children => new[] { listItem };

    public override string ToString() => $"<ListWidget> {listItem} </ListWidget>";
}
