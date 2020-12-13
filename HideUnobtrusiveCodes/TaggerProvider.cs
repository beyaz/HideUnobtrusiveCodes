﻿using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace HideUnobtrusiveCodes
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("CSharp")]
    [TagType(typeof(TagData))]
    internal sealed class TaggerProvider : ITaggerProvider
    {
        #region Public Methods
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            

            return buffer.Properties.GetOrCreateSingletonProperty(() => new Tagger(buffer)) as ITagger<T>;
        }
        #endregion
    }
}