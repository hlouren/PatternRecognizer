using System;
using System.Collections.Generic;
using System.Linq;

namespace Example;

internal static class ObjectExtensions {

    public static IEnumerable<T> GetChildren<T>(this T obj) {
        if (obj == null) {
            yield break;
        }

        var type = obj.GetType();
        var props = type.GetProperties().OrderBy(p => p.Name);
        var childProps = props.Where(p => p.PropertyType.IsArray);

        foreach (var prop in childProps) {
            if (prop.GetValue(obj) is not Array children) {
                continue;
            }

            foreach (var child in children) {
                yield return (T)child;
            }
        }
    }
}
