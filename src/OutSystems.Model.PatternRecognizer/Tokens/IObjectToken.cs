namespace OutSystems.Model.PatternRecognizer.Tokens;

internal interface IObjectToken<out TObject> {
    TObject Object { get; }
    TObject ObjectForToken { get; }
}
