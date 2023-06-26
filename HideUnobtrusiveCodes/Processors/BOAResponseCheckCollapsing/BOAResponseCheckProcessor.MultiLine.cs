using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HideUnobtrusiveCodes.Common;

namespace HideUnobtrusiveCodes.Processors.BOAResponseCheckCollapsing
{
    class MultilineProcessOutput
    {
        public int endIndex { get; set; }
        public bool hasVarDecleration { get; set; }
        public bool isFound { get; set; }
        public string responseVariableName { get; set; }
        public string summary { get; set; }
        public int variableAssingmentLineIndex { get; set; }
        public string finalValType { get; set; }
        public string finalValName { get; set; }
        public string finalValExtension { get; set; }
    }

    static class BOAResponseCheckProcessorMultiline
    {
        public static MultilineProcessOutput ProcessMultiLine(IReadOnlyList<string> lines, int startIndex)
        {
            return ProcessMultiLine(startIndex, i => i>=0 && i < lines.Count, i => lines[i]);
        }

        public static MultilineProcessOutput ProcessMultiLine(int startIndex, Func<int, bool> canAccessLineAt, Func<int, string> readLineAt)
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

            if (successCheckLine.StartsWith(padding + "if (!") && successCheckLine.EndsWith(".Success)"))
            {
                var responseVariableName = successCheckLine.Trim().RemoveFromStart("if (!").RemoveFromEnd(".Success)");

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

                                // go upper
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

                                    if (GetSpaceLengthInFront(readLineAt(cursor)) == spaceCount + defaultPadding.Length)
                                    {
                                        cursor--;
                                        continue;
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
                                
                                // go down
                                var cursorForDown = startIndex + 5;
                                
                                // skip empty and comment lines
                                while (canAccessLineAt(cursorForDown))
                                {
                                    var isCommment = lineHasMatch(cursorForDown, line => line?.TrimStart().StartsWith("//") == true);
                                    var isEmptyLine = lineHasMatch(cursorForDown, string.IsNullOrWhiteSpace);
                                    if (isCommment || isEmptyLine)
                                    {
                                        cursorForDown++;
                                        continue;
                                    }
                                    break;
                                }


                                string finalValType = null;
                                string finalValName = null;
                                string finalValExtension = null;
                                
                                if (canAccessLineAt(cursorForDown))
                                {
                                    var line = readLineAt(cursorForDown);

                                    var index = line.IndexOf($"= {responseVariableName}.Value", StringComparison.OrdinalIgnoreCase);
                                    if (index > 0)
                                    {
                                        var result = line.Substring(0, index).Trim();

                                        var firstSpaceIndex = result.IndexOf(' ');
                                        if (firstSpaceIndex > 1)
                                        {
                                            finalValType = result.Substring(0, firstSpaceIndex);

                                            finalValName = result.Substring(firstSpaceIndex).Trim();
                                            
                                            finalValExtension = line.Substring(index + $"= {responseVariableName}.Value".Length);
                                            if (finalValExtension==";")
                                            {
                                                finalValExtension = null;
                                            }
                                        }
                                    }
                                }
                                

                                var sb = new StringBuilder();
                                for (var i = cursor; i < startIndex; i++)
                                {
                                    if (i == cursor)
                                    {
                                        if (finalValType != null)
                                        {
                                            sb.Append(finalValType);
                                            sb.Append(" ");
                                        }
                                        
                                        if (finalValName != null)
                                        {
                                            sb.Append(finalValName);
                                        }

                                        sb.Append(" = ");
                                        
                                        sb.AppendLine(readLineAt(i)
                                                          .RemoveFromStart(padding + responseVariableName + " = ")
                                                          .RemoveFromStart(padding + "var "+responseVariableName + " = "));
                                        continue;
                                    }

                                    
                                    
                                    
                                    if (i == startIndex-1)
                                    {
                                        if (finalValExtension != null)
                                        {
                                            sb.AppendLine(readLineAt(i).RemoveFromEnd(";") + finalValExtension);
                                            continue;
                                        }
                                    }
                                    
                                    sb.AppendLine(readLineAt(i));
                                    
                                }

                                return new MultilineProcessOutput
                                {
                                    isFound                     = true,
                                    variableAssingmentLineIndex = cursor,
                                    endIndex                    = startIndex + 4,
                                    responseVariableName        = responseVariableName,
                                    hasVarDecleration           = hasVarDecleration,
                                    summary                     = sb.ToString().Trim(),
                                    
                                    finalValType =finalValType, 
                                    finalValName = finalValName, 
                                    finalValExtension = finalValExtension
                                };
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
            var index = line.IndexOf(c => !char.IsWhiteSpace(c));

            if (index >= 0)
            {
                return index;
            }

            return line.Length;
        }
    }
}