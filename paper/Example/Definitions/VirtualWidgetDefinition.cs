using System.Collections.Generic;
using System.Linq;
using Virtualizer = OutSystems.Model.PatternRecognizer.PatternRecognizer<Example.NativeMetamodel.ModelObject, Example.VirtualMetamodel.ModelObject, Example.Definitions.VirtualizationContext>;

namespace Example.Definitions;

internal record VirtualWidgetDefinition(string Name, Property[] Properties, VirtualWidgetPattern[] Patterns, Virtualizer.PatternHandler? Handler = default) {
    public IEnumerable<ParserPattern> Convert() => Patterns.Select(p => p.Convert(this));
}
