using System;
using System.Linq;
using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Processors;
using Microsoft.VisualStudio.Text;

namespace HideUnobtrusiveCodes.Common
{
    /// <summary>
    ///     The mixin
    /// </summary>
    static class Mixin
    {
        #region Public Methods
        /// <summary>
        ///     Gets the first character index has value.
        /// </summary>
        public static int GetFirstCharIndexHasValue(string value)
        {
            return value.IndexOf(value.TrimStart().First().ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Gets the text at line function.
        /// </summary>
        public static Func<int, string> GetTextAtLineFunc(ITextSnapshotLine[] lines)
        {
            var length = lines.Length;

            return requestedLine =>
            {
                if (requestedLine < length)
                {
                    return lines[requestedLine].GetText();
                }

                return null;
            };
        }

        /// <summary>
        ///     Lines the contains.
        /// </summary>
        public static bool LineContains(Func<int, string> getTextAtline, int lineIndex, string value)
        {
            return getTextAtline(lineIndex)?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        ///     Lines the starts with.
        /// </summary>
        public static bool LineStartsWith(Func<int, string> getTextAtline, int lineIndex, string value)
        {
            return getTextAtline(lineIndex)?.TrimStart().StartsWith(value) ?? false;
        }

        /// <summary>
        ///     Lines the starts with.
        /// </summary>
        public static bool LineStartsWith(Scope scope, int lineIndex, string[] values)
        {
            var getTextAtLine = scope.Get(Keys.GetTextAtLine);

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
        ///     Skips the chars.
        /// </summary>
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
    }
}