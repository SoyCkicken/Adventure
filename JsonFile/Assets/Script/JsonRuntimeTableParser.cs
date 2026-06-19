using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class JsonRuntimeTableParser
{
    public static bool TryGetRootArray(string jsonContent, string rootKey, out JArray array, out string error)
    {
        array = null;
        error = null;

        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            error = "jsonContent is empty.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(rootKey))
        {
            error = "rootKey is empty.";
            return false;
        }

        if (!TryParseObject(jsonContent, out JObject root, out string parseError))
        {
            string repairedJson = RepairLineTerminatedStrings(jsonContent);
            string repairedError = null;
            if (ReferenceEquals(repairedJson, jsonContent) ||
                !TryParseObject(repairedJson, out root, out repairedError))
            {
                string tableError = null;
                if (TryParseGeneratedTableArray(jsonContent, rootKey, out array, out tableError))
                {
                    return true;
                }

                error = ReferenceEquals(repairedJson, jsonContent)
                    ? parseError
                    : $"{parseError}; repair fallback failed: {repairedError}; table fallback failed: {tableError}";
                return false;
            }
        }

        if (!root.TryGetValue(rootKey, out JToken token))
        {
            error = $"Root key '{rootKey}' was not found.";
            return false;
        }

        if (token.Type != JTokenType.Array)
        {
            error = $"Root key '{rootKey}' is {token.Type}, not Array.";
            return false;
        }

        array = (JArray)token;
        return true;
    }

    public static bool TryParseList<T>(
        string jsonContent,
        string rootKey,
        out List<T> items,
        out string error,
        Dictionary<string, string> fieldAliases = null)
    {
        items = null;

        if (!TryGetRootArray(jsonContent, rootKey, out JArray array, out error))
        {
            return false;
        }

        try
        {
            string wrappedJson = WrapJsonArray(array, fieldAliases);
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            items = wrapper?.items ?? new List<T>();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static string WrapJsonArray(string jsonArray)
    {
        return "{\"items\":" + jsonArray + "}";
    }

    public static string WrapJsonArray(JArray jsonArray, Dictionary<string, string> fieldAliases = null)
    {
        JArray normalizedArray = fieldAliases == null || fieldAliases.Count == 0
            ? jsonArray
            : NormalizeFieldAliases(jsonArray, fieldAliases);

        return WrapJsonArray(normalizedArray.ToString(Formatting.None));
    }

    private static JArray NormalizeFieldAliases(JArray jsonArray, Dictionary<string, string> fieldAliases)
    {
        var normalizedArray = new JArray();

        foreach (JToken token in jsonArray)
        {
            JObject source = token as JObject;
            if (source == null)
            {
                normalizedArray.Add(token.DeepClone());
                continue;
            }

            var target = (JObject)source.DeepClone();
            foreach (var alias in fieldAliases)
            {
                if (target.TryGetValue(alias.Key, out JToken value) && !target.ContainsKey(alias.Value))
                {
                    target[alias.Value] = value.DeepClone();
                }
            }

            normalizedArray.Add(target);
        }

        return normalizedArray;
    }

    private static bool TryParseObject(string jsonContent, out JObject root, out string error)
    {
        root = null;
        error = null;

        try
        {
            root = JObject.Parse(jsonContent);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static string RepairLineTerminatedStrings(string jsonContent)
    {
        string normalized = jsonContent.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] lines = normalized.Split('\n');
        bool changed = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (CountUnescapedQuotes(line) % 2 == 0)
            {
                continue;
            }

            string trimmed = line.TrimEnd();
            string trailingWhitespace = line.Substring(trimmed.Length);

            if (trimmed.EndsWith(",", StringComparison.Ordinal))
            {
                lines[i] = trimmed.Substring(0, trimmed.Length - 1) + "\"," + trailingWhitespace;
                changed = true;
                continue;
            }

            lines[i] = trimmed + "\"" + trailingWhitespace;
            changed = true;
        }

        return changed ? string.Join("\n", lines) : jsonContent;
    }

    private static int CountUnescapedQuotes(string line)
    {
        int count = 0;
        bool escaped = false;

        foreach (char c in line)
        {
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                count++;
            }
        }

        return count;
    }

    private static bool TryParseGeneratedTableArray(string jsonContent, string rootKey, out JArray array, out string error)
    {
        array = new JArray();
        error = null;

        bool foundRoot = false;
        bool inObject = false;
        JObject current = null;

        using (var reader = new StringReader(jsonContent))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                if (!foundRoot)
                {
                    foundRoot = trimmed.StartsWith($"\"{rootKey}\"", StringComparison.Ordinal) &&
                                trimmed.Contains("[");
                    continue;
                }

                if (!inObject)
                {
                    if (trimmed.StartsWith("]", StringComparison.Ordinal))
                    {
                        break;
                    }

                    if (trimmed.StartsWith("{", StringComparison.Ordinal))
                    {
                        current = new JObject();
                        inObject = true;
                    }

                    continue;
                }

                if (trimmed.StartsWith("}", StringComparison.Ordinal))
                {
                    array.Add(current);
                    current = null;
                    inObject = false;
                    continue;
                }

                if (TryParseLooseProperty(trimmed, out string key, out JToken value))
                {
                    current[key] = value;
                }
            }
        }

        if (!foundRoot)
        {
            error = $"Root key '{rootKey}' was not found.";
            return false;
        }

        if (inObject)
        {
            error = $"Root key '{rootKey}' ended inside an object.";
            return false;
        }

        return true;
    }

    private static bool TryParseLooseProperty(string line, out string key, out JToken value)
    {
        key = null;
        value = null;

        if (!line.StartsWith("\"", StringComparison.Ordinal))
        {
            return false;
        }

        int keyEnd = FindNextUnescapedQuote(line, 1);
        if (keyEnd < 0)
        {
            return false;
        }

        int colon = line.IndexOf(':', keyEnd + 1);
        if (colon < 0)
        {
            return false;
        }

        key = line.Substring(1, keyEnd - 1);
        string valueText = line.Substring(colon + 1).Trim();
        if (valueText.EndsWith(",", StringComparison.Ordinal))
        {
            valueText = valueText.Substring(0, valueText.Length - 1).TrimEnd();
        }

        value = ParseLooseValue(valueText);
        return true;
    }

    private static JToken ParseLooseValue(string valueText)
    {
        if (string.IsNullOrEmpty(valueText))
        {
            return JValue.CreateString(string.Empty);
        }

        if (string.Equals(valueText, "null", StringComparison.OrdinalIgnoreCase))
        {
            return JValue.CreateNull();
        }

        if (bool.TryParse(valueText, out bool boolValue))
        {
            return new JValue(boolValue);
        }

        if (double.TryParse(valueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double numberValue))
        {
            return valueText.Contains(".")
                ? new JValue(numberValue)
                : new JValue((int)numberValue);
        }

        if (valueText.StartsWith("\"", StringComparison.Ordinal))
        {
            string text = valueText.Substring(1);
            if (text.EndsWith("\"", StringComparison.Ordinal))
            {
                text = text.Substring(0, text.Length - 1);
            }

            return JValue.CreateString(DecodeLooseString(text));
        }

        return JValue.CreateString(valueText);
    }

    private static int FindNextUnescapedQuote(string text, int startIndex)
    {
        bool escaped = false;
        for (int i = startIndex; i < text.Length; i++)
        {
            char c = text[i];
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                return i;
            }
        }

        return -1;
    }

    private static string DecodeLooseString(string text)
    {
        return text
            .Replace("\\r", "\r")
            .Replace("\\n", "\n")
            .Replace("\\t", "\t")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }
}
