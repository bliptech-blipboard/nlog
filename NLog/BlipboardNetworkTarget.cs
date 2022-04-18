using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;

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
                ?? new SimpleLayout("${logger:truncate=80}");

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
