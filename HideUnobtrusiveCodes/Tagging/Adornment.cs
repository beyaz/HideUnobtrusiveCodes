using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HideUnobtrusiveCodes.Dataflow;
using HideUnobtrusiveCodes.Processors;

namespace HideUnobtrusiveCodes.Tagging
{
    /// <summary>
    ///     The color adornment
    /// </summary>
    sealed class Adornment
    {
        #region Fields
        /// <summary>
        ///     The scope
        /// </summary>
        readonly Scope scope;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="Adornment" /> class.
        /// </summary>
        internal Adornment(Scope scope)
        {
            this.scope = scope;

            UIElement = CreateElement(scope);
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets the data.
        /// </summary>
        public TagData Data => scope.Get(Keys.TagModel);

        /// <summary>
        ///     Gets the UI element.
        /// </summary>
        public Control UIElement { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        ///     Updates the specified data.
        /// </summary>
        internal void Update(TagData data)
        {
            var updateTextBoxStyleForVisualStudio = scope.Get(Keys.UpdateTextBoxStyleForVisualStudio);

            var textBox = UIElement as TextBox;
            if (textBox != null)
            {
                textBox.Text = data.Text;

                updateTextBoxStyleForVisualStudio(textBox);
            }
        }

        /// <summary>
        ///     Creates the element.
        /// </summary>
        static Control CreateElement(Scope scope)
        {
            var tagData            = scope.Get(Keys.TagModel);
            var onAdornmentClicked = scope.Get(Keys.OnAdornmentClicked);

            if (tagData.ShowCommentIcon)
            {
                var el = CommentIcon.Create();
                el.MouseDoubleClick += (s, e) => { onAdornmentClicked?.Invoke(scope.Get(Keys.TagModel)); };
                return el;
            }

            return CreateTextBox(scope);
        }

        /// <summary>
        ///     Creates the text box.
        /// </summary>
        static TextBox CreateTextBox(Scope scope)
        {
            var tagData                           = scope.Get(Keys.TagModel);
            var onAdornmentClicked                = scope.Get(Keys.OnAdornmentClicked);
            var updateTextBoxStyleForVisualStudio = scope.Get(Keys.UpdateTextBoxStyleForVisualStudio);

            var element = new TextBox
            {
                Background      = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Text            = tagData.Text
            };

            updateTextBoxStyleForVisualStudio(element);

            element.MouseDoubleClick += (s, e) => { onAdornmentClicked?.Invoke(scope.Get(Keys.TagModel)); };

            return element;
        }
        #endregion
    }
}