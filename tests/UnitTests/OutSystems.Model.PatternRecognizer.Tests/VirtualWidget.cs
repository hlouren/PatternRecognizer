using OutSystems.Model.Tests.Shared.Metamodel;

namespace OutSystems.Model.PatternRecognizer.Tests;

internal record VirtualWidget(string Type, IWidget[] NativeWidgets, VirtualWidget[] Children, (string Name, object Data)[]? Captures = null);