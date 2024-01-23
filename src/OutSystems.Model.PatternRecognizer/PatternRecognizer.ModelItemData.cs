namespace OutSystems.Model.PatternRecognizer;

public partial class PatternRecognizer<TObject, TResult, TContext> {

    public record ModelItemData(TObject Object) : ItemData {
        public override string ToString() => Object?.ToString() ?? "";
    }
}