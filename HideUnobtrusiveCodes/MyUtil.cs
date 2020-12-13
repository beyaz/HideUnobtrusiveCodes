using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace HideUnobtrusiveCodes
{
    class MyUtil
    {
        #region Public Methods        
        /// <summary>
        /// Gets the text block styler.
        /// </summary>
        public static Action<TextBox> GetTextBlockStyler(IEditorFormatMap editorFormatMap)
        {
            return textblock => ApplyStyle(editorFormatMap, textblock);
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

        public static void Trace(string message)
        {
            var indent = string.Empty.PadRight(traceIndent, ' ');

            message = indent + message;

            AppendToFile("d:\\trace.txt",message);
        }

        static int traceIndent = 0;

        public static void OpenTraceScope()
        {
            traceIndent += 4;
        }

        public static void CloseTraceScope()
        {
            traceIndent -= 4;
        }
        public static void Trace(IReadOnlyList<ITagSpan<IntraTextAdornmentTag>> tags)
        {
            OpenTraceScope();
            foreach (var tagSpan in tags)
            {
                Trace($"Position: {tagSpan.Span.Start.Position} - {tagSpan.Span.Length}");
            }
            CloseTraceScope();
        }
        
        public static void Trace(NormalizedSnapshotSpanCollection spans)
        {
            OpenTraceScope();
            foreach (var tagSpan in spans)
            {
                Trace($"Position: {tagSpan.Span.Start} - {tagSpan.Span.Length}");
            }
            CloseTraceScope();
        }

        #endregion
    }
}