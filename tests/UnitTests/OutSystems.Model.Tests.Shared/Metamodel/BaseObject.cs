using System.Collections.Generic;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public interface IBaseObject {

    IEnumerable<IBaseObject> Children { get; }
}
