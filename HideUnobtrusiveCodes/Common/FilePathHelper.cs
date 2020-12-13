using System.IO;

namespace HideUnobtrusiveCodes
{
    /// <summary>
    ///     The file path helper
    /// </summary>
    static class FilePathHelper
    {
        /// <summary>
        ///     Gets the file full path.
        /// </summary>
        public static string GetFileFullPath(string fileName)
        {
            return Path.GetDirectoryName(typeof(FilePathHelper).Assembly.Location) + Path.DirectorySeparatorChar + fileName;
        }
    }
}