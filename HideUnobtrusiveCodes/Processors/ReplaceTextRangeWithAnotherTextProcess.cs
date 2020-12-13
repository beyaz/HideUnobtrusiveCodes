using System;
using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Tagging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static HideUnobtrusiveCodes.Processors.Keys;

namespace HideUnobtrusiveCodes.Processors
{
    /// <summary>
    ///     The replace text range with another text process
    /// </summary>
    class ReplaceTextRangeWithAnotherTextProcess
    {
        #region Public Methods
        /// <summary>
        ///     Processes the replace text range with another texts.
        /// </summary>
        public static void ProcessReplaceTextRangeWithAnotherTexts(Scope scope)
        {
            var getTextAtLine     = scope.Get(GetTextAtLine);
            var textSnapshotLines = scope.Get(TextSnapshotLines);
            var currentLineIndex  = scope.Get(CurrentLineIndex);
            var options           = scope.Get(Option);

            var currentTextSnapshotLine = textSnapshotLines[currentLineIndex];

            foreach (var item in options.ReplaceTextRangeWithAnotherText)
            {
                var text = getTextAtLine(currentLineIndex);

                var indexOf = text.IndexOf(item.Value, StringComparison.OrdinalIgnoreCase);

                if (indexOf >= 0)
                {
                    var addTagSpan = scope.Get(AddTagSpan);

                    var span = new SnapshotSpan(currentTextSnapshotLine.Start + indexOf, item.Value.Length);
                    var tag  = new TagData {Text = item.NewValue, Span = span};

                    addTagSpan(new TagSpan<TagData>(span, tag));

                    // focus to next not processed position
                    scope.Update(CurrentLineIndex, currentLineIndex + 1);

                    scope.Update(IsAnyValueProcessed, true);

                    return;
                }
            }
        }
        #endregion
    }
}