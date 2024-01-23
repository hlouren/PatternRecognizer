using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.ParserGenerator.Collections;

internal class ExtendedSet<T> where T : notnull {

    private bool containsEpsilon;
    public bool ContainsEpsilon => containsEpsilon;

    private readonly HashSet<T> items = new();
    public IEnumerable<T> Items => items;

    public bool Add(T item) => items.Add(item);

    public bool AddEpsilon() {
        if (containsEpsilon) {
            return false;
        } else {
            containsEpsilon = true;
            return true;
        }
    }

    public bool AddRange(ExtendedSet<T> other, bool addEpsilonIfPresent = true) {
        var changed = false;
        var newItems = other.items.Except(items);
        if (newItems.Any()) {
            changed = true;
            foreach (var newItem in newItems) {
                Add(newItem);
            }
        }

        if (addEpsilonIfPresent && !containsEpsilon && other.containsEpsilon) {
            changed = true;
            containsEpsilon = true;
        }

        return changed;
    }

    public override string ToString() {
        var itemsAsString =
            (containsEpsilon ? new[] { "ε" } : Enumerable.Empty<string>()).
            Concat(items.OrderBy(i => i).Select(i => i.ToString()));

        return string.Join(" ", itemsAsString);
    }
}
