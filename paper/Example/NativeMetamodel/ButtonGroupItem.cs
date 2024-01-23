namespace Example.NativeMetamodel;

public record ButtonGroupItem(string Id, string Value, params Text[] TextWidgets) : Widget(Id) ;
