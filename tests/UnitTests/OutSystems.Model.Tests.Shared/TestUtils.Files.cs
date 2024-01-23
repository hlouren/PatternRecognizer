using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OutSystems.Model.Tests.Shared;

/// <summary>
/// Contains shared methods to be used by unit tests
/// </summary>
public static partial class TestUtils {

    /// <summary>
    /// Contains methods for handling with tests input and output files
    /// </summary>
    public static class Files {

        public record InputFile(string TestCaseName, string Path) {
            public override string ToString() => TestCaseName;
        }

        public static IEnumerable<InputFile> GetInputFilesByPattern(string filenamePattern, string? prefixToRemove = null, [CallerFilePath] string testFilePath = "") {
            var testFileName = Path.GetFileName(testFilePath);
            testFileName = $"{testFileName[..testFileName.LastIndexOf(".")]}";
            var folder = Path.GetFullPath(Path.Combine(testFilePath, "..", "Inputs"));

            foreach (var path in Directory.GetFiles(folder, filenamePattern)) {
                var unitName = Path.GetFileNameWithoutExtension(path);
                if (prefixToRemove != null && unitName.StartsWith(prefixToRemove)) {
                    unitName = unitName[prefixToRemove.Length..];
                }
                yield return new InputFile(unitName, path);
            }
        }

        public static IEnumerable<InputFile> GetInputFiles([CallerFilePath] string testFilePath = "", [CallerMemberName] string methodName = "") {
            var testFileName = Path.GetFileName(testFilePath);
            testFileName = $"{testFileName[..testFileName.LastIndexOf(".")]}";
            var folder = Path.GetFullPath(Path.Combine(testFilePath, "..", "Inputs", testFileName));

            foreach (var path in Directory.GetFiles(folder, $"{methodName}.*")) {
                var fileName = Path.GetFileNameWithoutExtension(path);
                var testCaseName = fileName[(methodName.Length + 1)..];
                yield return new InputFile(testCaseName, path);
            }
        }

        public static string GetObtainedFileName(string extension, [CallerFilePath] string testFilePath = "", [CallerMemberName] string methodName = "", string testCaseName = "") {
            var testFileName = Path.GetFileName(testFilePath);
            testFileName = $"{testFileName[..testFileName.LastIndexOf(".")]}";
            var folder = Path.GetFullPath(Path.Combine(testFilePath, "..", "Results", testFileName, "Obtained"));
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            return Path.Combine(folder, $"{methodName}{(testCaseName == null || testCaseName == "" ? "" : $".{testCaseName}")}.{extension}");
        }

        public static string SaveObtainedResult(string result, string fileExtension, [CallerFilePath] string testFilePath = "", [CallerMemberName] string methodName = "", string testCaseName = "") {
            var obtainedFile = GetObtainedFileName(fileExtension, testFilePath, methodName, testCaseName);
            result = result.Trim().NormalizeNewLines();
            File.WriteAllText(obtainedFile, result);
            return obtainedFile;
        }

        public static T GetTestCase<T>(InputFile inputFile, JsonSerializerOptions? options = default) {
            var json = File.ReadAllText(inputFile.Path);
            var updatedOptions = options != default ? new JsonSerializerOptions(options) : new JsonSerializerOptions();

            updatedOptions.ReadCommentHandling = JsonCommentHandling.Skip;

            var testCase = JsonSerializer.Deserialize<T>(json, updatedOptions);
            if (testCase == null) {
                throw new InvalidOperationException("Unable to deserialize test case");
            }
            return testCase;
        }
    }
}