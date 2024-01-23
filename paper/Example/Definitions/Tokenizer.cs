using Example.NativeMetamodel;
using OutSystems.Model.PatternRecognizer;
using System.Collections.Generic;

namespace Example.Definitions;

internal class Tokenizer : ITokenizer<ModelObject>
{

    public static readonly Tokenizer Instance = new Tokenizer();

    private Tokenizer() { }

    public IEnumerable<ModelObject> GetChildren(ModelObject item) => item.GetChildren();

    public string GetTypeName(ModelObject item) => item.GetType().Name;

    public (ModelObject Object, ModelObject ObjectForToken) Normalize(ModelObject item) => (item, item);
}
