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
            // cursor for newt line
            var downCursor = startIndex;
            
            var upCursor = startIndex - 1;
            
            var defaultPadding = "    ";

            if (!canAccessLineAt(downCursor))
            {
                return default;
            }

            var successCheckLine = readLineAt(downCursor);

            var spaceCount = GetSpaceLengthInFront(successCheckLine);
            if (spaceCount < 4)
            {
                return default;
            }

            var padding = "".PadLeft(spaceCount, ' ');

            if (successCheckLine.StartsWith(padding + "if (!") && successCheckLine.EndsWith(".Success)"))
            {
                var responseVariableName = successCheckLine.Trim().RemoveFromStart("if (!").RemoveFromEnd(".Success)");

                downCursor++;
                
                if (readLineAt(downCursor) == padding + "{")
                {
                    downCursor++;
                    if (lineHasMatch(downCursor, x => x == padding + defaultPadding + $"returnObject.Results.AddRange({responseVariableName}.Results);"))
                    {
                        downCursor++;
                        if (lineHasMatch(downCursor, x => x == padding + defaultPadding + "return returnObject;"))
                        {
                            downCursor++;
                            if (lineHasMatch(downCursor, x => x == padding + "}"))
                            {
                                var callerStartLineIndex = -1;

                                

                                var hasVarDecleration = false;

                                // go upper
                                while (canAccessLineAt(upCursor))
                                {
                                    var line = readLineAt(upCursor);
                                    
                                    if (string.IsNullOrWhiteSpace(line))
                                    {
                                        upCursor--;
                                        continue;
                                    }
                                    if (line.StartsWith(padding + $"var {responseVariableName} = "))
                                    {
                                        hasVarDecleration    = true;
                                        callerStartLineIndex = upCursor;
                                        break;
                                    }

                                    if (line.StartsWith(padding + $"{responseVariableName} = "))
                                    {
                                        callerStartLineIndex = upCursor;
                                        break;
                                    }

                                    if (GetSpaceLengthInFront(line) >= spaceCount + defaultPadding.Length)
                                    {
                                        upCursor--;
                                        continue;
                                    }

                                    if (GetSpaceLengthInFront(line) != spaceCount)
                                    {
                                        break;
                                    }

                                    upCursor--;
                                }

                                if (callerStartLineIndex == -1)
                                {
                                    return default;
                                }
                                
                                
                                var downCursorTemp = downCursor + 1;
                                
                                // skip empty and comment lines
                                while (canAccessLineAt(downCursorTemp))
                                {
                                    var isCommment = lineHasMatch(downCursorTemp, line => line?.TrimStart().StartsWith("//") == true);
                                    var isEmptyLine = lineHasMatch(downCursorTemp, string.IsNullOrWhiteSpace);
                                    if (isCommment || isEmptyLine)
                                    {
                                        downCursorTemp++;
                                        continue;
                                    }
                                    break;
                                }

                                
                                string finalValType = null;
                                string finalValName = null;
                                string finalValExtension = null;

                                
                                if (canAccessLineAt(downCursorTemp))
                                {
                                    var line = readLineAt(downCursorTemp);

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

                                            downCursor = downCursorTemp;
                                        }
                                    }
                                }
                                

                                var sb = new StringBuilder();
                                for (var i = upCursor; i < startIndex; i++)
                                {
                                    if (string.IsNullOrWhiteSpace(readLineAt(i)))
                                    {
                                        continue;
                                    }
                                    
                                    if (i == upCursor)
                                    {
                                        if (finalValType != null)
                                        {
                                            sb.Append(finalValType);
                                            sb.Append(" ");
                                        }
                                        
                                        if (finalValName != null)
                                        {
                                            sb.Append(finalValName);
                                            sb.Append(" = ");
                                        }

                                        
                                        
                                        sb.AppendLine(readLineAt(i)
                                                          .RemoveFromStart(padding + responseVariableName + " = ")
                                                          .RemoveFromStart(padding + "var "+responseVariableName + " = "));
                                        continue;
                                    }

                                    
                                    sb.AppendLine(readLineAt(i).RemoveFromStart(padding));
                                    
                                }
                                
                                if (finalValExtension != null)
                                {
                                    while (true)
                                    {
                                        if (sb[sb.Length-1]==';' || sb[sb.Length-1]=='\n' || sb[sb.Length-1]=='\r')
                                        {
                                            sb.Remove(sb.Length - 1, 1);
                                            continue;
                                        }    
                                        break;
                                    }
                                    
                                    
                                    sb.Append(finalValExtension);
                                }

                                return new MultilineProcessOutput
                                {
                                    isFound                     = true,
                                    variableAssingmentLineIndex = upCursor,
                                    endIndex                    = downCursor,
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
            var index = line.IndexOf(c=>!char.IsWhiteSpace(c));

            if (index >= 0)
            {
                var tabCount = line.Substring(0, index).Count(c => c == '\t');
                if (tabCount > 0)
                {
                    return index - tabCount + tabCount * 4;
                }
                return index;
            }

            return line.Length;
        }
    }
}