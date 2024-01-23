namespace Example.Definitions;

public class VirtualizationContext {
    private int nextId = 0;

    public string NextId => $"v{++nextId}";
}
