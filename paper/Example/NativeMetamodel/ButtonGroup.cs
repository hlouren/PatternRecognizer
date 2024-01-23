namespace Example.NativeMetamodel;

public record ButtonGroup(string Id, string Variable, params ButtonGroupItem[] Items) : Input(Id, Variable);
