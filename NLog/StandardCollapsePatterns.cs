using System;
using System.Collections.Generic;
using System.Text;

namespace Bliptech.Blipboard.NLog
{
    internal class StandardCollapsePatterns
    {
        internal static CollapsePattern[] Patterns = new[]
        {
            new CollapsePattern("dummy", @""),

            new CollapsePattern("numbers", @"(?<![\\w+-])[-+]?[0-9]*\\.?[0-9]+([eE][-+]?[0-9]+)?"),

            new CollapsePattern("guids", @"[0-9A-Fa-f]\{8\}-([0-9A-Fa-f]\{4\}-)\{3\}[0-9A-Fa-f]\{12\}"),

            new CollapsePattern("double-quotes", @"""[^""]*"""),

            new CollapsePattern("single-quotes", @"'[^']*'"),

            new CollapsePattern("brackets", @"\\[[^[]*\\]"),
        };
    }
}
