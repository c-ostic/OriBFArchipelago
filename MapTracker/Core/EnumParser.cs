using System;
using System.Linq;

namespace OriBFArchipelago.MapTracker.Core
{
    internal class EnumParser
    {
        internal static string[] GetEnumNames<T>() where T : Enum
        {
            var names = Enum.GetNames(typeof(T));
            var parsednames = names.Select(c => c.Replace("_", " "));
            return parsednames.ToArray();
        }

        internal static T GetEnumValue<T>(string value)
        {
            var parsedValue = value.Replace(" ", "_");
            return (T)Enum.Parse(typeof(T), parsedValue);
        }
    }
}
