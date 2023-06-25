using System;
using System.Collections.Generic;
using HideUnobtrusiveCodes.Common;

namespace HideUnobtrusiveCodes.Processors.BOAResponseCheckCollapsing
{
    static class BOAResponseCheckProcessorMultiline
    {
        public static
            (bool isFound,
            int variableAssingmentLineIndex,
            int endIndex,
            string responseVariableName,
            bool hasVarDecleration)
            ProcessMultiLine(IReadOnlyList<string> lines, int startIndex)
        {
            return ProcessMultiLine(startIndex, i => i < lines.Count, i => lines[i]);
        }

        public static
            (bool isFound,
            int variableAssingmentLineIndex,
            int endIndex,
            string responseVariableName,
            bool hasVarDecleration)
            ProcessMultiLine(int startIndex, Func<int, bool> canAccessLineAt, Func<int, string> readLineAt)
        {
            var defaultPadding = "    ";

            if (!canAccessLineAt(startIndex))
            {
                return default;
            }

            var successCheckLine = readLineAt(startIndex);

            var spaceCount = GetSpaceLengthInFront(successCheckLine);
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

                                var hasVarDecleration = false;

                                while (canAccessLineAt(cursor))
                                {
                                    if (lineHasMatch(cursor, line => line.StartsWith(padding + $"var {responseVariableName} = ")))
                                    {
                                        hasVarDecleration    = true;
                                        callerStartLineIndex = cursor;
                                        break;
                                    }

                                    if (lineHasMatch(cursor, line => line.StartsWith(padding + $"{responseVariableName} = ")))
                                    {
                                        callerStartLineIndex = cursor;
                                        break;
                                    }

                                    if (GetSpaceLengthInFront(readLineAt(cursor)) != spaceCount)
                                    {
                                        break;
                                    }

                                    cursor--;
                                }

                                if (callerStartLineIndex == -1)
                                {
                                    return default;
                                }

                                return
                                (
                                    isFound: true,
                                    variableAssingmentLineIndex: cursor,
                                    endIndex: startIndex + 4,
                                    responseVariableName,
                                    hasVarDecleration
                                );
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

        static int GetSpaceLengthInFront(string line)
        {
            return line.IndexOf(c => !char.IsWhiteSpace(c));
        }
    }
}