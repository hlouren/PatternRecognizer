using System.Collections.Generic;

namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public abstract record ParserAction() {

        public virtual IEnumerable<ParserAction> GetAvailableActions(TTerminal token) {
            yield return this;
        }
    };
}