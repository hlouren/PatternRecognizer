namespace Example.NativeMetamodel;

public record Form(string Id, params Widget[] Widgets) : ModelObject(Id);
