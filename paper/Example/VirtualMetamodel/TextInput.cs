namespace Example.VirtualMetamodel;

// TextInput and BooleanInput are the only actual virtual widgets...

public record TextInput(string Id, string Variable, string Label, int Pattern) : Widget(Id) { }
