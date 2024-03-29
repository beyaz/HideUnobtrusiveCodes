﻿using System;
using System.Collections.Generic;
using System.Linq;
using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Options;
using HideUnobtrusiveCodes.Processors;
using HideUnobtrusiveCodes.Processors.BOAResponseCheckCollapsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using static HideUnobtrusiveCodes.Tagging.GlobalScopeAccess;
using static HideUnobtrusiveCodes.Common.Mixin;
using HideUnobtrusiveCodes.Application;

namespace HideUnobtrusiveCodes.Tagging
{
    sealed class TaggerContext
    {
        public OptionsModel Option { get; set; }
        
        public Func<int, bool> CanAccessLineAt{ get; set; }
        
        public Func<int, string> ReadLineAt { get; set; }
        public int CurrentLineIndex { get; set; }
        public int TotalLength { get; set; }
        public ITextSnapshotLine[] TextSnapshotLines { get; set; }
    }
    
    /// <summary>
    ///     The tagger
    /// </summary>
    sealed class Tagger : ITagger<TagData>
    {
        #region Fields
        /// <summary>
        ///     The scope
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        readonly AdornmentTaggerScope scope;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="Tagger" /> class.
        /// </summary>
        public Tagger(ITextBuffer buffer)
        {
            buffer.Changed += (sender, args) => HandleBufferChanged(args);

            scope = AdornmentTagger.scopeStatic ?? throw new ArgumentNullException(nameof(AdornmentTagger.scopeStatic));
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the intersecting lines.
        /// </summary>
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
        /// <summary>
        ///     Gets all the tags that intersect the specified spans.
        /// </summary>
        public IEnumerable<ITagSpan<TagData>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return GetTagList(spans);
        }

        /// <summary>
        ///     The proces list
        /// </summary>
        static readonly Action<Scope>[] procesList =
        {
            ScopeToVariableAssignmentProcessor.ProcessScopeToVariableAssignments,
            CommentProcessor.ProcessReplaceWithCommentIconWhenLineStartsWith,
            ReplaceLineWithAnotherTextProcess.ProcessReplaceLineWithAnotherText,
            ReplaceTextRangeWithAnotherTextProcess.ProcessReplaceTextRangeWithAnotherTexts,
            HideLineWhenLineStartsWithProcessor.ProcessHideLineWhenLineStartsWith,
            //BOAResponseCheckProcessor.Process,
            BOAResponseCheckProcessor.ProcessMultiLine
        };

        /// <summary>
        ///     Gets the tag list.
        /// </summary>
        IReadOnlyList<ITagSpan<TagData>> GetTagList(NormalizedSnapshotSpanCollection spans)
        {
            var returnList = new List<ITagSpan<TagData>>();

            var options = GlobalScope.Options;

            var snapshotLines = GetIntersectingLines(spans).ToArray();


            var textAtLineFunc = GetTextAtLineFunc(snapshotLines);

            var taggerContext = new TaggerContext
            {
                Option            = options,
                CanAccessLineAt   = i => i >=0 && i < snapshotLines.Length,
                ReadLineAt        = textAtLineFunc,
                CurrentLineIndex  = 0,
                TotalLength       = snapshotLines.Length,
                TextSnapshotLines = snapshotLines
            };

            return RunAll(taggerContext, ReplaceIgnoreLinesWithCommentIcon, ResponseCheck);
            
            /*
            var textSnapshotLines = snapshotLines.ToList();
            
            
            

            var scope = new Scope
            {
                {Keys.Option, options},
                {Keys.GetTextAtLine, textAtLineFunc},
                {Keys.ScopeAssignmentVariableNames, new List<string>()},
                {Keys.TextSnapshotLines, snapshotLines},
                {Keys.CurrentLineIndex, 0}
            };
            scope.SetupGet(Keys.AddTagSpan, s => returnList.Add);
            scope.SetupGet(Keys.LineCount, c => textSnapshotLines.Count);

           

            Parse(scope);

            return returnList;*/
        }

