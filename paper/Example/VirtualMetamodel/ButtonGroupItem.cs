namespace Example.VirtualMetamodel;

public record ButtonGroupItem(string Id, string Value, params Widget[] TextWidgets) : Widget(Id);
