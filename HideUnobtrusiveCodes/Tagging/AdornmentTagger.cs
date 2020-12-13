using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Processors;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using AdornmentCache = System.Collections.Generic.Dictionary<Microsoft.VisualStudio.Text.SnapshotSpan, HideUnobtrusiveCodes.Tagging.Adornment>;

namespace HideUnobtrusiveCodes.Tagging
{
    /// <summary>
    ///     The adornment tagger
    /// </summary>
    sealed class AdornmentTagger : ITagger<IntraTextAdornmentTag>, IDisposable
    {
        #region Static Fields
        /// <summary>
        ///     The scope static
        /// </summary>
        internal static AdornmentTaggerScope scopeStatic;
        #endregion

        #region Fields
        /// <summary>
        ///     The data tagger
        /// </summary>
        readonly ITagAggregator<TagData> dataTagger;

        /// <summary>
        ///     The invalidated spans
        /// </summary>
        readonly List<SnapshotSpan> invalidatedSpans = new List<SnapshotSpan>();

        /// <summary>
        ///     The scope
        /// </summary>
        readonly AdornmentTaggerScope scope;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="AdornmentTagger" /> class.
        /// </summary>
        AdornmentTagger(ITagAggregator<TagData> dataTagger, AdornmentTaggerScope scope)
        {
            this.scope = scope;

            snapshot = view.TextBuffer.CurrentSnapshot;

            view.LayoutChanged      += HandleLayoutChanged;
            view.TextBuffer.Changed += HandleBufferChanged;

            this.dataTagger = dataTagger;

            this.dataTagger.TagsChanged += HandleDataTagsChanged;
        }
        #endregion

        #region Public Events
        /// <summary>
        ///     Occurs when tags are added to or removed from the provider.
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        #endregion

        #region Properties
        /// <summary>
        ///     Gets or sets the snapshot.
        /// </summary>
        ITextSnapshot snapshot { get; set; }

        /// <summary>
        ///     The view
        /// </summary>
        IWpfTextView view => scope.WpfTextView;
        #endregion

        #region Public Methods
        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            view.Properties.RemoveProperty(typeof(AdornmentTagger));
        }

