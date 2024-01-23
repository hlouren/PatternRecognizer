using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace OutSystems.Model.Tests.Shared;

public static partial class TestUtils {

    /// <summary>
    /// Contains assertion methods
    /// </summary>
    public static class Asserts {

        public static void AssertObtainedFilesResult(IEnumerable<string> obtainedFiles) {
            foreach (var path in obtainedFiles) {
                AssertObtainedFileResult(path);
            }
        }

        public static void AssertObtainedFileResult(string obtainedFile) {
            var obtained = File.ReadAllText(obtainedFile);

            var expectedFile = obtainedFile.Replace("Obtained", "Expected");
            FileAssert.Exists(expectedFile, $"Expected file {expectedFile} not found. Check obtained file {obtainedFile} and rename it if the content is ok");
            var expectedResult = File.ReadAllText(expectedFile).Trim().NormalizeNewLines();
            Assert.AreEqual(expectedResult, obtained, $"Expected file {expectedFile} and obtained file {obtainedFile} have different content");
        }

        public static void AssertTestResult(string result, string fileExtension, [CallerFilePath] string testFilePath = "", [CallerMemberName] string methodName = "", string testCaseName = "") =>
            AssertTestResultWithInvocationInfo(result, fileExtension, testFilePath, methodName, testCaseName);

        public static void AssertTestResultWithInvocationInfo(string result, string fileExtension, [CallerFilePath] string testFilePath = "", [CallerMemberName] string methodName = "", string testCaseName = "") {
            var obtainedFile = Files.GetObtainedFileName(fileExtension, testFilePath, methodName, testCaseName);
            result = result.Trim().NormalizeNewLines();
            File.WriteAllText(obtainedFile, result);

            var expectedFile = obtainedFile.Replace("Obtained", "Expected");
            FileAssert.Exists(expectedFile, $"Expected file {expectedFile} not found. Check obtained file {obtainedFile} and rename it if the content is ok");
            var expectedResult = File.ReadAllText(expectedFile).Trim().NormalizeNewLines();
            Assert.AreEqual(expectedResult, result, $"Expected file {expectedFile} and obtained file {obtainedFile} have different content");
        }
    }
}