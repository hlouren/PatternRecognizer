namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    public abstract record Symbol() {

        public string? CaptureName { get; set; }
    }
}