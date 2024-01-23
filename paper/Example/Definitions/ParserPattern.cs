using OutSystems.Model.PatternRecognizer.PatternItems;
using Virtualizer = OutSystems.Model.PatternRecognizer.PatternRecognizer<Example.NativeMetamodel.ModelObject, Example.VirtualMetamodel.ModelObject, Example.Definitions.VirtualizationContext>;

namespace Example.Definitions;

internal class ParserPattern : Virtualizer.Pattern {

    public ParserPatternKind Kind { get; }

    public ParserPattern(
        ParserPatternKind kind,
        string name,
        string nonTerminal,
        PatternItem[] items,
        Virtualizer.PatternHandler? handler = null,
        bool passThrough = false) :
        base(name, nonTerminal, items, handler, passThrough) {
        Kind = kind;
    }
}
