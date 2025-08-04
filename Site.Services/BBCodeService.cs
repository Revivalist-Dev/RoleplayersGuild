// F:\Visual Studio\RoleplayersGuild\Site.Services\BBCodeService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace RoleplayersGuild.Site.Services
{
    #region Supporting Classes for AST

    public abstract class BBNode
    {
        public int Start { get; set; }
        public int End { get; set; }
    }

    public class TextNode : BBNode
    {
        public override string ToString() => $"TextNode ({Start}-{End})";
    }

    public class TagNode : BBNode
    {
        public string TagName { get; set; }
        public string? Attribute { get; set; }
        public List<BBNode> Content { get; set; } = new List<BBNode>();
        public override string ToString() => $"TagNode:[{TagName}] ({Start}-{End})";
    }

    public class BBTag
    {
        public string? HtmlPrefix { get; set; }
        public string? HtmlSuffix { get; set; }
        public Func<TagNode, string, int, Task<string>>? CustomRenderer { get; set; }
        public object Allowed { get; set; } = true;
    }

    #endregion

    public class BBCodeService : IBBCodeService
    {
        private readonly IDataService _dataService;
        private readonly ImageSettings _imageSettings;
        private readonly Dictionary<string, BBTag> _tags;
        private string _currentText = "";

        private static readonly Regex UrlRegex = new Regex(@"^\s*((?:https?|ftps?|irc):\/\/[^\s/$.?#""][^\s]*)\s*$", RegexOptions.Compiled);
        private static readonly Regex EmoteRegex = BuildEmoteRegex();

        public BBCodeService(IDataService dataService, IOptions<ImageSettings> imageSettings)
        {
            _dataService = dataService;
            _imageSettings = imageSettings.Value;
            _tags = InitializeTags();
        }

        private static Regex BuildEmoteRegex()
        {
            var emoteNames = "hex-smile heart hex-yell hex-sad hex-grin hex-red hex-razz hex-twist hex-roll hex-mad hex-confuse hex-eek hex-wink lif-angry lif-blush lif-cry lif-evil lif-gasp lif-happy lif-meh lif-neutral lif-ooh lif-purr lif-roll lif-sad lif-sick lif-smile lif-whee lif-wink lif-wtf lif-yawn cake".Split(' ');
            string pattern = $":({string.Join("|", emoteNames.Select(Regex.Escape))}):";
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        private Dictionary<string, BBTag> InitializeTags()
        {
            var tags = new Dictionary<string, BBTag>(StringComparer.OrdinalIgnoreCase)
            {
                { "b", new BBTag { HtmlPrefix = "<strong>", HtmlSuffix = "</strong>" } },
                { "i", new BBTag { HtmlPrefix = "<em>", HtmlSuffix = "</em>" } },
                { "u", new BBTag { HtmlPrefix = "<span style=\"text-decoration:underline;\">", HtmlSuffix = "</span>" } },
                { "s", new BBTag { HtmlPrefix = "<del>", HtmlSuffix = "</del>" } },
                { "hr", new BBTag { HtmlPrefix = "<hr />", HtmlSuffix = "" } },
                { "sub", new BBTag { HtmlPrefix = "<sub>", HtmlSuffix = "</sub>", Allowed = new[] { "b", "i", "u" } } },
                { "sup", new BBTag { HtmlPrefix = "<sup>", HtmlSuffix = "</sup>", Allowed = new[] { "b", "i", "u" } } },
                { "big", new BBTag { HtmlPrefix = "<span class=\"bigtext\">", HtmlSuffix = "</span>" } },
                { "small", new BBTag { HtmlPrefix = "<span class=\"smalltext\">", HtmlSuffix = "</span>" } },
                { "heading", new BBTag { HtmlPrefix = "<div class=\"heading\">", HtmlSuffix = "</div>", Allowed = false } },
                { "left", new BBTag { HtmlPrefix = "<div style=\"text-align: left;\">", HtmlSuffix = "</div>" } },
                { "center", new BBTag { HtmlPrefix = "<div style=\"text-align: center;\">", HtmlSuffix = "</div>" } },
                { "right", new BBTag { HtmlPrefix = "<div style=\"text-align: right;\">", HtmlSuffix = "</div>" } },
                { "justify", new BBTag { HtmlPrefix = "<div style=\"text-align: justify;\">", HtmlSuffix = "</div>" } },
                { "indent", new BBTag { HtmlPrefix = "<div style=\"padding-left: 3em;\">", HtmlSuffix = "</div>" } },
                { "quote", new BBTag { HtmlPrefix = "<blockquote class=\"blockquote\">", HtmlSuffix = "</blockquote>" } },
                { "noparse", new BBTag { CustomRenderer = (node, content, id) => Task.FromResult(WebUtility.HtmlEncode(content)), Allowed = false } },
                { "color", new BBTag { CustomRenderer = RenderColorTag, Allowed = true } },
                { "url", new BBTag { CustomRenderer = RenderUrlTag, Allowed = false } },
                { "collapse", new BBTag { CustomRenderer = RenderCollapseTag, Allowed = true } },
                { "img", new BBTag { CustomRenderer = RenderImgTag, Allowed = false } },
                { "icon", new BBTag { CustomRenderer = RenderUserOrIconTag, Allowed = false } },
                { "user", new BBTag { CustomRenderer = RenderUserOrIconTag, Allowed = false } },
                { "eicon", new BBTag { CustomRenderer = (n,c, id) => Task.FromResult(WebUtility.HtmlEncode($"[eicon]{c}[/eicon]")), Allowed = false } }
            };
            return tags;
        }

        public async Task<string> ParseAsync(string bbcode, int characterId)
        {
            if (string.IsNullOrEmpty(bbcode))
                return string.Empty;

            _currentText = bbcode.Replace("[]", "&#91;&#93;");

            var ast = GenerateAst(0, _currentText.Length);
            NormalizeAst(ast, 0, _currentText.Length);

            string html = await ParseAstToHtml(ast, _tags.Keys.ToArray(), characterId);

            if (!html.Contains("<br", StringComparison.OrdinalIgnoreCase))
            {
                html = html.Replace("\r\n", "\n").Replace("\n", "<br />");
            }

            html = ParseEmotes(html);
            _currentText = "";
            return html;
        }

        #region AST Generation
        private List<BBNode> GenerateAst(int start, int end)
        {
            var nodes = new List<BBNode>();
            int currentPos = start;

            while (currentPos < end)
            {
                int tagStartPos = _currentText.IndexOf('[', currentPos, end - currentPos);

                if (tagStartPos == -1)
                {
                    if (currentPos < end) nodes.Add(new TextNode { Start = currentPos, End = end });
                    break;
                }

                if (tagStartPos > currentPos)
                {
                    nodes.Add(new TextNode { Start = currentPos, End = tagStartPos });
                }

                int tagEndBracketPos = _currentText.IndexOf(']', tagStartPos, end - tagStartPos);
                if (tagEndBracketPos == -1)
                {
                    nodes.Add(new TextNode { Start = tagStartPos, End = end });
                    break;
                }

                string fullTag = _currentText.Substring(tagStartPos + 1, tagEndBracketPos - tagStartPos - 1);

                if (fullTag.StartsWith('/'))
                {
                    nodes.Add(new TextNode { Start = tagStartPos, End = tagEndBracketPos + 1 });
                    currentPos = tagEndBracketPos + 1;
                    continue;
                }

                var match = Regex.Match(fullTag, @"^([a-z0-9]+)(?:=([^\]]+))?$", RegexOptions.IgnoreCase);

                if (!match.Success || !_tags.ContainsKey(match.Groups[1].Value))
                {
                    nodes.Add(new TextNode { Start = tagStartPos, End = tagEndBracketPos + 1 });
                    currentPos = tagEndBracketPos + 1;
                    continue;
                }

                string tagName = match.Groups[1].Value;
                string? attribute = match.Groups[2].Success ? match.Groups[2].Value : null;

                if (string.Equals(tagName, "hr", StringComparison.OrdinalIgnoreCase))
                {
                    nodes.Add(new TagNode { TagName = tagName, Attribute = attribute, Start = tagStartPos, End = tagEndBracketPos + 1 });
                    currentPos = tagEndBracketPos + 1;
                    continue;
                }

                int closingTagPos = FindMatchingEndTag(tagName, tagEndBracketPos + 1, end);
                if (closingTagPos == -1)
                {
                    nodes.Add(new TextNode { Start = tagStartPos, End = tagEndBracketPos + 1 });
                    currentPos = tagEndBracketPos + 1;
                }
                else
                {
                    var tagNode = new TagNode
                    {
                        TagName = tagName,
                        Attribute = attribute,
                        Start = tagStartPos,
                        End = closingTagPos + tagName.Length + 3,
                        Content = GenerateAst(tagEndBracketPos + 1, closingTagPos)
                    };
                    nodes.Add(tagNode);
                    currentPos = tagNode.End;
                }
            }
            return nodes;
        }
        private int FindMatchingEndTag(string tagName, int startIndex, int endIndex)
        {
            string startTagPattern = $"[{tagName}";
            string endTagPattern = $"[/{tagName}]";
            int nestingLevel = 1;
            int searchPos = startIndex;

            while (searchPos < endIndex)
            {
                int nextEndTag = _currentText.IndexOf(endTagPattern, searchPos, endIndex - searchPos, StringComparison.OrdinalIgnoreCase);
                int nextStartTag = _currentText.IndexOf(startTagPattern, searchPos, endIndex - searchPos, StringComparison.OrdinalIgnoreCase);

                if (nextEndTag == -1) return -1;

                if (nextStartTag != -1 && nextStartTag < nextEndTag && _currentText.IndexOf(']', nextStartTag) > nextStartTag)
                {
                    nestingLevel++;
                    searchPos = _currentText.IndexOf(']', nextStartTag) + 1;
                    if (searchPos == 0) return -1;
                }
                else
                {
                    nestingLevel--;
                    if (nestingLevel == 0) return nextEndTag;
                    searchPos = nextEndTag + endTagPattern.Length;
                }
            }
            return -1;
        }
        private void NormalizeAst(List<BBNode> ast, int startPos, int endPos)
        {
            if (!ast.Any()) return;

            ast.First().Start = startPos;

            for (int i = 0; i < ast.Count; i++)
            {
                if (i < ast.Count - 1)
                {
                    ast[i].End = ast[i + 1].Start;
                }
                else
                {
                    ast[i].End = endPos;
                }

                if (ast[i] is TagNode tagNode)
                {
                    int contentStart = tagNode.Start + tagNode.TagName.Length + (tagNode.Attribute != null ? 1 + tagNode.Attribute.Length : 0) + 2;
                    int contentEnd = tagNode.End - (tagNode.TagName.Length + 3);
                    if (contentStart < contentEnd)
                    {
                        NormalizeAst(tagNode.Content, contentStart, contentEnd);
                    }
                }
            }
        }
        #endregion

        #region HTML Rendering from AST
        private async Task<string> ParseAstToHtml(List<BBNode> nodes, object allowedTags, int characterId)
        {
            var result = new StringBuilder();
            foreach (var node in nodes)
            {
                if (node is TextNode textNode)
                {
                    if (textNode.Start < textNode.End)
                    {
                        result.Append(WebUtility.HtmlEncode(_currentText.Substring(textNode.Start, textNode.End - textNode.Start)));
                    }
                }
                else if (node is TagNode tagNode)
                {
                    if (!_tags.TryGetValue(tagNode.TagName, out var tagDef))
                    {
                        result.Append(WebUtility.HtmlEncode(_currentText.Substring(tagNode.Start, tagNode.End - tagNode.Start)));
                        continue;
                    }

                    bool isAllowed = allowedTags is bool bVal ? bVal : (allowedTags as string[])?.Contains(tagNode.TagName, StringComparer.OrdinalIgnoreCase) ?? false;

                    if (!isAllowed)
                    {
                        result.Append(WebUtility.HtmlEncode(_currentText.Substring(tagNode.Start, tagNode.End - tagNode.Start)));
                        continue;
                    }

                    object innerAllowed = GetNewAllowedScope(allowedTags, tagDef.Allowed);
                    string innerContent = await ParseAstToHtml(tagNode.Content, innerAllowed, characterId);

                    if (tagDef.CustomRenderer != null)
                    {
                        result.Append(await tagDef.CustomRenderer(tagNode, innerContent, characterId));
                    }
                    else
                    {
                        result.Append(tagDef.HtmlPrefix + innerContent + tagDef.HtmlSuffix);
                    }
                }
            }
            return result.ToString();
        }
        private object GetNewAllowedScope(object outerAllowed, object innerAllowedDef)
        {
            if (outerAllowed is bool outerBool && !outerBool) return false;
            if (innerAllowedDef is bool innerBool && !innerBool) return false;
            if (outerAllowed is bool outerBoolTrue && outerBoolTrue) return innerAllowedDef;
            if (innerAllowedDef is bool innerBoolTrue && innerBoolTrue) return outerAllowed;

            var outerArr = (string[])outerAllowed;
            var innerArr = (string[])innerAllowedDef;
            return outerArr.Intersect(innerArr, StringComparer.OrdinalIgnoreCase).ToArray();
        }
        #endregion

        #region Custom Tag Renderers
        private Task<string> RenderColorTag(TagNode node, string content, int characterId)
        {
            var allowedColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "red", "blue", "white", "yellow", "pink", "gray", "green", "orange", "purple", "black", "brown", "cyan" };
            string color = node?.Attribute ?? "";

            if (allowedColors.Contains(color))
            {
                return Task.FromResult($"<span class=\"{WebUtility.HtmlEncode(color)}font\">{content}</span>");
            }
            else if (Regex.IsMatch(color, @"^(#([0-9a-f]{3}){1,2})$", RegexOptions.IgnoreCase))
            {
                return Task.FromResult($"<span style=\"color:{WebUtility.HtmlEncode(color)};\">{content}</span>");
            }

            return Task.FromResult(WebUtility.HtmlEncode(_currentText.Substring(node.Start, node.End - node.Start)));
        }
        private Task<string> RenderUrlTag(TagNode node, string content, int characterId)
        {
            string url = node.Attribute ?? content;
            string linkText = string.IsNullOrEmpty(node.Attribute) ? url : content;

            if (!UrlRegex.IsMatch(url))
            {
                return Task.FromResult($"[bad url: {WebUtility.HtmlEncode(url)}] {WebUtility.HtmlEncode(linkText)}");
            }

            var match = UrlRegex.Match(url);
            string cleanUrl = match.Groups[1].Value;
            string hostname = "unknown";
            if (Uri.TryCreate(cleanUrl, UriKind.Absolute, out var uri))
            {
                hostname = uri.Host.Replace("www.", "");
            }

            string encodedUrl = WebUtility.HtmlEncode(cleanUrl);
            string encodedText = WebUtility.HtmlEncode(linkText);

            string link = $"<a href=\"{encodedUrl}\" class=\"ParsedLink ImageLink\" target=\"_blank\" rel=\"nofollow noreferrer noopener\">{encodedText}</a>";

            if (!string.IsNullOrEmpty(node.Attribute))
            {
                link += $" <span style=\"font-size: 0.8em;\">[{WebUtility.HtmlEncode(hostname)}]</span>";
            }

            return Task.FromResult(link);
        }
        private Task<string> RenderCollapseTag(TagNode node, string content, int characterId)
        {
            string headerText = WebUtility.HtmlEncode(!string.IsNullOrEmpty(node.Attribute) ? node.Attribute : "Details");
            return Task.FromResult(
                "<div class=\"CollapseHeader\">" +
                    $"<div class=\"CollapseHeaderText\"><span>{headerText}</span></div>" +
                    $"<div class=\"CollapseBlock\" style=\"display:none;\">{content}</div>" +
                "</div>"
            );
        }
        private async Task<string> RenderImgTag(TagNode node, string content, int characterId)
        {
            if (int.TryParse(node.Attribute, out int inlineId))
            {
                // Corrected: SQL query now uses PascalCase
                var inline = (await _dataService.GetRecordsAsync<CharacterInline>(
                    """SELECT * FROM "CharacterInlines" WHERE "CharacterId" = @characterId AND "InlineId" = @inlineId""",
                    new { characterId, inlineId })).FirstOrDefault();

                if (inline != null)
                {
                    // Corrected: Property access is now PascalCase (ImageUrl not ImageURL)
                    string url = _imageSettings.DisplayCharacterInlinesFolder + inline.InlineImageUrl;
                    string altText = WebUtility.HtmlEncode(content);
                    return $"<img class=\"ImageBlock\" src=\"{url}\" alt=\"{altText}\" />";
                }
            }
            return $"[Invalid Image: {WebUtility.HtmlEncode(node.Attribute)}]";
        }
        private Task<string> RenderUserOrIconTag(TagNode node, string content, int characterId)
        {
            string charName = WebUtility.HtmlEncode(content);
            return Task.FromResult($"<span class=\"bb-user-link\">{charName}</span>");
        }
        private string ParseEmotes(string text)
        {
            string emotePath = "/images/smileys/";
            return EmoteRegex.Replace(text, match => {
                string emoteName = match.Groups[1].Value;
                string encodedEmoteName = WebUtility.HtmlEncode(emoteName);
                return $"<img src='{emotePath}{encodedEmoteName.ToLowerInvariant()}.png' alt='{encodedEmoteName} emote' title=':{encodedEmoteName}:' align='middle'/>";
            });
        }
        #endregion
    }
}