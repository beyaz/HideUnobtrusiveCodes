using System;
using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Tagging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static HideUnobtrusiveCodes.Common.Mixin;
using static HideUnobtrusiveCodes.Processors.Keys;

namespace HideUnobtrusiveCodes.Processors
{
    /// <summary>
    ///     The replace line with another text process
    /// </summary>
    class ReplaceLineWithAnotherTextProcess
    {
        #region Public Methods
        /// <summary>
        ///     Processes the replace line with another text.
        /// </summary>
        public static void ProcessReplaceLineWithAnotherText(Scope scope)
        {
            var getTextAtLine     = scope.Get(GetTextAtLine);
            var textSnapshotLines = scope.Get(TextSnapshotLines);
            var currentLineIndex  = scope.Get(CurrentLineIndex);
            var options           = scope.Get(Option);

            foreach (var item in options.ReplaceLineWithAnotherTextWhenLineContains)
            {
                if (LineContains(scope, currentLineIndex, item.Value))
                {
                    scope.Update(IsAnyValueProcessed, true);

                    var indexOfFirstChar = GetFirstCharIndexHasValue(getTextAtLine(currentLineIndex));

                    var startPoint = textSnapshotLines[currentLineIndex].Start.Add(indexOfFirstChar);

                    var end = textSnapshotLines[currentLineIndex].End;

                    var span = new SnapshotSpan(startPoint, end);

                    var tag = new TagData {Text = item.NewValue, Span = span};

                    scope.Get(AddTagSpan)(new TagSpan<TagData>(span, tag));

                    // focus to next not processed position
                    scope.Update(CurrentLineIndex, currentLineIndex + 1);
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Lines the contains.
        /// </summary>
        static bool LineContains(Scope scope, int lineIndex, string value)
        {
            var getTextAtLine = scope.Get(GetTextAtLine);

            var line = getTextAtLine(lineIndex);
            if (line == null)
            {
                return false;
            }

            return line.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        #endregion
    }
}