namespace HideUnobtrusiveCodes
{
    /// <summary>
    ///     The options model
    /// </summary>
    public class OptionsModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the line processors.
        /// </summary>
   
        public OldNewPair[] ReplaceLineWithAnotherTextWhenLineContains { get; set; }
        
        public string[]ReplaceWithCommentIconWhenLineStartsWith{ get; set; }

        public OldNewPair[] ReplaceTextRangeWithAnotherText { get; set; }
        
        public string[]HideLineWhenLineStartsWith{ get; set; }

        #endregion
    }

    public class OldNewPair
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the type of the condition.
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        #endregion
    }
}