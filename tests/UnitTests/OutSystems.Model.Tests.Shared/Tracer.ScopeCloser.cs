using System;
using System.CodeDom.Compiler;

namespace OutSystems.Model.Tests.Shared;

public partial class Tracer {

    private record ScopeCloser(IndentedTextWriter Writer) : IDisposable {
        public void Dispose() => Writer.Indent--;
    }
}
