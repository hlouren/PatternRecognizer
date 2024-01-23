using System;
using System.Collections.Generic;
using OutSystems.Model.Parser.Collections;

namespace OutSystems.Model.Parser;

public partial class GeneralLRParser<TTerminal, TResult, TContext> {

    private class Configuration {

        private readonly GeneralLRParser<TTerminal, TResult, TContext> owner;
        private readonly LazyList<TTerminal> stream;
        private TreeStructuredStack<Symbol> stack;
        public ParserAction? Action { get; set; }

        public Configuration(GeneralLRParser<TTerminal, TResult, TContext> owner, LazyList<TTerminal> stream) {
            this.owner = owner;
            this.stream = stream;
            stack = new(new StateSymbol(0));
            StreamPosition = 0;
            Action = null;
        }

        private Configuration(
            GeneralLRParser<TTerminal, TResult, TContext> owner,
            LazyList<TTerminal> stream,
            TreeStructuredStack<Symbol> stack,
            int streamPosition,
            ParserAction? action) {

            this.owner = owner;
            this.stream = stream;
            this.stack = stack;
            StreamPosition = streamPosition;
            Action = action;
        }

        public Configuration Fork(ParserAction? action) => new(owner, stream, stack, StreamPosition, action);

        public void PushState(int state) => stack = stack.Push(new StateSymbol(state));
        public void PushTerminal(TTerminal terminal) => stack = stack.Push(new TerminalSymbol(terminal));
        public void Push(Symbol symbol) => stack = stack.Push(symbol);

        public List<Symbol> Pop(int symbolCount) {
            var result = new List<Symbol>();

            // stack contains alternating StateSymbol and (TerminalSymbol|NonTerminalSymbol)
            // and we're only interested in the Terminal and NonTerminal symbols
            while (result.Count < symbolCount) {
                if (stack == null) {
                    throw new InvalidOperationException();
                }

                if (stack.Data is not StateSymbol) {
                    result.Add(stack.Data);
                }

                var newStack = stack.Pop();
                if (newStack == null) {
                    throw new InvalidOperationException();
                }

                stack = newStack;
            }

            result.Reverse();
            return result;
        }

        public IEnumerable<TerminalSymbol> TopContiguousTerminals {
            get {
                var auxStack = stack;
                while (auxStack != null) {
                    switch (auxStack.Data) {
                        case TerminalSymbol t:
                            yield return t;
                            break;
                        case NonTerminalSymbol:
                            yield break;
                    }
                    auxStack = auxStack.Previous;
                }
            }
        }

        public State GetTopState() => owner.states[((StateSymbol)stack.Data).State];

        public TTerminal Token => stream.TryGetElementAt(StreamPosition, out var token) ? token : owner.eofTerminal;
        public void MoveNext() => StreamPosition++;
        public int StreamPosition { get; set; }

        public override string ToString() => $"token={Token} stack=[ {stack} ]";
    }
}