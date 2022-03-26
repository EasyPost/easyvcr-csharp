using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace EasyPost.Scotch
{
    public static class Utils
    {
        public static string GetFilePath(string folderPath, string fileName)
        {
            return Path.Combine(folderPath, fileName);
        }

        public static string GetFilePathInCurrentDirectory(string fileName)
        {
            return Path.Combine(GetSourceFileDirectory(), fileName);
        }

        private static string GetSourceFileDirectory([CallerFilePath] string sourceFilePath = "")
        {
            if (string.IsNullOrEmpty(sourceFilePath)) throw new ArgumentNullException(nameof(sourceFilePath));

            var path = Path.GetDirectoryName(sourceFilePath);
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Could not get directory from source file path");

            return path;
        }
    }
}