        static ITagSpan<TagData> ResponseCheck(TaggerContext taggerContext)
        {
            var response = BOAResponseCheckProcessorMultiline.ProcessMultiLine(taggerContext.CurrentLineIndex, taggerContext.CanAccessLineAt, i=>taggerContext.ReadLineAt(i)?.Replace("\t","    "));
            if (response?.isFound == true)
            {
                var start = taggerContext.TextSnapshotLines[response.variableAssingmentLineIndex].Start.Add(GetFirstCharIndexHasValue(taggerContext.ReadLineAt(response.variableAssingmentLineIndex)));
                var end = taggerContext.TextSnapshotLines[response.endIndex].End;
                
                var span = new SnapshotSpan(start, end);
                var tag  = new TagData {Text = response.summary, Span = span};

                taggerContext.CurrentLineIndex = response.endIndex + 1;

                return new TagSpan<TagData>(span, tag);

            }

            return null;
        }
        
        static ITagSpan<TagData> ReplaceIgnoreLinesWithCommentIcon(TaggerContext taggerContext)
        {
            var currentLineIndex = taggerContext.CurrentLineIndex;

            var canAccessLineAt = taggerContext.CanAccessLineAt;

            string readLineAt(int i)
            {
                return taggerContext.ReadLineAt(i)?.Replace("\t", "    ");
            }

            bool isCommentLine(string line)
            {
                return CanIgnoreLine(line, taggerContext.Option.ReplaceWithCommentIconWhenLineStartsWith);
            }

            var response = CommentProcessor.Process(currentLineIndex, canAccessLineAt, readLineAt, isCommentLine);
            if (response == default)
            {
                return default;
            }

            var tagSpan = CommentTagger.ConvertCommentToTagSpan(taggerContext, response.startLineIndex, response.endLineIndex);

            taggerContext.CurrentLineIndex = response.endLineIndex + 1;

            return tagSpan;
            
            
        }
        
        static bool CanIgnoreLine(string line, IReadOnlyList<string> ignorePrefixList)
        {
            foreach (var prefix in ignorePrefixList)
            {
                if (line?.TrimStart().StartsWith(prefix,StringComparison.OrdinalIgnoreCase)== true)
                {
                    return true;
                }
            }

            return false;
        }
        
        
        static IReadOnlyList<ITagSpan<TagData>> RunAll(TaggerContext taggerContext, params Func<TaggerContext, ITagSpan<TagData>>[] funcList)
        {
            var returnList = new List<ITagSpan<TagData>>();
            
            var length = taggerContext.TotalLength;

            for (var i = 0; i < length;)
            {
                taggerContext.CurrentLineIndex = i;
                
                foreach (var func in funcList)
                {
                    var tagSpan = func(taggerContext);
                    if (tagSpan != null)
                    {
                        returnList.Add(tagSpan);
                        i = taggerContext.CurrentLineIndex;
                        break;
                    }
                }

                if (taggerContext.CurrentLineIndex == i)
                {
                    i = taggerContext.CurrentLineIndex + 1;
                }
                else
                {
                    i = taggerContext.CurrentLineIndex;
                }
            }

            return returnList;
        }
        

        /// <summary>
        ///     Parses the specified scope.
        /// </summary>
        static void Parse(Scope scope)
        {
            var textSnapshotLines = scope.Get(Keys.TextSnapshotLines);

            var length = textSnapshotLines.Count;

            for (var i = 0; i < length;)
            {
                scope.Update(Keys.CurrentLineIndex, i);

                scope.Update(Keys.IsAnyValueProcessed, false);

                foreach (var process in procesList)
                {
                    process(scope);
                    if (scope.Get(Keys.IsAnyValueProcessed))
                    {
                        break;
                    }
                }

                var currentLineIndex = scope.Get(Keys.CurrentLineIndex);

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

        /// <summary>
        ///     Occurs when tags are added to or removed from the provider.
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        #endregion
    }
}