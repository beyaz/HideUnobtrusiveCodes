using System;
using System.Collections.Generic;
using HideUnobtrusiveCodes.Common;

namespace HideUnobtrusiveCodes.Processors.BOAResponseCheckCollapsing
{
    /// <summary>
    ///     The boa response check processor
    /// </summary>
    partial class BOAResponseCheckProcessor
    {
        public static (bool isFound, int variableAssingmentLineIndex, int endIndex, string responseVariableName)
            ProcessMultiLine(IReadOnlyList<string> lines, int startIndex)
        {
            return ProcessMultiLine(startIndex, i => i < lines.Count, i => lines[i]);
        }

        public static (bool isFound, int variableAssingmentLineIndex, int endIndex, string responseVariableName)
            ProcessMultiLine(int startIndex, Func<int, bool> canAccessLineAt, Func<int, string> readLineAt)
        {
            var defaultPadding = "    ";

            if (!canAccessLineAt(startIndex))
            {
                return default;
            }

            var successCheckLine = readLineAt(startIndex);

            var spaceCount = successCheckLine.IndexOf(c => !char.IsWhiteSpace(c));
            if (spaceCount < 4)
            {
                return default;
            }

            var padding = "".PadLeft(spaceCount, ' ');

            if (successCheckLine.StartsWith(padding + "if(!") && successCheckLine.EndsWith(".Success)"))
            {
                var responseVariableName = successCheckLine.Trim().RemoveFromStart("if(!").RemoveFromEnd(".Success)");

                if (readLineAt(startIndex + 1) == padding + "{")
                {
                    if (lineHasMatch(startIndex + 2, x => x == padding + defaultPadding + $"returnObject.Results.AddRange({responseVariableName}.Results);"))
                    {
                        if (lineHasMatch(startIndex + 3, x => x == padding + defaultPadding + "return returnObject;"))
                        {
                            if (lineHasMatch(startIndex + 4, x => x == padding + "}"))
                            {
                                var callerStartLineIndex = -1;

                                var cursor = startIndex - 1;

                                while (canAccessLineAt(cursor))
                                {
                                    if (lineHasMatch(cursor, line => line.StartsWith(padding + $"var {responseVariableName} = ")))
                                    {
                                        callerStartLineIndex = cursor;
                                        break;
                                    }

                                    cursor--;
                                }

                                if (callerStartLineIndex == -1)
                                {
                                    return default;
                                }

                                return (isFound: true, variableAssingmentLineIndex: cursor, endIndex: startIndex + 4, responseVariableName);
                            }
                        }
                    }
                }
            }

            return default;

            bool lineHasMatch(int lineIndex, Func<string, bool> checkLine)
            {
                if (canAccessLineAt(lineIndex))
                {
                    return checkLine(readLineAt(lineIndex));
                }

                return false;
            }
        }
    }
}