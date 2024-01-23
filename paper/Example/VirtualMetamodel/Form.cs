namespace Example.VirtualMetamodel;

public record Form(string Id, params Widget[] Widgets) : ModelObject(Id);
