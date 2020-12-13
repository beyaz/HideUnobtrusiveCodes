using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static HideUnobtrusiveCodes.Mixin;

namespace HideUnobtrusiveCodes
{
    partial class Mixin
    {
        #region Static Fields
        public static readonly DataKey<int> CurrentLineIndex = CreateKey<int>();
        public static readonly DataKey<int> LineCount = CreateKey<int>();
        public static readonly DataKey<IReadOnlyList<ITextSnapshotLine>> TextSnapshotLines = CreateKey<IReadOnlyList<ITextSnapshotLine>>();
        public static readonly DataKey<Action<ITagSpan<TagData>>> AddTagSpan = CreateKey<Action<ITagSpan<TagData>>>();
        #endregion

    }
}

namespace HideUnobtrusiveCodes.Processors
{
    static class ScopeToVariableAssignmentProcessor
    {
        #region Public Methods
        public static void ProcessScopeToVariableAssignments(Scope scope)
        {
            var getTextAtLine     = scope.Get(GetTextAtLine);
            var textSnapshotLines = scope.Get(TextSnapshotLines);
            var currentLineIndex  = scope.Get(CurrentLineIndex);

            var startLineIndex = currentLineIndex;

            var i = currentLineIndex;

            var hasValue = false;

            while (IsScopeAssignment(scope, i))
            {
                i++;
                hasValue = true;
            }

            if (!hasValue)
            {
                return;
            }

            scope.Update(IsAnyValueProcessed,true);

            i--; // come back to last successfull line


            var indexOfFirstChar = GetFirstCharIndexHasValue(getTextAtLine(startLineIndex));

            var startPoint = textSnapshotLines[startLineIndex].Start.Add(indexOfFirstChar);

            var end = textSnapshotLines[i].End;

            var span = new SnapshotSpan(startPoint, end);

            var variableNames = scope.Get(ScopeAssignmentVariableNames);

            var label = "var " + string.Join(" | ", variableNames) + " < *;";

            var tag = new TagData{Text = label,Span = span};

            scope.Get(AddTagSpan)(new TagSpan<TagData>(span, tag));

            // focus to next not processed position
            scope.Update(CurrentLineIndex,i+1);

            variableNames.Clear();
        }
        #endregion
    }
}