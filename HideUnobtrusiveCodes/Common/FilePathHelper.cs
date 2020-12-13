using System.IO;

namespace HideUnobtrusiveCodes.Common
{
    /// <summary>
    ///     The file path helper
    /// </summary>
    static class FilePathHelper
    {
        #region Public Methods
        /// <summary>
        ///     Gets the file full path.
        /// </summary>
        public static string GetFileFullPath(string fileName)
        {
            return Path.GetDirectoryName(typeof(FilePathHelper).Assembly.Location) + Path.DirectorySeparatorChar + fileName;
        }
        #endregion
    }
}