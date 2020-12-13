using System.IO;
using YamlDotNet.Serialization;
using static HideUnobtrusiveCodes.Common.FilePathHelper;

namespace HideUnobtrusiveCodes.Common
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

            var optionFilePath = GetFileFullPath("HideUnobtrusiveCodes.Options.yaml");

            return instance = ReadFromFile(optionFilePath);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Reads from file.
        /// </summary>
        static OptionsModel ReadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
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