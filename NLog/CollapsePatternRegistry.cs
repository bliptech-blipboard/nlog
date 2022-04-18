using System;
using System.Collections.Generic;
using System.Linq;

namespace Bliptech.Blipboard.NLog
{
    public class CollapsePattern
    {
        public String Name { get; set; }

        public String Regex { get; set; }

        public CollapsePattern(String name, String regex)
        {
            Name = name;
            Regex = regex;
        }
    }

    public class CollapsePatternRegistry
    {
        public static CollapsePatternRegistry Default { get; set; } = new CollapsePatternRegistry();

        Dictionary<String, CollapsePattern> patterns = new Dictionary<String, CollapsePattern>();

        internal CollapsePatternRegistry()
        {
            Add(StandardCollapsePatterns.Patterns);
        }

        public void Add(IEnumerable<CollapsePattern> patterns)
        {
            foreach (var pattern in patterns)
            {
                Add(pattern);
            }
        }

        public void Add(CollapsePattern pattern, Boolean force = false)
        {
            if (!force && patterns.ContainsKey(pattern.Name))
            {
                throw new Exception($"A pattern named '{pattern.Name}' is already registered as '{pattern.Regex}'; use force paramter to override");
            }

            patterns[pattern.Name] = pattern;
        }

        public void Add(String name, String regex, Boolean force = false)
        {
            Add(new CollapsePattern(name, regex), force);
        }

        public String GetPatternRegex(String name)
        {
            if (!patterns.TryGetValue(name.Trim(), out var pattern)) throw new Exception($"No collapse pattern with name '{name}' is known");

            return pattern.Regex;
        }

        public String GetCombinedPatternRegex(String separatedNames)
        {
            return GetCombinedPatternRegex(separatedNames.Split(','));
        }

        public String GetCombinedPatternRegex(IEnumerable<String> names)
        {
            return String.Join("|", from n in names select $"({GetPatternRegex(n)})");
        }
    }
}
