using System;
using System.Collections.Generic;
using System.Linq;

namespace OutSystems.Model.Tests.Shared;

public static class TokenUtils {

    public static IEnumerable<Token> Tokenize(this string testCase, char separator = ',') => 
        testCase == string.Empty ? Enumerable.Empty<Token>() : testCase.Split(separator).Select(Tokenize);

    // in our tests we're using the convention that the value for a terminal can be provided in square brackets.
    // eg:
    //   <text/>       -> the value is the empty string
    //   <text/>[xpto] -> the value is xpto
    public static Token Tokenize(this string item) => (item.IndexOf("["), item.LastIndexOf("]")) switch {
        (-1, -1) => new(item, null),
        (int i1, int i2) when i1 < i2 => new(item[..i1], item.Substring(i1 + 1, i2 - i1 - 1)),
        _ => throw new ArgumentException("Unrecognized token")
    };
}
