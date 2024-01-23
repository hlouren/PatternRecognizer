namespace OutSystems.Model.PatternRecognizer;

public partial class PatternRecognizer<TObject, TResult, TContext> {

    public record MatchedPatternData(Pattern Pattern, TResult Result) : ItemData {
        public override string ToString() => Pattern.Name;
    }
}
