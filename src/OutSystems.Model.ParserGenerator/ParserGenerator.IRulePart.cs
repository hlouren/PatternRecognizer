namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

    public interface IRulePart {
        bool HasCondition { get; }
        public string? CaptureName { get; }
    }
}