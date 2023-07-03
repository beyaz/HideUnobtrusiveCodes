using System;
using HideUnobtrusiveCodes.Common;
using HideUnobtrusiveCodes.Options;
using YamlDotNet.Serialization;

namespace HideUnobtrusiveCodes.Application
{
    static class App
    {
        public static readonly OptionsModel Options = SafeExecute(OptionsReader.ReadOptionsFromFile);

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
        
        public static void Trace(object instance)
        {
            if (Options.LogEnabled)
            {
                if (instance == null)
                {
                    MyUtil.Trace("null");
                    return;
                }

                MyUtil.Trace(new Serializer().Serialize(instance));
            }
        }
    }
}