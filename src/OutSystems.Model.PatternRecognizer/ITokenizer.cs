using System.Collections.Generic;

namespace OutSystems.Model.PatternRecognizer;

public interface ITokenizer<T> {

    string GetTypeName(T item);
    IEnumerable<T> GetChildren(T item);
    (T Object, T ObjectForToken) Normalize(T item);
}
