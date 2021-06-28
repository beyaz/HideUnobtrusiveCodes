using System;
using HideUnobtrusiveCodes.Common;
using HideUnobtrusiveCodes.Options;

namespace HideUnobtrusiveCodes.Application
{
    static class App
    {
        #region Static Fields
        public static readonly OptionsModel Options = SafeExecute(OptionsReader.ReadOptionsFromFile);
        #endregion

        #region Public Methods
        public static T SafeExecute<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                MyUtil.Log(e);
            }

            return default;
        }

        public static void Trace(string message)
        {
            if (Options.LogEnabled)
            {
                MyUtil.Trace(message);
            }
        }
        #endregion
    }
}