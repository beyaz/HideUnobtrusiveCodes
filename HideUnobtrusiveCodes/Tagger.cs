using System;
using System.Collections.Generic;
using System.Linq;
using HideUnobtrusiveCodes.BOAResponseCheckCollapsing;
using HideUnobtrusiveCodes.Processors;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static HideUnobtrusiveCodes.GlobalScopeAccess;
using static HideUnobtrusiveCodes.Mixin;

namespace HideUnobtrusiveCodes
{
    sealed class Tagger : ITagger<TagData>
    {
        #region Fields
        readonly AdornmentTaggerScope scope;
        #endregion

        #region Constructors
        public Tagger(ITextBuffer buffer)
        {
            buffer.Changed += (sender, args) => HandleBufferChanged(args);

            scope = AdornmentTagger.scopeStatic ?? throw new ArgumentNullException(nameof(AdornmentTagger.scopeStatic));
        }
        #endregion

        #region Methods
        static IEnumerable<ITextSnapshotLine> GetIntersectingLines(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                return Enumerable.Empty<ITextSnapshotLine>();
            }

            var lastVisitedLineNumber = -1;

            var snapshot = spans[0].Snapshot;

            var lines = new List<ITextSnapshotLine>();

            foreach (var span in spans)
            {
                var firstLine = snapshot.GetLineNumberFromPosition(span.Start);

                var lastLine = snapshot.GetLineNumberFromPosition(span.End);

                for (var i = Math.Max(lastVisitedLineNumber, firstLine); i <= lastLine; i++)
                {
                    lines.Add(snapshot.GetLineFromLineNumber(i));
                }

                lastVisitedLineNumber = lastLine;
            }

            return lines;
        }

        /// <summary>
        ///     Handle buffer changes. The default implementation expands changes to full lines and sends out
        ///     a <see cref="TagsChanged" /> event for these lines.
        /// </summary>
        void HandleBufferChanged(TextContentChangedEventArgs args)
        {
            if (args.Changes.Count == 0)
            {
                return;
            }

            var temp = TagsChanged;
            if (temp == null)
            {
                return;
            }

            // Combine all changes into a single span so that
            // the ITagger<>.TagsChanged event can be raised just once for a compound edit
            // with many parts.

            var snapshot = args.After;

            var start = args.Changes[0].NewPosition;
            var end   = args.Changes[args.Changes.Count - 1].NewEnd;

            var totalAffectedSpan = new SnapshotSpan(snapshot.GetLineFromPosition(start).Start, snapshot.GetLineFromPosition(end).End);

            temp(this, new SnapshotSpanEventArgs(totalAffectedSpan));
        }
        #endregion

        #region ITagger implementation
        public IEnumerable<ITagSpan<TagData>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return GetTagList(spans);
        }

        static readonly Action<Scope>[] procesList =
        {
            ScopeToVariableAssignmentProcessor.ProcessScopeToVariableAssignments,
            CommentProcessor.ProcessReplaceWithCommentIconWhenLineStartsWith,
            ReplaceLineWithAnotherTextProcess.ProcessReplaceLineWithAnotherText,
            ReplaceTextRangeWithAnotherTextProcess.ProcessReplaceTextRangeWithAnotherTexts,
            HideLineWhenLineStartsWithProcessor.ProcessHideLineWhenLineStartsWith,
            BOAResponseCheckProcessor.Process
        };

        IReadOnlyList<ITagSpan<TagData>> GetTagList(NormalizedSnapshotSpanCollection spans)
        {
            var returnList = new List<ITagSpan<TagData>>();

            var options = GlobalScope.Options;

            var snapshotLines = GetIntersectingLines(spans).ToArray();

            var length = snapshotLines.Length;

            var textAtLineFunc = GetTextAtLineFunc(snapshotLines);

            var textSnapshotLines = snapshotLines.ToList();

            var scope = new Scope
            {
                {Option, options},
                {GetTextAtLine, textAtLineFunc},
                {ScopeAssignmentVariableNames, new List<string>()},
                {TextSnapshotLines, snapshotLines},
                {CurrentLineIndex, 0}
            };
            scope.SetupGet(AddTagSpan, s => returnList.Add);
            scope.SetupGet(LineCount, c => textSnapshotLines.Count);

            Parse(scope);

            return returnList;
        }

        static void Parse(Scope scope)
        {
            var textSnapshotLines = scope.Get(TextSnapshotLines);

            var length = textSnapshotLines.Count;

            for (var i = 0; i < length;)
            {
                scope.Update(CurrentLineIndex, i);

                scope.Update(IsAnyValueProcessed, false);

                foreach (var process in procesList)
                {
                    process(scope);
                    if (scope.Get(IsAnyValueProcessed))
                    {
                        break;
                    }
                }

                var currentLineIndex = scope.Get(CurrentLineIndex);

                if (currentLineIndex == i)
                {
                    i = currentLineIndex + 1;
                }
                else
                {
                    i = currentLineIndex;
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        #endregion
    }
}