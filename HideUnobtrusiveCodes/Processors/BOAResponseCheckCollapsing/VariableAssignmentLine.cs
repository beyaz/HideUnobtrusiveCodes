using System.Collections.Generic;
using System.Linq;
using HideUnobtrusiveCodes.Common;

namespace HideUnobtrusiveCodes.Processors.BOAResponseCheckCollapsing
{
    /// <summary>
    ///     The variable assignment line
    /// </summary>
    public class VariableAssignmentLine
    {
        #region Public Properties
        /// <summary>
        ///     Gets the assigned value.
        /// </summary>
        public string AssignedValue { get; private set; }

        /// <summary>
        ///     Gets the name of the variable.
        /// </summary>
        public string VariableName { get; private set; }

        /// <summary>
        ///     Gets the name of the variable type.
        /// </summary>
        public string VariableTypeName { get; private set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Determines whether [is response value match] [the specified current line as assignment line].
        /// </summary>
        public static bool IsResponseValueMatch(VariableAssignmentLine currentLineAsAssignmentLine, VariableAssignmentLine responseValueAssignmentToAnotherVariable)
        {
            var variableName  = currentLineAsAssignmentLine.VariableName + ".Value";
            var assignedValue = responseValueAssignmentToAnotherVariable.AssignedValue;

            if (variableName == assignedValue)
            {
                return true;
            }

            if (HasExtra(assignedValue, variableName))
            {
                var extraExtension = assignedValue.RemoveFromStart(variableName);

                if (!currentLineAsAssignmentLine.AssignedValue.EndsWith(extraExtension))
                {
                    currentLineAsAssignmentLine.AssignedValue += extraExtension;
                }

                return true;
            }

            var multipleAssignments = assignedValue.Split(' ', '=', ' ').Where(x => string.IsNullOrWhiteSpace(x) == false).ToList();
            if (multipleAssignments.Count > 0)
            {
                assignedValue = multipleAssignments.Last();

                if (variableName == assignedValue || HasExtra(assignedValue, variableName))
                {
                    var extraExtension = assignedValue.RemoveFromStart(variableName);

                    if (!currentLineAsAssignmentLine.AssignedValue.EndsWith(extraExtension))
                    {
                        currentLineAsAssignmentLine.AssignedValue += extraExtension;
                    }

                    multipleAssignments.RemoveAt(multipleAssignments.Count - 1);

                    responseValueAssignmentToAnotherVariable.VariableName += " = " + string.Join(" = ", multipleAssignments);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Parses the specified line.
        /// </summary>
        public static VariableAssignmentLine Parse(string line)
        {
            if (line == null)
            {
                return null;
            }

            var arr = line.Split('=');

            if (arr.Length < 2)
            {
                return null;
            }

            if (arr.Length > 2)
            {
                arr = new[]
                {
                    arr[0],
                    string.Join("=", arr.Skip(1).Take(arr.Length - 1))
                };
            }

            var namePart = arr[0]?.Trim().Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            VariableAssignmentLine variableAssignmentLine = null;

            if (namePart?.Length == 1)
            {
                variableAssignmentLine = new VariableAssignmentLine
                {
                    VariableName = namePart[0]
                };
            }
            else if (namePart?.Length == 2)
            {
                variableAssignmentLine = new VariableAssignmentLine
                {
                    VariableName     = namePart[1],
                    VariableTypeName = namePart[0]
                };
            }
            else
            {
                return null;
            }

            var items = new List<string>();
            for (var i = 1; i < arr.Length; i++)
            {
                items.Add(arr[i].Trim());
            }

            var valuePart = string.Join(" = ", items).Trim();
            if (string.IsNullOrWhiteSpace(valuePart))
            {
                return null;
            }

            if (valuePart.EndsWith(";") == false)
            {
                return null;
            }

            variableAssignmentLine.AssignedValue = valuePart.Substring(0, valuePart.Length - 1);

            return variableAssignmentLine;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Determines whether the specified assigned value has extra.
        /// </summary>
        static bool HasExtra(string assignedValue, string variableName)
        {
            return assignedValue.StartsWith(variableName + ".") ||
                   assignedValue.StartsWith(variableName + "?.") ||
                   assignedValue.StartsWith(variableName + " ");
        }
        #endregion
    }
}