using System.Collections.Generic;

namespace OutSystems.Model.PatternRecognizer;

public partial class PatternRecognizer<TObject, TResult, TContext> {

    private record RepeaterData(IEnumerable<ItemData> Items, Dictionary<string, ListData>? CapturedData) : 
        ListData(Items) {
        public override string ToString() => base.ToString();
    }
}