        // Produces tags on the snapshot that the tag consumer asked for.
        /// <summary>
        ///     Gets all the tags that intersect the specified spans.
        /// </summary>
        public IEnumerable<ITagSpan<IntraTextAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null || spans.Count == 0)
            {
                return Enumerable.Empty<ITagSpan<IntraTextAdornmentTag>>();
            }

            TranslateDisabledSnapshots();

            var requestedSnapshot = spans[0].Snapshot;

            var translatedSpans = new NormalizedSnapshotSpanCollection(spans.Select(span => span.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive)));

            // Grab the adornments.
            var tags = new List<ITagSpan<IntraTextAdornmentTag>>();
            foreach (var tagSpan in GetAdornmentTagsOnSnapshot(translatedSpans))
            {
                // Translate each adornment to the snapshot that the tagger was asked about.
                var span = tagSpan.Span.TranslateTo(requestedSnapshot, SpanTrackingMode.EdgeExclusive);

                if (Mixin.IsIntersectWithDisabledSpans(scope, span))
                {
                    continue;
                }

                var tag = new IntraTextAdornmentTag(tagSpan.Tag.Adornment, tagSpan.Tag.RemovalCallback, tagSpan.Tag.Affinity);

                tags.Add(new TagSpan<IntraTextAdornmentTag>(span, tag));
            }

            return tags;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the tagger.
        /// </summary>
        internal static ITagger<IntraTextAdornmentTag> GetTagger(Lazy<ITagAggregator<TagData>> tagger, AdornmentTaggerScope scope)
        {
            return scope.WpfTextView.Properties.GetOrCreateSingletonProperty(() => new AdornmentTagger(tagger.Value, scope));
        }

        /// <summary>
        ///     Gets the adornment data.
        /// </summary>
        static IReadOnlyList<Tuple<SnapshotSpan, PositionAffinity?, TagData>> GetAdornmentData(NormalizedSnapshotSpanCollection spans, Func<IEnumerable<IMappingTagSpan<TagData>>> getTags)
        {
            var returnValues = new List<Tuple<SnapshotSpan, PositionAffinity?, TagData>>();

            if (spans.Count == 0)
            {
                return returnValues;
            }

            var snapshot = spans[0].Snapshot;

            foreach (var dataTagSpan in getTags())
            {
                var dataTagSpans = dataTagSpan.Span.GetSpans(snapshot);

                // Ignore data tags that are split by projection.
                // This is theoretically possible but unlikely in current scenarios.
                if (dataTagSpans.Count != 1)
                {
                    continue;
                }

                var span = dataTagSpans[0];

                var adornmentAffinity = new PositionAffinity?(PositionAffinity.Successor);

                var affinity = span.Length > 0 ? null : adornmentAffinity;

                returnValues.Add(Tuple.Create(span, affinity, dataTagSpan.Tag));
            }

            return returnValues;
        }

        /// <summary>
        ///     Translates the adornment cache.
        /// </summary>
        static AdornmentCache TranslateAdornmentCache(ITextSnapshot targetSnapshot, AdornmentCache adornmentCache)
        {
            var translatedAdornmentCache = new AdornmentCache();

            foreach (var pair in adornmentCache)
            {
                var key       = pair.Key.TranslateTo(targetSnapshot, SpanTrackingMode.EdgeExclusive);
                var adornment = pair.Value;

                adornment.Data.Span = key;

                translatedAdornmentCache.Add(key, adornment);
            }

            return translatedAdornmentCache;
        }

        /// <summary>
        ///     Asynchronouses the update.
        /// </summary>
        void AsyncUpdate()
        {
            // Store the snapshot that we're now current with and send an event
            // for the text that has changed.
            if (snapshot != view.TextBuffer.CurrentSnapshot)
            {
                snapshot = view.TextBuffer.CurrentSnapshot;

                scope.AdornmentCache = TranslateAdornmentCache(snapshot, scope.AdornmentCache);
            }

            List<SnapshotSpan> translatedSpans;
            lock (invalidatedSpans)
            {
                translatedSpans = invalidatedSpans.Select(s => s.TranslateTo(snapshot, SpanTrackingMode.EdgeInclusive)).ToList();
                invalidatedSpans.Clear();
            }

            if (translatedSpans.Count == 0)
            {
                return;
            }

            scope.EditedSpans.AddRange(translatedSpans);

            var start = translatedSpans.Select(span => span.Start).Min();
            var end   = translatedSpans.Select(span => span.End).Max();

            start = snapshot.GetLineFromPosition(start).Start;
            end   = snapshot.GetLineFromPosition(end).End;

            RaiseTagsChanged(new SnapshotSpan(start, end));
        }

        // Produces tags on the snapshot that this tagger is current with.
        /// <summary>
        ///     Gets the adornment tags on snapshot.
        /// </summary>
        IReadOnlyList<TagSpan<IntraTextAdornmentTag>> GetAdornmentTagsOnSnapshot(NormalizedSnapshotSpanCollection spans)
        {
            var returnList = new List<TagSpan<IntraTextAdornmentTag>>();

            var adornmentCache = scope.AdornmentCache;

            if (spans.Count == 0)
            {
                return returnList;
            }

            var snapshot = spans[0].Snapshot;

            Debug.Assert(snapshot == this.snapshot);

            // Since WPF UI objects have state (like mouse hover or animation) and are relatively expensive to create and lay out,
            // this code tries to reuse controls as much as possible.
            // The controls are stored in this.adornmentCache between the calls.

            // Mark which adornments fall inside the requested spans with Keep=false
            // so that they can be removed from the cache if they no longer correspond to data tags.
            var toRemove = new HashSet<SnapshotSpan>();
            foreach (var pair in adornmentCache)
            {
                if (Mixin.HasIntersection(spans, new NormalizedSnapshotSpanCollection(pair.Key.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive))))
                {
                    toRemove.Add(pair.Key);
                }
            }

            foreach (var spanDataPair in GetAdornmentData(spans, () => dataTagger.GetTags(spans)).Distinct(new Comparer()))
            {
                // Look up the corresponding adornment or create one if it's new.
                var snapshotSpan  = spanDataPair.Item1;
                var affinity      = spanDataPair.Item2;
                var adornmentData = spanDataPair.Item3;

                var adornment = GetOrCreateAdornment(adornmentCache, snapshotSpan, adornmentData, toRemove);

                returnList.Add(new TagSpan<IntraTextAdornmentTag>(snapshotSpan, new IntraTextAdornmentTag(adornment, null, affinity)));
            }

            foreach (var snapshotSpan in toRemove)
            {
                adornmentCache.Remove(snapshotSpan);
            }

            return returnList;
        }

        /// <summary>
        ///     Gets the or create adornment.
        /// </summary>
        UIElement GetOrCreateAdornment(AdornmentCache adornmentCache, SnapshotSpan snapshotSpan, TagData tagData, HashSet<SnapshotSpan> toRemove)
        {
            Adornment adornment;
            if (adornmentCache.TryGetValue(snapshotSpan, out adornment))
            {
                if (UpdateAdornment(adornment, tagData))
                {
                    toRemove.Remove(snapshotSpan);
                }
            }
            else
            {
                var adornmentScope = new Scope
                {
                    {Keys.TagModel, tagData},
                    {Keys.OnAdornmentClicked, OnAdornmentClicked},
                    {Keys.UpdateTextBoxStyleForVisualStudio, scope.TextBlockStyler}
                };
                adornment = new Adornment(adornmentScope);

                // Get the adornment to measure itself. Its DesiredSize property is used to determine
                // how much space to leave between text for this adornment.
                // Note: If the size of the adornment changes, the line will be reformatted to accommodate it.
                // Note: Some adornments may change size when added to the view's visual tree due to inherited
                // dependency properties that affect layout. Such options can include SnapsToDevicePixels,
                // UseLayoutRounding, TextRenderingMode, TextHintingMode, and TextFormattingMode. Making sure
                // that these properties on the adornment match the view's values before calling Measure here
                // can help avoid the size change and the resulting unnecessary re-format.
                adornment.UIElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                adornmentCache.Add(snapshotSpan, adornment);
            }

            return adornment.UIElement;
        }

        /// <summary>
        ///     Handles the buffer changed.
        /// </summary>
        void HandleBufferChanged(object sender, TextContentChangedEventArgs args)
        {
            var editedSpans = args.Changes.Select(change => new SnapshotSpan(args.After, change.NewSpan)).ToList();

            InvalidateSpans(editedSpans);
        }

        /// <summary>
        ///     Handles the data tags changed.
        /// </summary>
        void HandleDataTagsChanged(object sender, TagsChangedEventArgs args)
        {
            var changedSpans = args.Span.GetSpans(view.TextBuffer.CurrentSnapshot);
            InvalidateSpans(changedSpans);
        }

        /// <summary>
        ///     Handles the layout changed.
        /// </summary>
        void HandleLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            var adornmentCache = scope.AdornmentCache;

            var visibleSpan = view.TextViewLines.FormattedSpan;

            // Filter out the adornments that are no longer visible.
            var toRemove = new List<SnapshotSpan>(from keyValuePair in adornmentCache
                                                  where !keyValuePair.Key.TranslateTo(visibleSpan.Snapshot, SpanTrackingMode.EdgeExclusive).IntersectsWith(visibleSpan)
                                                  select keyValuePair.Key);

            foreach (var span in toRemove)
            {
                adornmentCache.Remove(span);
            }
        }

        /// <summary>
        ///     Causes intra-text adornments to be updated asynchronously.
        /// </summary>
        void InvalidateSpans(IList<SnapshotSpan> spans)
        {
            lock (invalidatedSpans)
            {
                var wasEmpty = invalidatedSpans.Count == 0;

                invalidatedSpans.AddRange(spans);

                if (wasEmpty && invalidatedSpans.Count > 0)
                {
                    view.VisualElement.Dispatcher.BeginInvoke(new Action(AsyncUpdate));
                }
            }
        }

        /// <summary>
        ///     Called when [adornment clicked].
        /// </summary>
        void OnAdornmentClicked(TagData tagData)
        {
            scope.DisabledSnapshotSpans.Add(tagData.Span);

            InvalidateSpans(new List<SnapshotSpan> {tagData.Span});
        }

        /// <summary>
        ///     Causes intra-text adornments to be updated synchronously.
        /// </summary>
        void RaiseTagsChanged(SnapshotSpan span)
        {
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
        }

        /// <summary>
        ///     Translates the disabled snapshots.
        /// </summary>
        void TranslateDisabledSnapshots()
        {
            TranslateToCurrentSnapshot(scope.DisabledSnapshotSpans);
            TranslateToCurrentSnapshot(scope.EditedSpans);
        }

        /// <summary>
        ///     Translates to current snapshot.
        /// </summary>
        void TranslateToCurrentSnapshot(List<SnapshotSpan> spans)
        {
            if (spans == null)
            {
                return;
            }

            for (var i = 0; i < spans.Count; i++)
            {
                spans[i] = spans[i].TranslateTo(view.TextBuffer.CurrentSnapshot, SpanTrackingMode.EdgeExclusive);
            }
        }

        /// <summary>
        ///     Updates the adornment.
        /// </summary>
        bool UpdateAdornment(Adornment adornment, TagData dataTagData)
        {
            adornment.Update(dataTagData);
            return true;
        }
        #endregion

        /// <summary>
        ///     The comparer
        /// </summary>
        class Comparer : IEqualityComparer<Tuple<SnapshotSpan, PositionAffinity?, TagData>>
        {
            #region Public Methods
            /// <summary>
            ///     Equalses the specified x.
            /// </summary>
            public bool Equals(Tuple<SnapshotSpan, PositionAffinity?, TagData> x, Tuple<SnapshotSpan, PositionAffinity?, TagData> y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                return x.Item1.Equals(y.Item1);
            }

            /// <summary>
            ///     Returns a hash code for this instance.
            /// </summary>
            public int GetHashCode(Tuple<SnapshotSpan, PositionAffinity?, TagData> obj)
            {
                return obj.Item1.GetHashCode();
            }
            #endregion
        }
    }
}