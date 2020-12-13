using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using AdornmentCache = System.Collections.Generic.Dictionary<Microsoft.VisualStudio.Text.SnapshotSpan, HideUnobtrusiveCodes.Adornment>;

namespace HideUnobtrusiveCodes
{
    class AdornmentTaggerScope
    {
        public List<SnapshotSpan> DisabledSnapshotSpans { get; } = new List<SnapshotSpan>();

        
        public  Action<TextBox> textBlockStyler{ get; set; }
        public IWpfTextView WpfTextView{ get; set; }
        public List<SnapshotSpan> editedSpans { get; } = new List<SnapshotSpan>();

        /// <summary>
        ///     The adornment cache
        /// </summary>
        public AdornmentCache adornmentCache = new AdornmentCache();
    }
}