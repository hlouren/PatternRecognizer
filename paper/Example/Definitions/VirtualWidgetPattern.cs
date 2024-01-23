using OutSystems.Model.PatternRecognizer.PatternItems;
using System.CodeDom.Compiler;
using System.Linq;
using static Example.Definitions.Config;

namespace Example.Definitions;

internal record VirtualWidgetPattern(int Id, PropertyBinding[] Bindings, PatternItem[] Items) {

    public ParserPattern Convert(VirtualWidgetDefinition widget) =>
        new(ParserPatternKind.VirtualWidget, $"{widget.Name}.{Id}", VirtualWidgetNonTerminal, Items, widget.Handler);

    public void Print(IndentedTextWriter writer) {
        writer.Indent++;

        // not entirely correct, but currently we've no examples with nonterminals
        foreach (var item in Items.Cast<VirtualWidgetPatternItem>()) {
            item.Print(writer);
        }

        writer.Indent--;
    }
}
