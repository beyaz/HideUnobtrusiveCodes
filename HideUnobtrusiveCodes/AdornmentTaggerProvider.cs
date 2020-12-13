using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace HideUnobtrusiveCodes
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("CSharp")]
    [TagType(typeof(IntraTextAdornmentTag))]
    internal sealed class AdornmentTaggerProvider : IViewTaggerProvider
    {
        #region Public Methods
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
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

            Action<TextBox> textBlockStyler = MyUtil.GetTextBlockStyler(FormatMapService.GetEditorFormatMap(textView));

            // FormatMapService.GetEditorFormatMap("text").GetProperties("Comment")["ForegroundColor"]
            
            var scope = new AdornmentTaggerScope()
            {
                textBlockStyler = textBlockStyler,
                WpfTextView = (IWpfTextView) textView
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
        [Import] internal IBufferTagAggregatorFactoryService BufferTagAggregatorFactoryService;

        [Import] internal IEditorFormatMapService FormatMapService; // MEF

        #pragma warning restore 649
    }
}