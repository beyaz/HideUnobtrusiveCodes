using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using BOA.DataFlow;

namespace HideUnobtrusiveCodes
{
    /// <summary>
    ///     The scope
    /// </summary>
    class Scope : DataContext
    {
    }

    /// <summary>
    ///     The data key
    /// </summary>
    class DataKey<T> : BOA.DataFlow.DataKey<T>
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataKey{T}" /> class.
        /// </summary>
        public DataKey(Type locatedType, string fieldName) : base(locatedType, fieldName)
        {
        }
        #endregion
    }

    partial class Mixin
    {
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
    }
}