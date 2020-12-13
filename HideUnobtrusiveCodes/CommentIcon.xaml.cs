using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideUnobtrusiveCodes
{
    /// <summary>
    ///     Interaction logic for CommentIcon.xaml
    /// </summary>
    public partial class CommentIcon
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommentIcon" /> class.
        /// </summary>
        public CommentIcon()
        {
            InitializeComponent();
            Cursor = Cursors.Hand;
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Creates this instance.
        /// </summary>
        public static Control Create()
        {
            return new CommentIcon();
        }
        #endregion
    }
}