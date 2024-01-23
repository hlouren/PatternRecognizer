namespace Example.VirtualMetamodel;

public record Container(string Id, params Widget[] Widgets) : Widget(Id);
