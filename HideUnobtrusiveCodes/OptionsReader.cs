using System;
using System.IO;
using YamlDotNet.Serialization;

namespace HideUnobtrusiveCodes
{
    /// <summary>
    ///     The options reader
    /// </summary>
    public class OptionsReader
    {
        #region Static Fields
        /// <summary>
        ///     The instance
        /// </summary>
        static OptionsModel instance;
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the option file path.
        /// </summary>
        static string OptionFilePath => Path.GetDirectoryName(typeof(OptionsReader).Assembly.Location) + Path.DirectorySeparatorChar + "HideUnobtrusiveCodes.Options.yaml";
        #endregion

        #region Public Methods
        /// <summary>
        ///     Tries the read from file.
        /// </summary>
        public static OptionsModel ReadOptionsFromFile()
        {
            if (instance != null)
            {
                return instance;
            }

            return instance = ReadFromFile(OptionFilePath, () => new FileNotFoundException(OptionFilePath));
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Reads from file.
        /// </summary>
        static OptionsModel ReadFromFile(string filePath, Action whenFileIsNotFound)
        {
            if (!File.Exists(filePath))
            {
                whenFileIsNotFound();
                return null;
            }

            var yamlContent = string.Empty;

            using (TextReader reader = new StreamReader(File.OpenRead(filePath)))
            {
                yamlContent = reader.ReadToEnd();
            }

            return new Deserializer().Deserialize<OptionsModel>(yamlContent);
        }
        #endregion
    }
}