using System.IO;

namespace EasyPost.EasyVCR.InternalUtilities
{
    internal static class Utils
    {
        /// <summary>
        /// Combine a folder and a file name to create a path.
        /// </summary>
        /// <param name="folderPath">Path to parent folder of file.</param>
        /// <param name="fileName">Name of file.</param>
        /// <returns>Path to file.</returns>
        internal static string GetFilePath(string folderPath, string fileName)
        {
            return Path.Combine(folderPath, fileName);
        }
    }
}
