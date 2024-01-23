using System.Collections.Generic;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface IIfWidget : IWidget {
    IWidgetContent<IWidget> TrueBranch { get; }
    IWidgetContent<IWidget> FalseBranch { get; }
}

internal class IfWidget : IIfWidget {
    private readonly WidgetContent<IWidget> trueBranch = new("TrueBranch");
    IWidgetContent<IWidget> IIfWidget.TrueBranch => trueBranch;

    private readonly WidgetContent<IWidget> falseBranch = new("FalseBranch");
    IWidgetContent<IWidget> IIfWidget.FalseBranch => falseBranch;

    IEnumerable<IBaseObject> IBaseObject.Children => new[] { trueBranch, falseBranch };

    public override string ToString() => $"<IfWidget> {trueBranch} {falseBranch} </IfWidget>";
}
