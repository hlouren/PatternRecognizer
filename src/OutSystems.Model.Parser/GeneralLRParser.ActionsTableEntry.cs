using System.Collections.Generic;
using System.Linq;
using OutSystems.Model.Parser.Classifiers;

namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public record ActionsTableEntry(List<ParserAction> Actions, IClassifier<TTerminal>[] Classifiers) {

        internal void SortActions(IComparer<ParserAction> comparer) =>
            Actions.Sort(comparer);

        public override string ToString() =>
            string.Join(", ", Actions) +
            (Classifiers.Any() ? $" ; {Classifiers.Length} classifiers" : "");
    }
}