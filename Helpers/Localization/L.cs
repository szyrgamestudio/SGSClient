namespace SGSClient.Helpers;
public static class L
{
    // podstawowy getter
    public static string p(string key)
    {
        return LocalizedText.Instance[key];
    }

    // formatowanie
    public static string pf(string key, params object[] args)
    {
        string baseText = LocalizedText.Instance[key];
        return string.Format(baseText, args);
    }

    // fallback gdy brak klucza
    public static string pOr(string key, string fallback)
    {
        string result = LocalizedText.Instance[key];

        // jeśli LocalizedText zwróci "[key]" → brak tłumaczenia
        if (result == $"[{key}]")
            return fallback;

        return result;
    }

    // wersja async – na wypadek późniejszych źródeł
    public static Task<string> pAsync(string key)
    {
        return Task.FromResult(LocalizedText.Instance[key]);
    }

    // async + format
    public static async Task<string> pfAsync(string key, params object[] args)
    {
        string baseText = await pAsync(key);
        return string.Format(baseText, args);
    }
}
