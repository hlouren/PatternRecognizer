using System;
using OutSystems.Model.Parser;

namespace OutSystems.Model.ParserGenerator;

public partial class ParserGenerator<TTerminal, TResult, TContext> where TTerminal : notnull, IEquatable<TTerminal> {

    private record ItemSetInfo(GeneralLRParser<TTerminal, TResult, TContext>.State LRState, int Number, RuleItemSet CanonicalSet);
}