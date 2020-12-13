using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Tagging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static HideUnobtrusiveCodes.Common.Mixin;

namespace HideUnobtrusiveCodes.Processors
{
    /// <summary>
    ///     The comment processor
    /// </summary>
    static class CommentProcessor
    {
        #region Public Methods
        /// <summary>
        ///     Processes the replace with comment icon when line starts with.
        /// </summary>
        public static void ProcessReplaceWithCommentIconWhenLineStartsWith(Scope scope)
        {
            var getTextAtLine     = scope.Get(GetTextAtLine);
            var textSnapshotLines = scope.Get(TextSnapshotLines);
            var currentLineIndex  = scope.Get(CurrentLineIndex);
            var options           = scope.Get(Option);

            var startLineIndex = currentLineIndex;

            var i = currentLineIndex;

            var hasValue = false;

            while (LineStartsWith(scope, i, options.ReplaceWithCommentIconWhenLineStartsWith))
            {
                i++;
                hasValue = true;
            }

            if (!hasValue)
            {
                return;
            }

            scope.Update(IsAnyValueProcessed, true);

            i--; // come back to last successfull line

            var indexOfFirstChar = GetFirstCharIndexHasValue(getTextAtLine(startLineIndex));

            var startPoint = textSnapshotLines[startLineIndex].Start.Add(indexOfFirstChar);

            var end = textSnapshotLines[i].End;

            var span = new SnapshotSpan(startPoint, end);

            var tag = new TagData {Span = span, ShowCommentIcon = true};

            scope.Get(AddTagSpan)(new TagSpan<TagData>(span, tag));

            // focus to next not processed position
            scope.Update(CurrentLineIndex, i + 1);
        }
        #endregion
    }
}