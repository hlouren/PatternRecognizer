using System.Collections.Generic;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface ITableWidget : IWidget {
    IWidgetContent<IHeaderCellWidget> HeaderRow { get; }
    IWidgetContent<IDataCellWidget> DataRow { get; }
}

internal class TableWidget : ITableWidget {
    private readonly WidgetContent<IHeaderCellWidget> headerRow = new("HeaderRow");
    IWidgetContent<IHeaderCellWidget> ITableWidget.HeaderRow => headerRow;

    private readonly WidgetContent<IDataCellWidget> dataRow = new("DataRow");
    IWidgetContent<IDataCellWidget> ITableWidget.DataRow => dataRow;

    IEnumerable<IBaseObject> IBaseObject.Children => new IBaseObject[] { headerRow, dataRow };

    public override string ToString() => $"<TableWidget> {headerRow} {dataRow} </TableWidget>";
}
