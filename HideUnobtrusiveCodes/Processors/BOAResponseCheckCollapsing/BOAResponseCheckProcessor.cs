using System;
using System.Text;
using HideUnobtrusiveCodes.Common;
using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Tagging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static System.String;
using static HideUnobtrusiveCodes.Common.Mixin;

namespace HideUnobtrusiveCodes.Processors.BOAResponseCheckCollapsing
{
    /// <summary>
    ///     The boa response check processor
    /// </summary>
    static class BOAResponseCheckProcessor
    {
        #region Static Fields
        /// <summary>
        ///     The cursor
        /// </summary>
        static readonly DataKey<int> Cursor = new DataKey<int>(typeof(BOAResponseCheckProcessor), nameof(Cursor));

        /// <summary>
        ///     The is parse failed
        /// </summary>
        static readonly DataKey<bool> IsParseFailed = new DataKey<bool>(typeof(BOAResponseCheckProcessor), nameof(IsParseFailed));

        /// <summary>
        ///     The response assignment line
        /// </summary>
        static readonly DataKey<VariableAssignmentLine> ResponseAssignmentLine = new DataKey<VariableAssignmentLine>(typeof(BOAResponseCheckProcessor), nameof(ResponseAssignmentLine));
        #endregion

        #region Public Methods
        /// <summary>
        ///     Processes the specified scope.
        /// </summary>
        public static void Process(Scope scope)
        {
            //var response = bo.call();   i                 
            //if (!response.Success)      i+1
            //{                           i+2
            //    return returnObject.Add(response);       -> var x = bo.call();
            //}
            // var x = response.Value;

            var lineCount         = scope.Get(Keys.LineCount);
            var getTextAtLine     = scope.Get(Keys.GetTextAtLine);
            var currentLineIndex  = scope.Get(Keys.CurrentLineIndex);
            var addTagSpan        = scope.Get(Keys.AddTagSpan);
            var textSnapshotLines = scope.Get(Keys.TextSnapshotLines);

            scope.Update(IsParseFailed, false);
            scope.Update(Cursor, currentLineIndex);

            if (IsEmptyOrCommentLine(getTextAtLine(currentLineIndex)?.Trim()))
            {
                return;
            }

            Run(scope,
                CheckMinimumLineCount,
                MoveCursor,
                ShouldBeResponseCheck,
                MoveCursor,
                s => ShouldBe(s, "{"),
                MoveCursor,
                ShouldBeCombineResponseWithReturnObject,
                MoveCursor,
                s => ShouldBe(s, "}"),
                MoveCursor,
                CalculateSpans);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Calculates the spans.
        /// </summary>
        static void CalculateSpans(Scope scope)
        {
            var lineCount              = scope.Get(Keys.LineCount);
            var getTextAtLine          = scope.Get(Keys.GetTextAtLine);
            var currentLineIndex       = scope.Get(Keys.CurrentLineIndex);
            var addTagSpan             = scope.Get(Keys.AddTagSpan);
            var textSnapshotLines      = scope.Get(Keys.TextSnapshotLines);
            var responseAssignmentLine = scope.Get(ResponseAssignmentLine);

            var responseValueAssignmentToAnotherVariable = VariableAssignmentLine.Parse(getTextAtLine(scope.Get(Cursor)));

            if (responseValueAssignmentToAnotherVariable != null &&
                VariableAssignmentLine.IsResponseValueMatch(responseAssignmentLine, responseValueAssignmentToAnotherVariable))
            {
                var sb = new StringBuilder();
                if (responseValueAssignmentToAnotherVariable.VariableTypeName != null)
                {
                    sb.Append(responseValueAssignmentToAnotherVariable.VariableTypeName);
                    sb.Append(" ");
                }

                sb.Append(responseValueAssignmentToAnotherVariable.VariableName);

                sb.Append(" = ");

                sb.Append(responseAssignmentLine.AssignedValue);
                sb.Append(";");

                var span = new SnapshotSpan(textSnapshotLines[currentLineIndex].Start.SkipChars(' '), textSnapshotLines[scope.Get(Cursor)].End);
                var tag  = new TagData {Text = sb.ToString(), Span = span};

                addTagSpan(new TagSpan<TagData>(span, tag));

                scope.Update(Keys.CurrentLineIndex, scope.Get(Cursor) + 1);
                scope.Update(Keys.IsAnyValueProcessed, true);

                return;
            }

            scope.Update(Cursor, scope.Get(Cursor) - 1);

            {
                var span = new SnapshotSpan(textSnapshotLines[currentLineIndex].Start.SkipChars(' '), textSnapshotLines[scope.Get(Cursor)].End);
                var tag  = new TagData {Text = responseAssignmentLine.AssignedValue + ";", Span = span};

                addTagSpan(new TagSpan<TagData>(span, tag));
            }

            scope.Update(Keys.CurrentLineIndex, scope.Get(Cursor) + 1);
            scope.Update(Keys.IsAnyValueProcessed, true);
        }

        /// <summary>
        ///     Checks the minimum line count.
        /// </summary>
        static void CheckMinimumLineCount(Scope scope)
        {
            var lineCount        = scope.Get(Keys.LineCount);
            var currentLineIndex = scope.Get(Keys.CurrentLineIndex);

            if (currentLineIndex + 4 >= lineCount)
            {
                scope.Update(IsParseFailed, true);
            }
        }

        /// <summary>
        ///     Determines whether [is empty or comment line] [the specified line].
        /// </summary>
        static bool IsEmptyOrCommentLine(string line)
        {
            return IsNullOrWhiteSpace(line) || line.StartsWith("//");
        }

        /// <summary>
        ///     Determines whether [is line equal to] [the specified scope].
        /// </summary>
        static bool IsLineEqualTo(Scope scope, string text)
        {
            var getTextAtLine = scope.Get(Keys.GetTextAtLine);
            var lineIndex     = scope.Get(Cursor);

            return getTextAtLine(lineIndex)?.Trim() == text;
        }

        /// <summary>
        ///     Moves the cursor.
        /// </summary>
        static void MoveCursor(Scope scope)
        {
            var lineCount     = scope.Get(Keys.LineCount);
            var getTextAtLine = scope.Get(Keys.GetTextAtLine);
            var cursor        = scope.Get(Cursor) + 1;

            while (cursor < lineCount)
            {
                var textAtLine = getTextAtLine(cursor)?.Trim();

                if (IsEmptyOrCommentLine(textAtLine))
                {
                    cursor++;
                    continue;
                }

                break;
            }

            scope.Update(Cursor, cursor);
        }

        /// <summary>
        ///     Runs the specified scope.
        /// </summary>
        static void Run(Scope scope, params Action<Scope>[] actions)
        {
            foreach (var action in actions)
            {
                action(scope);
                if (scope.Get(IsParseFailed))
                {
                    return;
                }
            }
        }

        /// <summary>
        ///     Shoulds the be.
        /// </summary>
        static void ShouldBe(Scope scope, string expectedText)
        {
            var lineCount     = scope.Get(Keys.LineCount);
            var getTextAtLine = scope.Get(Keys.GetTextAtLine);
            var cursor        = scope.Get(Cursor);

            if (!IsLineEqualTo(scope, expectedText))
            {
                scope.Update(IsParseFailed, true);
            }
        }

        /// <summary>
        ///     Shoulds the be combine response with return object.
        /// </summary>
        static void ShouldBeCombineResponseWithReturnObject(Scope scope)
        {
            var lineCount              = scope.Get(Keys.LineCount);
            var getTextAtLine          = scope.Get(Keys.GetTextAtLine);
            var currentLineIndex       = scope.Get(Keys.CurrentLineIndex);
            var addTagSpan             = scope.Get(Keys.AddTagSpan);
            var textSnapshotLines      = scope.Get(Keys.TextSnapshotLines);
            var responseAssignmentLine = scope.Get(ResponseAssignmentLine);

            if (IsLineEqualTo(scope, $"returnObject.Results.AddRange({responseAssignmentLine.VariableName}.Results);"))
            {
                MoveCursor(scope);

                if (IsLineEqualTo(scope, "return returnObject;"))
                {
                    return;
                }
            }

            if (IsLineEqualTo(scope, $"return returnObject.Add({responseAssignmentLine.VariableName});"))
            {
                return;
            }

            scope.Update(IsParseFailed, true);
        }

        /// <summary>
        ///     Shoulds the be response check.
        /// </summary>
        static void ShouldBeResponseCheck(Scope scope)
        {
            var lineCount     = scope.Get(Keys.LineCount);
            var getTextAtLine = scope.Get(Keys.GetTextAtLine);
            var cursor        = scope.Get(Cursor);

            var line = getTextAtLine(cursor);

            if (line == null)
            {
                scope.Update(IsParseFailed, true);
                return;
            }

            line = line.Replace(" ", Empty);

            if (line.StartsWith("if(!") && line.EndsWith(".Success)"))
            {
                var responseVariableName = line.RemoveFromStart("if(!");
                responseVariableName = responseVariableName.RemoveFromEnd(".Success)");

                var responseAssignmentLine = VariableAssignmentLine.Parse(getTextAtLine(scope.Get(Keys.CurrentLineIndex)));
                if (responseAssignmentLine == null)
                {
                    scope.Update(IsParseFailed, true);
                    return;
                }

                if (responseAssignmentLine.VariableName != responseVariableName)
                {
                    scope.Update(IsParseFailed, true);
                    return;
                }

                scope.Update(ResponseAssignmentLine, responseAssignmentLine);

                return;
            }

            scope.Update(IsParseFailed, true);
        }
        #endregion
    }
}