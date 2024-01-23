using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OutSystems.Model.Parser.Collections;

internal class LazyList<T> {

    private readonly IEnumerator<T> dataEnumerator;
    private readonly List<T> fetchedElements = new();

    public LazyList(IEnumerable<T> data) => dataEnumerator = data.GetEnumerator();

    public bool TryGetElementAt(int index, [MaybeNullWhen(false)] out T item) {
        if (!FetchUntilCountEquals(index + 1)) {
            item = default;
            return false;
        }

        item = fetchedElements.ElementAt(index);
        return true;
    }

    private bool FetchUntilCountEquals(int count) {
        while (fetchedElements.Count < count && dataEnumerator.MoveNext()) {
            fetchedElements.Add(dataEnumerator.Current);
        }

        return fetchedElements.Count >= count;
    }
}
