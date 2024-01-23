using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface IWidgetContent : IBaseObject {
    string Name { get; }
}

public interface IWidgetContent<TContent> : IWidgetContent where TContent : IWidget {
    IEnumerable<TContent> Widgets { get; }
    T CreateWidget<T>() where T : TContent;
}

internal class WidgetContent<TContent> : IWidgetContent<TContent> where TContent : IWidget {
    private readonly string name;
    string IWidgetContent.Name => name;

    private readonly List<TContent> widgets = new();
    IEnumerable<TContent> IWidgetContent<TContent>.Widgets => widgets;

    public WidgetContent(string name) => this.name = name;

    T IWidgetContent<TContent>.CreateWidget<T>() {
        var widget = ModelFactory.Create<T>();
        widgets.Add(widget);
        return widget;
    }

    IEnumerable<IBaseObject> IBaseObject.Children => widgets.Cast<IBaseObject>();

    public override string ToString() => $"<{name}> {string.Join(" ", widgets)} </{name}>";
}
