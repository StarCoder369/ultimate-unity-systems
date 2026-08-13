#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;

public static class ModularInspectorSourceRewriter
{
    public enum ChangeType
    {
        Convert,
        Remove
    }

    public sealed class Change
    {
        public ChangeType Type;
        public string AttributeName;
        public string OriginalText;
        public string ReplacementText;
        public int Line;
    }

    public sealed class Result
    {
        public string OriginalText;
        public string RewrittenText;
        public List<Change> Changes = new();
        public bool HasErrors;
        public string ErrorMessage;
    }

    private static readonly Dictionary<string, string> conversions = new()
    {
        { "InspectorHeader", "Header" },
        { "InspectorTooltip", "Tooltip" },
        { "InspectorSpace", "Space" },
        { "InspectorRange", "Range" }
    };

    private static readonly HashSet<string> removals = new()
    {
        "IncludeModularInspector",
        "InspectorHide",
        "InspectorReadOnly",
        "InspectorShowIf",
        "InspectorHideIf",
        "InspectorDisableIf",
        "InspectorLabel",
        "InspectorSeparator"
    };

    public static Result Rewrite(string source, string filePath)
    {
        Result result = new Result
        {
            OriginalText = source,
            RewrittenText = source
        };

        try
        {
            List<AttributeBlock> attributes = FindAttributes(source);

            if (attributes.Count == 0)
            {
                return result;
            }

            StringBuilder output = new StringBuilder(source);

            for (int i = attributes.Count - 1; i >= 0; i--)
            {
                AttributeBlock block = attributes[i];

                ProcessAttributeBlock(block, source, result);

                if (block.Replacement == null)
                {
                    continue;
                }

                output.Remove(block.Start, block.Length);
                output.Insert(block.Start, block.Replacement);
            }

            result.RewrittenText = output.ToString();
        }
        catch (Exception exception)
        {
            result.HasErrors = true;
            result.ErrorMessage = exception.Message;
            result.RewrittenText = source;
        }

        return result;
    }

    private sealed class AttributeBlock
    {
        public int Start;
        public int Length;
        public string Text;
        public string Replacement;
    }

