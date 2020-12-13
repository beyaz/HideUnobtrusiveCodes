using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace HideUnobtrusiveCodes
{
    /// <summary>
    ///     The tag data
    /// </summary>
    internal class TagData : ITag
    {
        #region Public Properties
        /// <summary>
        ///     The span
        /// </summary>
        public SnapshotSpan Span { get; set; }

        /// <summary>
        ///     Gets the text.
        /// </summary>
        public string Text { get; set; }

        public bool ShowCommentIcon { get; set; }
        #endregion
    }
}