using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using HideUnobtrusiveCodes.Common;
using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Tagging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace HideUnobtrusiveCodes.Processors
{
    /// <summary>
    ///     The keys
    /// </summary>
    static class Keys
    {
        #region Static Fields
        /// <summary>
        ///     The current line index
        /// </summary>
        public static readonly DataKey<int> CurrentLineIndex = CreateKey<int>();

        /// <summary>
        ///     The line count
        /// </summary>
        public static readonly DataKey<int> LineCount = CreateKey<int>();

        /// <summary>
        ///     The text snapshot lines
        /// </summary>
        public static readonly DataKey<IReadOnlyList<ITextSnapshotLine>> TextSnapshotLines = CreateKey<IReadOnlyList<ITextSnapshotLine>>();

        /// <summary>
        ///     The add tag span
        /// </summary>
        public static readonly DataKey<Action<ITagSpan<TagData>>> AddTagSpan = CreateKey<Action<ITagSpan<TagData>>>();

        /// <summary>
        ///     The get text at line
        /// </summary>
        public static readonly DataKey<Func<int, string>> GetTextAtLine = CreateKey<Func<int, string>>();

        /// <summary>
        ///     The scope assignment variable names
        /// </summary>
        public static readonly DataKey<List<string>> ScopeAssignmentVariableNames = CreateKey<List<string>>();

        /// <summary>
        ///     The option
        /// </summary>
        public static readonly DataKey<OptionsModel> Option = CreateKey<OptionsModel>();

        /// <summary>
        ///     The update text box style for visual studio
        /// </summary>
        public static readonly DataKey<Action<TextBox>> UpdateTextBoxStyleForVisualStudio = CreateKey<Action<TextBox>>();

        /// <summary>
        ///     The tag model
        /// </summary>
        public static readonly DataKey<TagData> TagModel = CreateKey<TagData>();

        /// <summary>
        ///     The on adornment clicked
        /// </summary>
        public static readonly DataKey<Action<TagData>> OnAdornmentClicked = CreateKey<Action<TagData>>();

        /// <summary>
        ///     The is any value processed
        /// </summary>
        public static readonly DataKey<bool> IsAnyValueProcessed = CreateKey<bool>();
        #endregion

        #region Methods
        /// <summary>
        ///     Creates the key.
        /// </summary>
        static DataKey<T> CreateKey<T>([CallerMemberName] string propertyName = null)
        {
            return new DataKey<T>(typeof(Keys), propertyName);
        }
        #endregion
    }
}