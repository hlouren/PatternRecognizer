using System;
using System.Collections.Generic;
using System.Linq;
using OutSystems.Model.Parser.Classifiers;

namespace OutSystems.Model.Parser;

public class ConditionalTransitionTable<TValue, TTarget> {

    private readonly IClassifier<TValue>[] classifiers;
    private readonly TTarget? defaultValue;
    private readonly TTarget[] table;
    private readonly int[] classifierBaseIndex;

    public int Length => table.Length;

    public ConditionalTransitionTable(IClassifier<TValue>[] classifiers, TTarget defaultValue) :
        this(classifiers, _ => defaultValue) {
        this.defaultValue = defaultValue;
    }

    public ConditionalTransitionTable(IClassifier<TValue>[] classifiers, Func<int, TTarget> getDefaultValue) {
        this.classifiers = classifiers;

        classifierBaseIndex = new int[classifiers.Length];
        var size = 1;
        for (var i = 0; i < classifiers.Length; i++) {
            if (i != 0) {
                classifierBaseIndex[i] = size;
            }
            size *= classifiers[i].ValueCount;
        }

        table = new TTarget[size];
        for (var i = 0; i < table.Length; i++) {
            table[i] = getDefaultValue(i);
        }
    }

    public TTarget this[int index] {
        get => table[index];
        set => table[index] = value;
    }

    public int GetClassifierValueAtIndex(IClassifier<TValue> classifier, int tableIndex) {
        var classifierIndex = Array.IndexOf(classifiers, classifier);
        if (classifierIndex == -1) {
            throw new InvalidOperationException("Missing classifier");
        }

        return (tableIndex / (classifierIndex == 0 ? 1 : classifierBaseIndex[classifierIndex])) % classifier.ValueCount;
    }

    public bool TryGetTarget(TValue value, out TTarget target) {
        var index = 0;
        for (var i = 0; i < classifiers.Length; i++) {
            index += classifiers[i].GetCategory(value) * (i == 0 ? 1 : classifierBaseIndex[i]);
        }

        target = table[index];
        return target != null && !target.Equals(defaultValue);
    }

    private bool IsDefaultValue(TTarget value) => defaultValue != null && defaultValue.Equals(value);

    public override string ToString() => $"{{ {string.Join(" ; ", Entries)} }}";

    private IEnumerable<string> Entries {
        get {
            var partialValues = new int[classifiers.Length];

            for (int i = 0; i < table.Length; i++) {
                var target = table[i];
                if (IsDefaultValue(target)) {
                    continue;
                }

                var index = i;
                var labels = classifiers.Select(c => {
                    var category = index % c.ValueCount;
                    index /= c.ValueCount;
                    return c.ToString(category);
                });

                var label = string.Join(",", labels);
                if (classifiers.Length != 1) {
                    label = $"({label})";
                }

                yield return $"{label} -> {target}";
            }
        }
    }
}
