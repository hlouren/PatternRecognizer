using System.Linq;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> {

	private sealed record RuleItem(int Number, NumberedRule Rule, int DotPosition) {

		public bool IsReadyToReduce => DotPosition == Rule.RightHandSide.Length;

		public override string ToString() {
			var parts =
				Rule.RightHandSide.Take(DotPosition).Select(p => p.ToString()).
				Concat(new[] { "." }).
				Concat(Rule.RightHandSide.Skip(DotPosition).Select(p => p.ToString()));
			return $"{Rule.LeftHandSide} -> {string.Join(" ", parts)}";
		}
	}
}