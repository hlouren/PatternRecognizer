using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface IExpressionWidget : IWidget {
    string Value { get; set; }
}

internal class ExpressionWidget : IExpressionWidget {
    private string value = "";
    string IExpressionWidget.Value {
        get => value;
        set => this.value = value;
    }

    IEnumerable<IBaseObject> IBaseObject.Children => Enumerable.Empty<IBaseObject>();

    public override string ToString() => $"<ExpressionWidget Value={value} />";
}
