using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using OutSystems.Model.Parser.Classifiers;
using OutSystems.Model.PatternRecognizer.Tokens;

namespace OutSystems.Model.PatternRecognizer.PatternItems;

internal static class PropertyGetters<TProperty> {

    private static readonly ConcurrentDictionary<(Type, string), PropertyGetter<Terminal, TProperty>> KnownGetters = new();

    public static PropertyGetter<Terminal, TProperty> Get(Type objectType, string propName) =>
        KnownGetters.GetOrAdd((objectType, propName),
            _ => {
                var prop = objectType.GetProperty(propName);
                if (prop == null) {
                    prop = objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).
                        Where(p => p.GetGetMethod(nonPublic: true) is { IsFinal: true, IsPrivate: true }).
                        FirstOrDefault(p => p.Name == propName);
                }
                if (prop == null) {
                    throw new InvalidOperationException($"Property {propName} not find in type {objectType.Name}");
                }

                return new(
                    t => {
                        var obj = ((IObjectToken<object>)t).ObjectForToken;
                        return (TProperty?)prop.GetValue(obj);
                    },
                    propName);
            });

    public static PropertyGetter<Terminal, TProperty> Get<TObject>(Func<TObject, TProperty?> getter, string propName) =>
        KnownGetters.GetOrAdd((typeof(TObject), propName),
            _ => new(
                t => {
                    var obj = (TObject)((IObjectToken<object>)t).ObjectForToken;
                    return getter(obj);
                },
                propName));
}
