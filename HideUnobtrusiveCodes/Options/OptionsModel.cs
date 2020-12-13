namespace HideUnobtrusiveCodes.Options
{
    /// <summary>
    ///     The options model
    /// </summary>
    public class OptionsModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the hide line when line starts with.
        /// </summary>
        public string[] HideLineWhenLineStartsWith { get; set; }

        /// <summary>
        ///     Gets or sets the line processors.
        /// </summary>

        public OldNewPair[] ReplaceLineWithAnotherTextWhenLineContains { get; set; }

        /// <summary>
        ///     Gets or sets the replace text range with another text.
        /// </summary>
        public OldNewPair[] ReplaceTextRangeWithAnotherText { get; set; }

        /// <summary>
        ///     Gets or sets the replace with comment icon when line starts with.
        /// </summary>
        public string[] ReplaceWithCommentIconWhenLineStartsWith { get; set; }
        #endregion
    }
}