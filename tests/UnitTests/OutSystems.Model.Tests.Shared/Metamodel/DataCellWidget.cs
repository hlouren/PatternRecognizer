using System.Collections.Generic;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface IDataCellWidget : IWidget {
    IWidgetContent<IWidget> Content { get; }
}

internal class DataCellWidget : IDataCellWidget {
    private readonly WidgetContent<IWidget> content = new("Content");
    IWidgetContent<IWidget> IDataCellWidget.Content => content;

    IEnumerable<IBaseObject> IBaseObject.Children => new[] { content };

    public override string ToString() => $"<DataCellWidget> {content} </DataCellWidget>";
}
