using System.Diagnostics;
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
            return Path.GetDirectoryName(WorkingDirectory) + Path.DirectorySeparatorChar + fileName;
        }

        static string WorkingDirectory
        {
            get
            {
                if (Process.GetCurrentProcess().ProcessName == "ApiInspector")
                {
                    return @"D:\git\HideUnobtrusiveCodes\HideUnobtrusiveCodes\bin\Debug\";
                }
                return  typeof(FilePathHelper).Assembly.Location;

            }
        }
        #endregion
    }
}