using System.Collections.Generic;

internal static class NetstandardCompatibility
{
    public static void Deconstruct<T1, T2>(
        this KeyValuePair<T1, T2> keyValuePair,
        out T1 key,
        out T2 value)
    {
        key = keyValuePair.Key;
        value = keyValuePair.Value;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dict,
        TKey key,
        TValue defaultValue = default)
    {
        if (dict.TryGetValue(key, out var value))
            return value;
        return defaultValue!;
    }
}
