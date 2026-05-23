using System.Linq;
using System.Text.RegularExpressions;

namespace AppDimens.Maui.Markup;

/// <summary>
/// Expands compact markup tokens (e.g. <c>{sdp:20}</c>) into MAUI markup extension syntax.
/// </summary>
public static class ShortDimenMarkupExpander
{
    private const string MarkupStart = "(?<!\\{\\})(?<!\\{)\\{";

    private static readonly (string Prefix, string ClassName, string FirstProperty)[] Facilitators =
    {
        ("sdpRotate", "SdpRotate", "Base"),
        ("hdpRotate", "HdpRotate", "Base"),
        ("wdpRotate", "WdpRotate", "Base"),
        ("hemRotate", "HemRotate", "Base"),
        ("wemRotate", "WemRotate", "Base"),
        ("sspRotate", "SspRotate", "Base"),
        ("hspRotate", "HspRotate", "Base"),
        ("wspRotate", "WspRotate", "Base"),
        ("semRotate", "SemRotate", "Base"),
        ("sdpQualifier", "SdpQualifier", "Base"),
        ("sspQualifier", "SspQualifier", "Base"),
        ("hspQualifier", "HspQualifier", "Base"),
        ("wspQualifier", "WspQualifier", "Base"),
        ("semQualifier", "SemQualifier", "Base"),
        ("hemQualifier", "HemQualifier", "Base"),
        ("wemQualifier", "WemQualifier", "Base"),
        ("sdpMode", "SdpMode", "Base"),
        ("sspMode", "SspMode", "Base"),
        ("hspMode", "HspMode", "Base"),
        ("wspMode", "WspMode", "Base"),
        ("semMode", "SemMode", "Base"),
        ("hemMode", "HemMode", "Base"),
        ("wemMode", "WemMode", "Base"),
        ("sdpScreen", "SdpScreen", "Base"),
        ("sspScreen", "SspScreen", "Base"),
        ("hspScreen", "HspScreen", "Base"),
        ("wspScreen", "WspScreen", "Base"),
        ("semScreen", "SemScreen", "Base"),
        ("hemScreen", "HemScreen", "Base"),
        ("wemScreen", "WemScreen", "Base"),
    };

    private static readonly string[] SimplePrefixes =
    {
        "sdpPhA", "sdpLwA", "sdpLhA", "sdpPwA",
        "sspPhA", "sspLwA", "sspLhA", "sspPwA",
        "hspLwA", "hspPwA", "wspLhA", "wspPhA",
        "semaPh", "semaLw", "semaLh", "semaPw",
        "hemaLw", "hemaPw", "wemaLh", "wemaPh",
        "sdpPh", "sdpLw", "sdpLh", "sdpPw",
        "hdpLw", "hdpPw", "wdpLh", "wdpPh",
        "sspPh", "sspLw", "sspLh", "sspPw",
        "hspLw", "hspPw", "wspLh", "wspPh",
        "semPh", "semLw", "semLh", "semPw",
        "hemLw", "hemPw", "wemLh", "wemPh",
        "sdpa", "hdpa", "wdpa", "sspa", "hspa", "wspa",
        "sdp", "ssp", "hdp", "wdp", "sem", "hem", "wem",
        "scaled",
    };

    public static string Expand(string content)
    {
        var result = content;

        result = Regex.Replace(result, MarkupStart + @"scaled:(-?\d+)\s*,", "{scaled:Scaled Value=$1,");

        foreach (var (prefix, className, firstProperty) in Facilitators)
        {
            var pattern = MarkupStart + $@"{Regex.Escape(prefix)}:(-?\d+)\s*,";
            var replacement = $"{{{prefix}:{className} {firstProperty}=$1,";
            result = Regex.Replace(result, pattern, replacement);
        }

        var prefixPattern = string.Join("|", SimplePrefixes.Select(Regex.Escape));
        var simplePattern = MarkupStart + $@"({prefixPattern}):(-?\d+)\}}";
        result = Regex.Replace(result, simplePattern, m =>
        {
            var prefix = m.Groups[1].Value;
            var value = m.Groups[2].Value;
            var className = char.ToUpperInvariant(prefix[0]) + prefix.Substring(1);
            return $"{{{prefix}:{className} Value={value}}}";
        });

        return result;
    }
}
