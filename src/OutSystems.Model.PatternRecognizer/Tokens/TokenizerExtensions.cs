using System;
using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.PatternRecognizer.Tokens;

internal static class TokenizerExtensions {

    private static IEnumerable<TToken> Tokenize<TItem, TToken>(
        this TItem node,
        Func<TItem, IEnumerable<TItem>> getChildren,
        Func<TItem, TerminalKind, TToken> createToken) {

        var children = getChildren(node);

        yield return createToken(node, TerminalKind.Open);
        foreach (var childToken in children.Tokenize(getChildren, createToken)) {
            yield return childToken;
        }
        yield return createToken(node, TerminalKind.Close);
    }

    private static IEnumerable<TToken> Tokenize<TItem, TToken>(
        this IEnumerable<TItem> items,
        Func<TItem, IEnumerable<TItem>> getChildren,
        Func<TItem, TerminalKind, TToken> createToken) =>
        items.SelectMany(i => i.Tokenize(getChildren, createToken));

    public static IEnumerable<ObjectToken<TObject>> Tokenize<TObject>(
        this ITokenizer<TObject> tokenizer,
        IEnumerable<TObject> objects) {

        return objects.Select(tokenizer.Normalize).Tokenize<(TObject Object, TObject ObjectForToken), ObjectToken<TObject>>(
            p => tokenizer.GetChildren(p.ObjectForToken).Select(tokenizer.Normalize),
            (w, kind) => new(tokenizer.GetTypeName(w.ObjectForToken), kind, w.Object, w.ObjectForToken));
    }
}
