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
        /// <summary>
        ///     Gets or sets a value indicating whether [show comment icon].
        /// </summary>
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