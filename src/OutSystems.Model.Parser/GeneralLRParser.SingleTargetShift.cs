namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    private sealed record SingleTargetShift(int TargetState) : ParserAction {
        public override string ToString() => $"Shift({TargetState})";
    }
}