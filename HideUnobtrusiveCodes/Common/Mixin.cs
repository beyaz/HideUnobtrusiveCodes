using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Returns the index of the first element in the sequence 
        /// that satisfies a condition.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IEnumerable{T}"/> that contains
        /// the elements to apply the predicate to.
        /// </param>
        /// <param name="predicate">
        /// A function to test each element for a condition.
        /// </param>
        /// <returns>
        /// The zero-based index position of the first element of <paramref name="source"/>
        /// for which <paramref name="predicate"/> returns <see langword="true"/>;
        /// or -1 if <paramref name="source"/> is empty
        /// or no element satisfies the condition.
        /// </returns>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, 
            Func<TSource, bool> predicate)
        {
            int i = 0;

            foreach (TSource element in source)
            {
                if (predicate(element))
                    return i;

                i++;
            }

            return -1;
        }
        
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
            if (values == null)
            {
                return false;
            }

            var getTextAtLine = scope.Get(Keys.GetTextAtLine);

            var line = getTextAtLine(lineIndex);
            if (line == null)
            {
                return false;
            }

            line = line.TrimStart();

            foreach (var value in values)
            {
                if (line?.StartsWith(value) == true)
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