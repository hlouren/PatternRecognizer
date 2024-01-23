using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public delegate TResult? RuleHandler(TContext context, Rule rule, List<Symbol> symbols);

    public sealed record Rule(
        string NonTerminal,
        int SymbolCount,
        RuleHandler Handle,
        string?[]? CaptureNames = null,
        string? Description = null,
        string? Name = null) {
        public override string ToString() =>
            (Name == null ? "" : $"[{Name}]    ") +
            (Description ?? $"{NonTerminal} -> " +
                (CaptureNames == null ?
                    $"#{SymbolCount}" :
                    string.Join(" ", CaptureNames.Select((c, i) => c ?? $"c{i}"))));
    }
}