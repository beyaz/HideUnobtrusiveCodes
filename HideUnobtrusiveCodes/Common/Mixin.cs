using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using HideUnobtrusiveCodes.BOAResponseCheckCollapsing;
using Microsoft.VisualStudio.Text;
using static HideUnobtrusiveCodes.MyUtil;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static HideUnobtrusiveCodes.Mixin;

namespace HideUnobtrusiveCodes
{
   

    /// <summary>
    ///     The mixin
    /// </summary>
    static class Mixin
    {
        public static readonly DataKey<int> CurrentLineIndex = CreateKey<int>();
        public static readonly DataKey<int> LineCount = CreateKey<int>();
        public static readonly DataKey<IReadOnlyList<ITextSnapshotLine>> TextSnapshotLines = CreateKey<IReadOnlyList<ITextSnapshotLine>>();
        public static readonly DataKey<Action<ITagSpan<TagData>>> AddTagSpan = CreateKey<Action<ITagSpan<TagData>>>();


        public static readonly DataKey<Func<int, string>> GetTextAtLine = CreateKey<Func<int, string>>();
        public static readonly DataKey<List<string>> ScopeAssignmentVariableNames = CreateKey<List<string>>();
        public static readonly DataKey<OptionsModel> Option = CreateKey<OptionsModel>();

        public static readonly DataKey<Action<TextBox>> UpdateTextBoxStyleForVisualStudio = CreateKey<Action<TextBox>>();
        
        public static readonly DataKey<TagData> TagModel = CreateKey<TagData>();
        public static readonly DataKey<Action<TagData>> OnAdornmentClicked = CreateKey<Action<TagData>>();
        public static readonly DataKey<bool> IsAnyValueProcessed = CreateKey<bool>();

        static DataKey<T> CreateKey<T>([CallerMemberName] string propertyName = null)
        {
            return new DataKey<T>(typeof(Mixin), propertyName);
        }

        public static bool LineStartsWith(Func<int,string> getTextAtline, int lineIndex, string value)
        {
            return getTextAtline(lineIndex)?.TrimStart().StartsWith(value)??false;
        }
        public static bool LineStartsWith(Scope scope, int lineIndex, string[] values)
        {
            var getTextAtLine = scope.Get(GetTextAtLine);

            var line = getTextAtLine(lineIndex);
            if (line == null)
            {
                return false;
            }

            line = line.TrimStart();

            foreach (var value in values)
            {
                if (line.StartsWith(value))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool LineContains(Scope scope, int lineIndex, string value)
        {
            var getTextAtLine = scope.Get(GetTextAtLine);

            var line = getTextAtLine(lineIndex);
            if (line == null)
            {
                return false;
            }

            return line.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool LineContains(Func<int,string> getTextAtline, int lineIndex, string value)
        {
            return getTextAtline(lineIndex)?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsScopeAssignment(Scope scope, int lineIndex)
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

             line = line .RemoveFromStart("var ");

            var variableName = line.Substring(0, line.IndexOf("=")).Trim();
            
            scopeAssignmentVariableNames.Add(variableName);
            
            return true;
        }

        public static  Func<int,string> GetTextAtLineFunc(ITextSnapshotLine[] lines)
        {
            var length = lines.Length;

            return (requestedLine) =>
            {
                if (requestedLine < length)
                {
                    return lines[requestedLine].GetText();
                }

                return null;
            };
        }

        public static SnapshotSpan HideLines(IReadOnlyList<ITextSnapshotLine> snapshotLines,int startLineIndex,int endLineIndex)
        {
            var length = snapshotLines.Count;

            var startLine = snapshotLines[startLineIndex];
            var endLine = snapshotLines[endLineIndex];

            if (startLineIndex > 0)
            {
                var start = snapshotLines[startLineIndex - 1].End;
                var end = snapshotLines[endLineIndex].End;

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


        #region Public Methods
        public static int GetFirstCharIndexHasValue(string value)
        {
            return value.IndexOf(value.TrimStart().First().ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasIntersection(NormalizedSnapshotSpanCollection a, NormalizedSnapshotSpanCollection b)
        {
            try
            {
                return a.IntersectsWith(b);
            }
            catch (Exception e)
            {
                Trace(e.ToString());
            }

            return false;
        }

        /// <summary>
        ///     Determines whether [is intersect with disabled spans] [the specified scope].
        /// </summary>
        public static bool IsIntersectWithDisabledSpans(AdornmentTaggerScope scope, SnapshotSpan span)
        {
            if (scope.DisabledSnapshotSpans.Any(x => IntersectsWith(x, span)))
            {
                return true;
            }

            if (scope.EditedSpans != null)
            {
                if (scope.EditedSpans.Any(x => IntersectsWith(x, span)))
                {
                    return true;
                }
            }

            return false;
        }

        public static SnapshotPoint SkipChars(this SnapshotPoint start, char value)
        {
            var currentChar = start.GetChar();

            while (currentChar == value)
            {
                start = start.Add(1);

                currentChar = start.GetChar();
            }

            return start;
        }
        #endregion

        #region Methods
        static bool IntersectsWith(SnapshotSpan a, SnapshotSpan b)
        {
            try
            {
                a = ToLine(a);
                b = ToLine(b);

                return a.IntersectsWith(b);
            }
            catch (Exception e)
            {
                Trace(e.ToString());
            }

            return false;
        }

        static SnapshotSpan ToLine(SnapshotSpan snapshotSpan)
        {
            var textSnapshotLine = snapshotSpan.Snapshot.GetLineFromPosition(snapshotSpan.Start.Position);

            var startPosition = textSnapshotLine.Start.Position;

            var endPosition = snapshotSpan.Snapshot.GetLineFromPosition(snapshotSpan.End.Position).End;

            return new SnapshotSpan(snapshotSpan.Snapshot, startPosition, endPosition - startPosition);
        }
        #endregion

        /// <summary>
        ///     Removes value from start of str
        /// </summary>
        public static string RemoveFromStart(this string data, string value)
        {
            if (data == null)
            {
                return null;
            }

            if (data.StartsWith(value, StringComparison.CurrentCulture))
            {
                return data.Substring(value.Length, data.Length - value.Length);
            }

            return data;
        }


        /// <summary>
        ///     Removes value from end of str
        /// </summary>
        public static string RemoveFromEnd(this string data, string value)
        {
            return RemoveFromEnd(data, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Removes from end.
        /// </summary>
        public static string RemoveFromEnd(this string data, string value, StringComparison comparison)
        {
            if (data.EndsWith(value, comparison))
            {
                return data.Substring(0, data.Length - value.Length);
            }

            return data;
        }
    }
}