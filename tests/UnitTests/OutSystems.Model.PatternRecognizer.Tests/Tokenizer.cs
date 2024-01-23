using OutSystems.Model.PatternRecognizer.Tokens;
using OutSystems.Model.Tests.Shared.Metamodel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.PatternRecognizer.Tests;

internal class Tokenizer : ITokenizer<IBaseObject> {

    public static readonly Tokenizer Instance = new Tokenizer();

    private Tokenizer() { }

    public string GetTypeName(IBaseObject obj) => obj switch {
        IWidgetContent c => c.Name,
        _ => $"I{obj.GetType().Name}",
    };

    public IEnumerable<IBaseObject> GetChildren(IBaseObject obj) => obj switch {
        IWidgetContent c => c.Children,
        IWidget => obj.Children.OfType<IWidgetContent>(),
        _ => throw new InvalidOperationException()
    };

    public (IBaseObject Object, IBaseObject ObjectForToken) Normalize(IBaseObject obj) => (obj, obj);
}
