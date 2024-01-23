using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface IIconWidget : IWidget {
}

internal class IconWidget : IIconWidget {
    IEnumerable<IBaseObject> IBaseObject.Children => Enumerable.Empty<IBaseObject>();

    public override string ToString() => "<IconWidget/>";
}
