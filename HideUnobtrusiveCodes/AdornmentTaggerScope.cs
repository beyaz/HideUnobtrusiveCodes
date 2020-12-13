using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using AdornmentCache = System.Collections.Generic.Dictionary<Microsoft.VisualStudio.Text.SnapshotSpan, HideUnobtrusiveCodes.Adornment>;

namespace HideUnobtrusiveCodes
{
    /// <summary>
    ///     The adornment tagger scope
    /// </summary>
    class AdornmentTaggerScope
    {
        #region Public Properties
        /// <summary>
        ///     The adornment cache
        /// </summary>
        public AdornmentCache AdornmentCache { get; set; } = new AdornmentCache();

        /// <summary>
        ///     Gets the disabled snapshot spans.
        /// </summary>
        public List<SnapshotSpan> DisabledSnapshotSpans { get; } = new List<SnapshotSpan>();

        /// <summary>
        ///     Gets the edited spans.
        /// </summary>
        public List<SnapshotSpan> EditedSpans { get; } = new List<SnapshotSpan>();

        /// <summary>
        ///     Gets or sets the text block styler.
        /// </summary>
        public Action<TextBox> TextBlockStyler { get; set; }

        /// <summary>
        ///     Gets or sets the WPF text view.
        /// </summary>
        public IWpfTextView WpfTextView { get; set; }
        #endregion
    }
}