    private static List<AttributeBlock> FindAttributes(string source)
    {
        List<AttributeBlock> result = new();

        bool inString = false;
        bool inCharacter = false;
        bool inLineComment = false;
        bool inBlockComment = false;

        for (int i = 0; i < source.Length; i++)
        {
            char current = source[i];
            char next = i + 1 < source.Length ? source[i + 1] : '\0';

            if (inLineComment)
            {
                if (current == '\n')
                {
                    inLineComment = false;
                }

                continue;
            }

            if (inBlockComment)
            {
                if (current == '*' && next == '/')
                {
                    inBlockComment = false;
                    i++;
                }

                continue;
            }

            if (inString)
            {
                if (current == '\\')
                {
                    i++;
                    continue;
                }

                if (current == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (inCharacter)
            {
                if (current == '\\')
                {
                    i++;
                    continue;
                }

                if (current == '\'')
                {
                    inCharacter = false;
                }

                continue;
            }

            if (current == '/' && next == '/')
            {
                inLineComment = true;
                i++;
                continue;
            }

            if (current == '/' && next == '*')
            {
                inBlockComment = true;
                i++;
                continue;
            }

            if (current == '"')
            {
                inString = true;
                continue;
            }

            if (current == '\'')
            {
                inCharacter = true;
                continue;
            }

            if (current != '[')
            {
                continue;
            }

            int end = FindAttributeEnd(source, i);

            if (end < 0)
            {
                throw new InvalidOperationException("An attribute block could not be parsed.");
            }

            string text = source.Substring(i, end - i + 1);

            if (ContainsModularInspectorAttribute(text))
            {
                result.Add(new AttributeBlock
                {
                    Start = i,
                    Length = end - i + 1,
                    Text = text
                });
            }

            i = end;
        }

        return result;
    }

    private static int FindAttributeEnd(string source, int start)
    {
        int squareDepth = 0;
        int parenthesisDepth = 0;
        int braceDepth = 0;

        bool inString = false;
        bool inCharacter = false;

        for (int i = start; i < source.Length; i++)
        {
            char current = source[i];

            if (inString)
            {
                if (current == '\\')
                {
                    i++;
                    continue;
                }

                if (current == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (inCharacter)
            {
                if (current == '\\')
                {
                    i++;
                    continue;
                }

                if (current == '\'')
                {
                    inCharacter = false;
                }

                continue;
            }

            if (current == '"')
            {
                inString = true;
                continue;
            }

            if (current == '\'')
            {
                inCharacter = true;
                continue;
            }

            if (current == '[')
            {
                squareDepth++;
            }
            else if (current == ']')
            {
                squareDepth--;

                if (squareDepth == 0 && parenthesisDepth == 0 && braceDepth == 0)
                {
                    return i;
                }
            }
            else if (current == '(')
            {
                parenthesisDepth++;
            }
            else if (current == ')')
            {
                parenthesisDepth--;
            }
            else if (current == '{')
            {
                braceDepth++;
            }
            else if (current == '}')
            {
                braceDepth--;
            }
        }

        return -1;
    }

    private static bool ContainsModularInspectorAttribute(string block)
    {
        foreach (string name in conversions.Keys)
        {
            if (ContainsAttributeName(block, name))
            {
                return true;
            }
        }

        foreach (string name in removals)
        {
            if (ContainsAttributeName(block, name))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsAttributeName(string block, string name)
    {
        string cleaned = RemoveWhitespace(block);

        return cleaned.Contains("[" + name) ||
               cleaned.Contains("," + name) ||
               cleaned.Contains("[" + name + "Attribute") ||
               cleaned.Contains("," + name + "Attribute");
    }

    private static void ProcessAttributeBlock(AttributeBlock block, string source, Result result)
    {
        string inner = block.Text.Substring(1, block.Text.Length - 2);

        List<string> attributes = SplitAttributes(inner);

        List<string> remaining = new();

        foreach (string attribute in attributes)
        {
            string name = GetAttributeName(attribute);

            if (conversions.TryGetValue(name, out string replacement))
            {
                string converted = ConvertAttribute(attribute, replacement);

                result.Changes.Add(new Change
                {
                    Type = ChangeType.Convert,
                    AttributeName = name,
                    OriginalText = attribute.Trim(),
                    ReplacementText = converted.Trim(),
                    Line = GetLine(source, block.Start)
                });

                remaining.Add(converted);
                continue;
            }

            if (removals.Contains(name))
            {
                result.Changes.Add(new Change
                {
                    Type = ChangeType.Remove,
                    AttributeName = name,
                    OriginalText = attribute.Trim(),
                    ReplacementText = "",
                    Line = GetLine(source, block.Start)
                });

                continue;
            }

            remaining.Add(attribute);
        }

        if (remaining.Count == 0)
        {
            block.Replacement = "";
            return;
        }

        if (remaining.Count == attributes.Count)
        {
            block.Replacement = null;
            return;
        }

        block.Replacement = "[" + string.Join(", ", remaining) + "]";
    }

    private static List<string> SplitAttributes(string text)
    {
        List<string> result = new();

        int start = 0;
        int parentheses = 0;
        int braces = 0;
        int brackets = 0;

        bool inString = false;
        bool inCharacter = false;

        for (int i = 0; i < text.Length; i++)
        {
            char current = text[i];

            if (inString)
            {
                if (current == '\\')
                {
                    i++;
                    continue;
                }

                if (current == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (inCharacter)
            {
                if (current == '\\')
                {
                    i++;
                    continue;
                }

                if (current == '\'')
                {
                    inCharacter = false;
                }

                continue;
            }

            if (current == '"')
            {
                inString = true;
                continue;
            }

            if (current == '\'')
            {
                inCharacter = true;
                continue;
            }

            if (current == '(')
            {
                parentheses++;
            }
            else if (current == ')')
            {
                parentheses--;
            }
            else if (current == '{')
            {
                braces++;
            }
            else if (current == '}')
            {
                braces--;
            }
            else if (current == '[')
            {
                brackets++;
            }
            else if (current == ']')
            {
                brackets--;
            }
            else if (current == ',' && parentheses == 0 && braces == 0 && brackets == 0)
            {
                result.Add(text.Substring(start, i - start));
                start = i + 1;
            }
        }

        result.Add(text.Substring(start));

        return result;
    }

    private static string GetAttributeName(string attribute)
    {
        string text = attribute.Trim();

        int parenthesis = text.IndexOf('(');

        if (parenthesis >= 0)
        {
            text = text.Substring(0, parenthesis).Trim();
        }

        int space = text.IndexOf(' ');

        if (space >= 0)
        {
            text = text.Substring(0, space).Trim();
        }

        if (text.EndsWith("Attribute", StringComparison.Ordinal))
        {
            text = text.Substring(0, text.Length - 9);
        }

        int dot = text.LastIndexOf('.');

        if (dot >= 0)
        {
            text = text.Substring(dot + 1);
        }

        return text;
    }

    private static string ConvertAttribute(string attribute, string replacement)
    {
        string text = attribute.Trim();

        int parenthesis = text.IndexOf('(');

        if (parenthesis < 0)
        {
            return "[" + replacement + "]";
        }

        string arguments = text.Substring(parenthesis);

        if (replacement == "Header")
        {
            string firstArgument = GetFirstArgument(arguments);

            if (string.IsNullOrWhiteSpace(firstArgument))
            {
                return "[" + replacement + "]";
            }

            return "[" + replacement + "(" + firstArgument + ")]";
        }

        return "[" + replacement + arguments + "]";
    }

    private static string GetFirstArgument(string arguments)
    {
        string text = arguments.Trim();

        if (text.StartsWith("("))
        {
            text = text.Substring(1);
        }

        int parentheses = 0;
        bool inString = false;

        for (int i = 0; i < text.Length; i++)
        {
            char current = text[i];

            if (inString)
            {
                if (current == '\\')
                {
                    i++;
                    continue;
                }

                if (current == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (current == '"')
            {
                inString = true;
                continue;
            }

            if (current == '(')
            {
                parentheses++;
            }
            else if (current == ')')
            {
                if (parentheses == 0)
                {
                    return text.Substring(0, i).Trim();
                }

                parentheses--;
            }
            else if (current == ',' && parentheses == 0)
            {
                return text.Substring(0, i).Trim();
            }
        }

        return text.TrimEnd(')').Trim();
    }

    private static string RemoveWhitespace(string text)
    {
        StringBuilder builder = new();

        foreach (char character in text)
        {
            if (!char.IsWhiteSpace(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private static int GetLine(string source, int position)
    {
        int line = 1;

        for (int i = 0; i < position && i < source.Length; i++)
        {
            if (source[i] == '\n')
            {
                line++;
            }
        }

        return line;
    }
}

#endif