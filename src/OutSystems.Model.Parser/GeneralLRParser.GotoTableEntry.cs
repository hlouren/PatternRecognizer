using System.Linq;
using OutSystems.Model.Parser.Classifiers;

namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public record GotoTableEntry(ISymbolTarget<NonTerminalSymbol> Target, IClassifier<NonTerminalSymbol>[] Classifiers) {

        public override string ToString() => Target.ToString() +
            (Classifiers.Any() ? $" ; {Classifiers.Length} classifiers" : "");
    }
}