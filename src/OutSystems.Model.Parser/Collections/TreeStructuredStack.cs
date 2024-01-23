namespace OutSystems.Model.Parser.Collections;

internal record TreeStructuredStack<T>(T Data, TreeStructuredStack<T>? Previous = null)
{
    public TreeStructuredStack<T> Push(T data) => new(data, this);
    public TreeStructuredStack<T>? Pop() => Previous;

    public override string? ToString() =>
        Previous == null ? Data?.ToString() : $"{Previous} {Data}";
}
