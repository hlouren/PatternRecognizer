namespace Example.VirtualMetamodel;

public record ButtonGroup(string Id, string Variable, params Widget[] Items) : Input(Id, Variable);
