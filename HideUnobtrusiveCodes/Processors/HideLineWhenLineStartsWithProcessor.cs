using System.Collections.Generic;
using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Tagging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static HideUnobtrusiveCodes.Common.Mixin;
using static HideUnobtrusiveCodes.Processors.Keys;

namespace HideUnobtrusiveCodes.Processors
{
    /// <summary>
    ///     The hide line when line starts with processor
    /// </summary>
    static class HideLineWhenLineStartsWithProcessor
    {
        #region Public Methods
        /// <summary>
        ///     Processes the hide line when line starts with.
        /// </summary>
        public static void ProcessHideLineWhenLineStartsWith(Scope scope)
        {
            var getTextAtLine     = scope.Get(GetTextAtLine);
            var textSnapshotLines = scope.Get(TextSnapshotLines);
            var currentLineIndex  = scope.Get(CurrentLineIndex);
            var options           = scope.Get(Option);
            var addTagSpan        = scope.Get(AddTagSpan);

            var startLineIndex = currentLineIndex;

            var i = currentLineIndex;

            while (LineStartsWith(scope, i, options.HideLineWhenLineStartsWith))
            {
                i++;
                scope.Update(IsAnyValueProcessed, true);
            }

            if (!scope.Get(IsAnyValueProcessed))
            {
                return;
            }

            i--; // come back to last successfull line

            var span = HideLines(textSnapshotLines, currentLineIndex, i);

            var tag = new TagData {Span = span};

            addTagSpan(new TagSpan<TagData>(span, tag));

            // focus to next not processed position
            scope.Update(CurrentLineIndex, i + 1);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Hides the lines.
        /// </summary>
        static SnapshotSpan HideLines(IReadOnlyList<ITextSnapshotLine> snapshotLines, int startLineIndex, int endLineIndex)
        {
            var length = snapshotLines.Count;

            var startLine = snapshotLines[startLineIndex];
            var endLine   = snapshotLines[endLineIndex];

            if (startLineIndex > 0)
            {
                var start = snapshotLines[startLineIndex - 1].End;
                var end   = snapshotLines[endLineIndex].End;

                return new SnapshotSpan(start, end);
            }

            if (startLineIndex + 1 < length)
            {
                var start = startLine.Start;
                var end   = snapshotLines[endLineIndex].End;

                return new SnapshotSpan(start, end);
            }

            {
                var start = startLine.Start;
                var end   = snapshotLines[endLineIndex].End;

                return new SnapshotSpan(start, end);
            }
        }
        #endregion
    }
}