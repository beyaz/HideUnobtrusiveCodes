using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static HideUnobtrusiveCodes.Mixin;

namespace HideUnobtrusiveCodes.Processors
{
    class ReplaceLineWithAnotherTextProcess
    {
        #region Public Methods
        public static void ProcessReplaceLineWithAnotherText(Scope scope)
        {
            var getTextAtLine     = scope.Get(GetTextAtLine);
            var textSnapshotLines = scope.Get(TextSnapshotLines);
            var currentLineIndex  = scope.Get(CurrentLineIndex);
            var options           = scope.Get(Option);

            foreach (var item in options.ReplaceLineWithAnotherTextWhenLineContains)
            {
                if (LineContains(scope, currentLineIndex, item.Value))
                {
                    scope.Update(IsAnyValueProcessed,true);

                    var indexOfFirstChar = GetFirstCharIndexHasValue(getTextAtLine(currentLineIndex));

                    var startPoint = textSnapshotLines[currentLineIndex].Start.Add(indexOfFirstChar);

                    var end = textSnapshotLines[currentLineIndex].End;

                    var span = new SnapshotSpan(startPoint, end);

                    var tag = new TagData {Text = item.NewValue, Span = span};

                    scope.Get(AddTagSpan)(new TagSpan<TagData>(span, tag));

                    // focus to next not processed position
                    scope.Update(CurrentLineIndex, currentLineIndex + 1);
                }
            }
        }
        #endregion
    }
}