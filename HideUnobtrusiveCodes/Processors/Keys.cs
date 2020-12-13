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
    static class Keys
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
            return new DataKey<T>(typeof(Keys), propertyName);
        }
    }
}