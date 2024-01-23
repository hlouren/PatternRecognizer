using System.Linq;

namespace OutSystems.Model.Tests.Shared;

public sealed record ParseTreeNode(string Type, string? CaptureName, ParseTreeNode[]? Children = null) {
    public override string ToString() {
        var tracer = new Tracer();
        Print(tracer);
        return tracer.ToString();
    }

    public void Print(Tracer tracer) {
        tracer.WriteLine(Type + (CaptureName == null ? "" : $" CaptureName={CaptureName}"));
        if (Children?.Any() == true) {
            tracer.IncrementIndent();
            foreach (var child in Children) {
                child.Print(tracer);
            }
            tracer.DecrementIndent();
        }
    }
}
