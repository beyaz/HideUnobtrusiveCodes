namespace HideUnobtrusiveCodes.Common
{
    /// <summary>
    ///     The options model
    /// </summary>
    public class OptionsModel
    {
        #region Public Properties
        public string[] HideLineWhenLineStartsWith { get; set; }

        /// <summary>
        ///     Gets or sets the line processors.
        /// </summary>

        public OldNewPair[] ReplaceLineWithAnotherTextWhenLineContains { get; set; }

        public OldNewPair[] ReplaceTextRangeWithAnotherText { get; set; }

        public string[] ReplaceWithCommentIconWhenLineStartsWith { get; set; }
        #endregion
    }
}