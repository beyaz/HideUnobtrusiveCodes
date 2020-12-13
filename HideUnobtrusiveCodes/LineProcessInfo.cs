namespace HideUnobtrusiveCodes
{
    /// <summary>
    ///     The line process information
    /// </summary>
    public class LineProcessInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the type of the condition.
        /// </summary>
        public ConditionType ConditionType { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [hide all line].
        /// </summary>
        public bool HideAllLine { get; set; }

        /// <summary>
        ///     Gets or sets the new value.
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        public bool ReplaceWithCommentIcon { get; set; }

        #endregion
    }
}