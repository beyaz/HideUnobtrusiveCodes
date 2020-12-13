using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HideUnobtrusiveCodes.Common;
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
        public TagData Data => scope.Get(Keys.TagModel);
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