namespace Example.VirtualMetamodel;

public record Label(string Id, NativeMetamodel.Widget TargetWidget, params Widget[] Widgets) : Widget(Id);
