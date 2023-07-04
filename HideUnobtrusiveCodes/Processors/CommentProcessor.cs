using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Tagging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using static HideUnobtrusiveCodes.Common.Mixin;
using static HideUnobtrusiveCodes.Processors.Keys;

namespace HideUnobtrusiveCodes.Processors
{
    static class CommentTagger
    {
        public static TagSpan<TagData> ConvertCommentToTagSpan(TaggerContext taggerContext, int startLineIndex, int endLineIndex)
        {
            var indexOfFirstChar = GetFirstCharIndexHasValue(taggerContext.ReadLineAt(startLineIndex));

            var startPoint = taggerContext.TextSnapshotLines[startLineIndex].Start.Add(indexOfFirstChar);

            var end = taggerContext.TextSnapshotLines[endLineIndex].End;

            var span = new SnapshotSpan(startPoint, end);

            var tag = new TagData {Span = span, ShowCommentIcon = true};

            return new TagSpan<TagData>(span, tag);
        }
    }

    /// <summary>
    ///     The comment processor
    /// </summary>
    static class CommentProcessor
    {
        public static (int startLineIndex, int endLineIndex) Process(int currentLineIndex, Func<int, bool> canAccessLineAt, Func<int, string> readLineAt,
            
            Func<string,bool> isCommentLine)
        {
            var i = currentLineIndex;

            var hasValue = false;

            while (canAccessLineAt(i) && isCommentLine(readLineAt(i)))
            {
                i++;
                hasValue = true;
            }

            if (!hasValue)
            {
                return default;
            }
            
            i--; // come back to last successfull line

            return (currentLineIndex, i);
        }
        
        
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