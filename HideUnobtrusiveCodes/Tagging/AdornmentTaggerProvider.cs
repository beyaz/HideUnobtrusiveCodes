using System;
using System.ComponentModel.Composition;
using HideUnobtrusiveCodes.Common;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using static HideUnobtrusiveCodes.Application.App;

namespace HideUnobtrusiveCodes.Tagging
{
    /// <summary>
    ///     The adornment tagger provider
    /// </summary>
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("CSharp")]
    [TagType(typeof(IntraTextAdornmentTag))]
    internal sealed class AdornmentTaggerProvider : IViewTaggerProvider
    {
        #region Public Methods
        /// <summary>
        ///     Creates a tag provider for the specified view and buffer.
        /// </summary>
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            Trace(nameof(CreateTagger));

            if (textView == null)
            {
                throw new ArgumentNullException("textView");
            }

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (buffer != textView.TextBuffer)
            {
                return null;
            }

            var textBlockStyler = MyUtil.GetTextBlockStyler(FormatMapService.GetEditorFormatMap(textView));

            // FormatMapService.GetEditorFormatMap("text").GetProperties("Comment")["ForegroundColor"]

            var scope = new AdornmentTaggerScope
            {
                TextBlockStyler = textBlockStyler,
                WpfTextView     = (IWpfTextView) textView
            };

            ITagAggregator<TagData> CreateTagAggregator()
            {
                AdornmentTagger.scopeStatic = scope;
                var value = BufferTagAggregatorFactoryService.CreateTagAggregator<TagData>(textView.TextBuffer);
                AdornmentTagger.scopeStatic = null;

                return value;
            }

            return AdornmentTagger.GetTagger(new Lazy<ITagAggregator<TagData>>(CreateTagAggregator), scope) as ITagger<T>;
        }
        #endregion

        #pragma warning disable 649 // "field never assigned to" -- field is set by MEF.
        /// <summary>
        ///     The buffer tag aggregator factory service
        /// </summary>
        [Import] internal IBufferTagAggregatorFactoryService BufferTagAggregatorFactoryService;

        /// <summary>
        ///     The format map service
        /// </summary>
        [Import] internal IEditorFormatMapService FormatMapService; // MEF

        #pragma warning restore 649
    }
}