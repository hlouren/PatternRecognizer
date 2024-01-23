namespace Example.NativeMetamodel;

public record Container(string Id, params Widget[] Widgets) : Widget(Id);