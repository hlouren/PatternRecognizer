using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface IScreen : IBaseObject {
    string Name { get; set; }

    IEnumerable<IWidget> Widgets { get; }
    T CreateWidget<T>() where T : IWidget;
}

internal class Screen : IScreen {
    private string name = "";
    string IScreen.Name {
        get => name;
        set => name = value;
    }

    private readonly List<IWidget> widgets = new();
    IEnumerable<IWidget> IScreen.Widgets => widgets;

    T IScreen.CreateWidget<T>() {
        var widget = ModelFactory.Create<T>();
        widgets.Add(widget);
        return widget;
    }

    IEnumerable<IBaseObject> IBaseObject.Children => widgets;
}
