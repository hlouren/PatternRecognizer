namespace Example.NativeMetamodel;

public record Label(string Id, Widget Widget, params Widget[] Widgets) : Widget(Id);
