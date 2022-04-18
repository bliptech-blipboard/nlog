using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bliptech.Blipboard
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
        static CollapsePattern[] StandardCollapsePatterns = new[]
        {
            new CollapsePattern("dummy", @""),
            new CollapsePattern("numbers", @"(?<![\\w+-])[-+]?[0-9]*\\.?[0-9]+([eE][-+]?[0-9]+)?"),
            new CollapsePattern("guids", @"[0-9A-Fa-f]\{8\}-([0-9A-Fa-f]\{4\}-)\{3\}[0-9A-Fa-f]\{12\}"),
            new CollapsePattern("double-quotes", @"""[^""]*"""),
            new CollapsePattern("single-quotes", @"'[^']*'"),
            new CollapsePattern("brackets", @"\\[[^[]*\\]"),
        };

        public static CollapsePatternRegistry Default { get; set; } = new CollapsePatternRegistry();

        Dictionary<String, CollapsePattern> patterns = new Dictionary<String, CollapsePattern>();

        internal CollapsePatternRegistry()
        {
            Add(StandardCollapsePatterns);
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

namespace Bliptech.Blipboard.NLog
{
    [Target("BlipboardNetwork")]
    public class BlipboardNetworkTarget : NetworkTarget
    {
        public Layout? Label { get; set; }
        public Layout? Level { get; set; }
        public Layout? MessageTemplate { get; set; }
        public Layout? Message { get; set; }
        public Layout? Details { get; set; }

        public String Collapse { get; set; } = "";

        public BlipboardNetworkTarget()
        {
            OnOverflow = NetworkTargetOverflowAction.Error;
            OnConnectionOverflow = NetworkTargetConnectionsOverflowAction.Block;
            MaxMessageSize = 3500;
            ConnectionCacheSize = 1;
        }

        protected override void InitializeTarget()
        {
            var label = Label
                ?? new SimpleLayout("${logger}");

            var level = Level
                ?? new SimpleLayout("${uppercase:${level}}");

            var messageTemplate = MessageTemplate
                ?? new SimpleLayout("${message:raw=true:truncate=240}");

            var message = Message
                ?? new SimpleLayout("${message:truncate=240}");

            var details = Details
                ?? new SimpleLayout("${exception:format=tostring:truncate=2000}");

            if (!String.IsNullOrWhiteSpace(Collapse))
            {
                var combinedRegex = CollapsePatternRegistry.Default.GetCombinedPatternRegex(Collapse);

                var regex = SimpleLayout.Escape(combinedRegex);

                var layoutText = "${replace:regex=true:searchFor=" + regex + ":replaceWith=*:inner=${message:raw=true}:truncate=240}";

                messageTemplate = new SimpleLayout(layoutText);
            }

            var rootLayout = new JsonLayout();
             
            rootLayout.Attributes.Add(new JsonAttribute("label", label));
            rootLayout.Attributes.Add(new JsonAttribute("level", level));
            rootLayout.Attributes.Add(new JsonAttribute("msg_template", messageTemplate));
            rootLayout.Attributes.Add(new JsonAttribute("message", message));
            rootLayout.Attributes.Add(new JsonAttribute("details", details));

            Layout = rootLayout;

            base.InitializeTarget();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            base.Write(logEvent);
        }
    }
}
