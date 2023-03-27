using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace EasyVCR
{
    /// <summary>
    ///     Internal tools for EasyVCR.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        ///     Combine a folder and a file name to create a path.
        /// </summary>
        /// <param name="folderPath">Path to parent folder of file.</param>
        /// <param name="fileName">Name of file.</param>
        /// <param name="createIfNotExist">Create the file path if it doesn't exist.</param>
        /// <returns>Path to file.</returns>
        public static string GetFilePath(string folderPath, string fileName, bool createIfNotExist = false)
        {
            var path = Path.Combine(folderPath, fileName);
            if (File.Exists(path)) return path;
            if (!createIfNotExist) throw new FileNotFoundException($"File {path} does not exist.");
            // Create the directory if it doesn't exist (file will be created when writing to it)
            Directory.CreateDirectory(folderPath);
            return path;

        }

        /// <summary>
        ///     Check if a response came from a recording.
        /// </summary>
        /// <param name="response">An HttpResponseMessage to check if came from an EasyVCR recording or not.</param>
        /// <returns>True if the response came from a recording, false otherwise.</returns>
        public static bool ResponseCameFromRecording(HttpResponseMessage response)
        {
            return response.Headers.Contains(Defaults.ViaRecordingHeaderKey);
        }

        internal static bool IsJsonArray(object? obj)
        {
            return obj is JArray;
        }

        internal static bool IsJsonDictionary(object? obj)
        {
            return obj is JObject;
        }
    }
}
