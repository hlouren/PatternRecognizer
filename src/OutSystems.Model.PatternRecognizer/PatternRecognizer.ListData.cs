using System.Collections.Generic;

namespace OutSystems.Model.PatternRecognizer;

public partial class PatternRecognizer<TObject, TResult, TContext> {

    public record ListData(IEnumerable<ItemData> Items) : ItemData {

        public override string ToString() => $"[ {string.Join(", ", Items)} ]";
    }
}
