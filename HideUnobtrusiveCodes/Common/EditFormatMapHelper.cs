using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;

namespace HideUnobtrusiveCodes.Common
{
    /// <summary>
    ///     The edit format map helper
    /// </summary>
    static class EditFormatMapHelper
    {
        #region Public Methods
        /// <summary>
        ///     Gets the size of the font.
        /// </summary>
        public static double GetFontSize(this IEditorFormatMap formatMap)
        {
            var result     = 10.0;
            var properties = formatMap.GetProperties("Plain Text");
            var flag       = properties.Contains("FontRenderingSize");
            if (flag)
            {
                result = (double) properties["FontRenderingSize"];
            }

            return result;
        }

        /// <summary>
        ///     Gets the typeface.
        /// </summary>
        public static Typeface GetTypeface(this IEditorFormatMap formatMap)
        {
            Typeface result     = null;
            var      properties = formatMap.GetProperties("Plain Text");
            var      flag       = properties.Contains("Typeface");
            if (flag)
            {
                result = properties["Typeface"] as Typeface;
            }

            return result;
        }
        #endregion
    }
}