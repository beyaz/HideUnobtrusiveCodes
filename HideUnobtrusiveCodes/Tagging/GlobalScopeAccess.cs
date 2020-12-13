using HideUnobtrusiveCodes.Common;

namespace HideUnobtrusiveCodes.Tagging
{
    /// <summary>
    ///     The global scope access
    /// </summary>
    class GlobalScopeAccess
    {
        #region Static Fields
        /// <summary>
        ///     The instance
        /// </summary>
        public static readonly GlobalScopeAccess instance = new GlobalScopeAccess();
        #endregion

        #region Fields
        /// <summary>
        ///     The options
        /// </summary>
        public readonly OptionsModel Options = OptionsReader.ReadOptionsFromFile();
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes the <see cref="GlobalScopeAccess" /> class.
        /// </summary>
        static GlobalScopeAccess()
        {
        }

        /// <summary>
        ///     Prevents a default instance of the <see cref="GlobalScopeAccess" /> class from being created.
        /// </summary>
        GlobalScopeAccess()
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets the global scope.
        /// </summary>
        public static GlobalScopeAccess GlobalScope => instance;
        #endregion
    }
}