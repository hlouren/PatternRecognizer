namespace OutSystems.Model.Tests.Shared;

public static class StringExtensions {

    public static string NormalizeNewLines(this string str) => str.Replace("\r\n", "\n");
}