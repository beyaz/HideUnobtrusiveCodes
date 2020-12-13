using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HideUnobtrusiveCodes.Processors
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ShouldReplaceTextRangeWithAnotherText()
        {
            var scope = new Scope();

            ReplaceTextRangeWithAnotherTextProcess.ProcessReplaceTextRangeWithAnotherTexts(scope);

            

        }
    }
}
