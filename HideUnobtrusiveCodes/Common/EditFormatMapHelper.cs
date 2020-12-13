using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;

namespace HideUnobtrusiveCodes.Common
{
    static class EditFormatMapHelper
    {
        #region Public Methods
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