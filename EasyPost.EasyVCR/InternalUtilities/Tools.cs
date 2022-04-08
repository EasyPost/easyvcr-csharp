using System;
using System.IO;
using System.Text;

namespace EasyPost.EasyVCR.InternalUtilities
{
    public static class Tools
    {
        /// <summary>
        ///     Combine a folder and a file name to create a path.
        /// </summary>
        /// <param name="folderPath">Path to parent folder of file.</param>
        /// <param name="fileName">Name of file.</param>
        /// <returns>Path to file.</returns>
        internal static string GetFilePath(string folderPath, string fileName)
        {
            return Path.Combine(folderPath, fileName);
        }

        /// <summary>
        ///     Convert a string to a base64 encoded string.
        /// </summary>
        /// <param name="input">String to be encoded.</param>
        /// <returns>A base64 encoded string.</returns>
        internal static string ToBase64String(string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }
    }
}
