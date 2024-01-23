using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using OutSystems.Model.Tests.Shared.Metamodel;

namespace OutSystems.Model.Tests.Shared.Metamodel;

public static class ModelFactory {

    public static IScreen CreateScreen() => new Screen();

    private static readonly ConcurrentDictionary<Type, ConstructorInfo> Constructors = new();

    internal static T Create<T>() where T : IBaseObject {
        if (!typeof(T).IsInterface) {
            throw new ArgumentException("Type must be an interface");
        }

        var constructor = Constructors.GetOrAdd(typeof(T),
            _ => {
                // T should be an interface, and we want to find the corresponding implementation class
                var className = $"{typeof(T).Namespace}.{typeof(T).Name[1..]}";
                var type = Type.GetType(className);
                if (type == null) {
                    throw new ArgumentException($"Unable to find implementation class for {typeof(T).Name}");
                }
                var constructor = type.GetConstructor(Array.Empty<Type>());
                if (constructor == null) {
                    throw new ArgumentException($"Class {className} does not contain a default constructor");
                }

                return constructor;
            });

        return (T)constructor.Invoke(null);
    }
}
