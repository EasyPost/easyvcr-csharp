using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace EasyPost.EasyVCR.Tests
{
    public static class TestUtils
    {
        internal static Cassette GetCassette(string cassetteName)
        {
            return new Cassette(GetDirectoryInCurrentDirectory("cassettes"), cassetteName);
        }

        internal static string GetCurrentDirectory()
        {
            return _GetCurrentDirectory();
        }

        internal static string GetDirectoryInCurrentDirectory(string directoryPath)
        {
            return Path.Combine(GetCurrentDirectory(), directoryPath);
        }

        private static string _GetCurrentDirectory([CallerFilePath] string sourceFilePath = "")
        {
            if (string.IsNullOrEmpty(sourceFilePath)) throw new ArgumentNullException(nameof(sourceFilePath));

            var path = Path.GetDirectoryName(sourceFilePath);
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Could not get directory from source file path");

            return path;
        }
    }
}
