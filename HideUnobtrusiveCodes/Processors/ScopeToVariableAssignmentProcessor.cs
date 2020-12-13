using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Tagging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static HideUnobtrusiveCodes.Common.Mixin;
using static HideUnobtrusiveCodes.Processors.Keys;

namespace HideUnobtrusiveCodes.Processors
{
    /// <summary>
    ///     The scope to variable assignment processor
    /// </summary>
    static class ScopeToVariableAssignmentProcessor
    {
        #region Public Methods
        /// <summary>
        ///     Processes the scope to variable assignments.
        /// </summary>
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

            scope.Update(IsAnyValueProcessed, true);

            i--; // come back to last successfull line

            var indexOfFirstChar = GetFirstCharIndexHasValue(getTextAtLine(startLineIndex));

            var startPoint = textSnapshotLines[startLineIndex].Start.Add(indexOfFirstChar);

            var end = textSnapshotLines[i].End;

            var span = new SnapshotSpan(startPoint, end);

            var variableNames = scope.Get(ScopeAssignmentVariableNames);

            var label = "var " + string.Join(" | ", variableNames) + " < *;";

            var tag = new TagData {Text = label, Span = span};

            scope.Get(AddTagSpan)(new TagSpan<TagData>(span, tag));

            // focus to next not processed position
            scope.Update(CurrentLineIndex, i + 1);

            variableNames.Clear();
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Determines whether [is scope assignment] [the specified scope].
        /// </summary>
        static bool IsScopeAssignment(Scope scope, int lineIndex)
        {
            var getTextAtline                = scope.Get(GetTextAtLine);
            var scopeAssignmentVariableNames = scope.Get(ScopeAssignmentVariableNames);

            var isStartsWithVar = LineStartsWith(getTextAtline, lineIndex, "var ");
            if (!isStartsWithVar)
            {
                return false;
            }

            var isScopeAccess = LineContains(getTextAtline, lineIndex, "= scope.Get(");
            if (!isScopeAccess)
            {
                return false;
            }

            var line = getTextAtline(lineIndex).Trim();

            line = line.RemoveFromStart("var ");

            var variableName = line.Substring(0, line.IndexOf("=")).Trim();

            scopeAssignmentVariableNames.Add(variableName);

            return true;
        }
        #endregion
    }
}