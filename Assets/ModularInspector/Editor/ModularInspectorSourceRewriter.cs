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
            string rewritten = RemoveModularInspectorUsing(source, result);
            rewritten = ProcessAttributes(rewritten, result);
            result.RewrittenText = rewritten;
        }
        catch (Exception exception)
        {
            result.HasErrors = true;
            result.ErrorMessage = $"Could not process {filePath}: {exception.Message}";
            result.RewrittenText = source;
        }

        return result;
    }

    private static string RemoveModularInspectorUsing(string source, Result result)
    {
        const string usingText = "using ModularInspector;";
        int searchStart = 0;

        while (true)
        {
            int index = source.IndexOf(usingText, searchStart, StringComparison.Ordinal);

            if (index < 0)
            {
                break;
            }

            int lineStart = index;

            while (lineStart > 0 && source[lineStart - 1] != '\n')
            {
                lineStart--;
            }

            int lineEnd = index + usingText.Length;

            while (lineEnd < source.Length && source[lineEnd] != '\n')
            {
                lineEnd++;
            }

            string original = source.Substring(lineStart, lineEnd - lineStart);

            result.Changes.Add(new Change
            {
                Type = ChangeType.Remove,
                AttributeName = "using ModularInspector",
                OriginalText = original.Trim(),
                ReplacementText = "",
                Line = GetLine(source, lineStart)
            });

            source = source.Remove(lineStart, lineEnd - lineStart);
            searchStart = lineStart;
        }

        return source;
    }

    private static string ProcessAttributes(string source, Result result)
    {
        StringBuilder output = new();
        int position = 0;

        while (position < source.Length)
        {
            int attributeStart = FindNextAttribute(source, position);

            if (attributeStart < 0)
            {
                output.Append(source, position, source.Length - position);
                break;
            }

            output.Append(source, position, attributeStart - position);

            int attributeEnd = FindAttributeEnd(source, attributeStart);

            if (attributeEnd < 0)
            {
                output.Append(source, attributeStart, source.Length - attributeStart);
                break;
            }

            string attributeBlock = source.Substring(attributeStart, attributeEnd - attributeStart + 1);
            string replacement = ProcessAttributeBlock(attributeBlock, source, attributeStart, result);

            output.Append(replacement);
            position = attributeEnd + 1;
        }

        return output.ToString();
    }

    private static string ProcessAttributeBlock(string block, string source, int position, Result result)
    {
        List<string> attributes = SplitAttributes(block.Substring(1, block.Length - 2));
        List<string> replacements = new();

        foreach (string attribute in attributes)
        {
            string trimmed = attribute.Trim();

            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            string name = GetAttributeName(trimmed);

            if (name == "InspectorHeader")
            {
                List<string> converted = ConvertHeader(trimmed);

                foreach (string replacement in converted)
                {
                    replacements.Add(replacement);
                }

                AddChange(result, ChangeType.Convert, name, trimmed, string.Join(" ", converted), source, position);
                continue;
            }

            if (name == "InspectorTooltip")
            {
                string replacement = ConvertSimpleAttribute(trimmed, "Tooltip");

                replacements.Add(replacement);
                AddChange(result, ChangeType.Convert, name, trimmed, replacement, source, position);
                continue;
            }

            if (name == "InspectorSpace")
            {
                string replacement = ConvertSimpleAttribute(trimmed, "Space");

                replacements.Add(replacement);
                AddChange(result, ChangeType.Convert, name, trimmed, replacement, source, position);
                continue;
            }

            if (name == "InspectorRange")
            {
                string replacement = ConvertSimpleAttribute(trimmed, "Range");

                replacements.Add(replacement);
                AddChange(result, ChangeType.Convert, name, trimmed, replacement, source, position);
                continue;
            }

            if (removals.Contains(name))
            {
                AddChange(result, ChangeType.Remove, name, trimmed, "", source, position);
                continue;
            }

            replacements.Add("[" + trimmed + "]");
        }

        if (replacements.Count == 0)
        {
            return "";
        }

        return string.Join(Environment.NewLine, replacements);
    }

    private static List<string> ConvertHeader(string attribute)
    {
        List<string> arguments = GetArguments(attribute);
        List<string> result = new();

        if (arguments.Count == 0)
        {
            result.Add("[Header]");
            return result;
        }

        result.Add($"[Header({arguments[0]})]");

        if (arguments.Count >= 2)
        {
            result.Add($"[Space({arguments[1]})]");
        }

        return result;
    }

    private static string ConvertSimpleAttribute(string attribute, string replacement)
    {
        int parenthesis = attribute.IndexOf('(');

        if (parenthesis < 0)
        {
            return $"[{replacement}]";
        }

        string arguments = attribute.Substring(parenthesis);

        return $"[{replacement}{arguments}]";
    }

    private static List<string> GetArguments(string attribute)
    {
        List<string> arguments = new();
        int start = attribute.IndexOf('(');

        if (start < 0)
        {
            return arguments;
        }

        int end = attribute.LastIndexOf(')');

        if (end <= start)
        {
            return arguments;
        }

        string contents = attribute.Substring(start + 1, end - start - 1);
        int argumentStart = 0;
        int parentheses = 0;
        bool inString = false;

        for (int i = 0; i < contents.Length; i++)
        {
            char current = contents[i];

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
                continue;
            }

            if (current == ')')
            {
                parentheses--;
                continue;
            }

            if (current == ',' && parentheses == 0)
            {
                arguments.Add(contents.Substring(argumentStart, i - argumentStart).Trim());
                argumentStart = i + 1;
            }
        }

        string finalArgument = contents.Substring(argumentStart).Trim();

        if (!string.IsNullOrEmpty(finalArgument))
        {
            arguments.Add(finalArgument);
        }

        return arguments;
    }

    private static List<string> SplitAttributes(string text)
    {
        List<string> result = new();
        int start = 0;
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
                continue;
            }

            if (current == ')')
            {
                parentheses--;
                continue;
            }

            if (current == ',' && parentheses == 0)
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
        string name = attribute.Trim();
        int parenthesis = name.IndexOf('(');

        if (parenthesis >= 0)
        {
            name = name.Substring(0, parenthesis).Trim();
        }

        int space = name.IndexOf(' ');

        if (space >= 0)
        {
            name = name.Substring(0, space).Trim();
        }

        int dot = name.LastIndexOf('.');

        if (dot >= 0)
        {
            name = name.Substring(dot + 1);
        }

        if (name.EndsWith("Attribute", StringComparison.Ordinal))
        {
            name = name.Substring(0, name.Length - "Attribute".Length);
        }

        return name;
    }

    private static int FindNextAttribute(string source, int start)
    {
        bool inString = false;
        bool inCharacter = false;
        bool inLineComment = false;
        bool inBlockComment = false;

        for (int i = start; i < source.Length; i++)
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

            if (current == '[')
            {
                return i;
            }
        }

        return -1;
    }

    private static int FindAttributeEnd(string source, int start)
    {
        int depth = 0;
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
                depth++;
                continue;
            }

            if (current == ']')
            {
                depth--;

                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static void AddChange(Result result, ChangeType type, string name, string original, string replacement, string source, int position)
    {
        result.Changes.Add(new Change
        {
            Type = type,
            AttributeName = name,
            OriginalText = original,
            ReplacementText = replacement,
            Line = GetLine(source, position)
        });
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