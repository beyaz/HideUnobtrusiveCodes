using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace HideUnobtrusiveCodes.Tagging
{
    /// <summary>
    ///     The tag data
    /// </summary>
    internal class TagData : ITag
    {
        #region Public Properties
        public bool ShowCommentIcon { get; set; }

        /// <summary>
        ///     The span
        /// </summary>
        public SnapshotSpan Span { get; set; }

        /// <summary>
        ///     Gets the text.
        /// </summary>
        public string Text { get; set; }
        #endregion
    }
}