using System.IO;

namespace EasyPost.Scotch.InternalUtilities
{
    internal static class Utils
    {
        internal static string GetFilePath(string folderPath, string fileName)
        {
            return Path.Combine(folderPath, fileName);
        }
    }
}
