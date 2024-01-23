namespace Example.NativeMetamodel;

public record Text(string Id, string Value, string? Style = null) : Widget(Id) ;
