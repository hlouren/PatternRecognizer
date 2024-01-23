namespace OutSystems.Model.PatternRecognizer.Tokens;

/// <summary>
/// Represents a terminal in the stream resulting from tokenizing model elements
/// </summary>
internal class ObjectToken<TObject> : Terminal, IObjectToken<TObject> {
    public TObject Object { get; }
    public TObject ObjectForToken { get; }

    public ObjectToken(string type, TerminalKind kind, TObject obj, TObject objectForToken) : base(type, kind) {
        Object = obj;
        ObjectForToken = objectForToken;
    }
}
