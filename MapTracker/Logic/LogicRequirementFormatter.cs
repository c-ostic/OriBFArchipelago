using System.Collections.Generic;
using System.Text;

namespace OriBFArchipelago.MapTracker.Logic
{
    /// <summary>
    /// Turns raw logic requirement tokens (e.g. "HealthCell:12", "DoubleJump", "Free")
    /// into human-readable labels for display in the hover panel.
    /// </summary>
    internal static class LogicRequirementFormatter
    {
        // Friendly names for counted resources (the part before the ':')
        private static readonly Dictionary<string, string> CountedNames = new Dictionary<string, string>
        {
            { "HealthCell", "Health" },
            { "EnergyCell", "Energy" },
            { "AbilityCell", "Ability Cells" },
            { "KeyStone", "Keystones" },
            { "GladesKeyStone", "Glades Keystones" },
            { "GrottoKeyStone", "Grotto Keystones" },
            { "GinsoKeyStone", "Ginso Keystones" },
            { "SwampKeyStone", "Swamp Keystones" },
            { "ForlornKeyStone", "Forlorn Keystones" },
            { "SorrowKeyStone", "Sorrow Keystones" },
            { "MapStone", "Mapstones" },
        };

        /// <summary>
        /// Tokens that carry no useful "you need this" meaning on their own. These are
        /// dropped from a combination unless they are the only token present.
        /// </summary>
        public static bool IsMeta(string token)
        {
            return token == "Free" || token == "Open" || token == "None" || token == "OpenWorld";
        }

        public static string FormatToken(string token)
        {
            if (token == "Free" || token == "Open")
                return "Free";
            if (token == "None")
                return "Impossible";
            if (token == "OpenWorld")
                return "Open World";

            // Counted resources, e.g. "HealthCell:12" -> "12 Health"
            int colon = token.IndexOf(':');
            if (colon >= 0)
            {
                string name = token.Substring(0, colon);
                string count = token.Substring(colon + 1);
                string friendly = CountedNames.TryGetValue(name, out string mapped) ? mapped : SplitCamelCase(name);
                return $"{count} {friendly}";
            }

            // Plain ability tokens, e.g. "DoubleJump" -> "Double Jump"
            return SplitCamelCase(token);
        }

        /// <summary>
        /// Inserts a space before each uppercase letter that follows a lowercase letter or digit.
        /// "DoubleJump" -> "Double Jump", "GinsoKey" -> "Ginso Key", "ChargeFlameBurn" -> "Charge Flame Burn".
        /// </summary>
        private static string SplitCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var sb = new StringBuilder(value.Length + 4);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (i > 0 && char.IsUpper(c) && !char.IsUpper(value[i - 1]) && value[i - 1] != ' ')
                    sb.Append(' ');
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
