using System;
using System.Collections.Generic;
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

        try
        {
            JObject root = JObject.Parse(jsonContent);
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
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
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
            if (token is not JObject source)
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
}
