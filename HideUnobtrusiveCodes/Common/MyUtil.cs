using System;
using System.IO;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text.Classification;

namespace HideUnobtrusiveCodes.Common
{
    class MyUtil
    {
        #region Public Methods
        public static void AppendToFile(string filePath, string message)
        {
            try
            {
                var fs = new FileStream(filePath, FileMode.Append);

                var sw = new StreamWriter(fs);
                sw.Write(message);
                sw.Write(Environment.NewLine);
                sw.Close();
                fs.Close();
            }
            catch
            {
                // ignored
            }
        }

        public static void ApplyStyle(IEditorFormatMap editorFormatMap, TextBox textBlock)
        {
            var typeface = editorFormatMap.GetTypeface();
            var fontSize = editorFormatMap.GetFontSize();
            if (typeface != null)
            {
                //Set format for text block
                textBlock.FontFamily  = typeface.FontFamily;
                textBlock.FontStyle   = typeface.Style;
                textBlock.FontStretch = typeface.Stretch;
                textBlock.FontWeight  = typeface.Weight;
                textBlock.FontSize    = fontSize;
            }
        }

        /// <summary>
        ///     Gets the text block styler.
        /// </summary>
        public static Action<TextBox> GetTextBlockStyler(IEditorFormatMap editorFormatMap)
        {
            return textblock => ApplyStyle(editorFormatMap, textblock);
        }

        public static void Trace(string message)
        {
            AppendToFile("d:\\trace.txt", message);
        }
        #endregion
    }
}