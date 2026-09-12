using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class NetworkClientConfig
{
    public const string FileName = "client-config.json";
    public const string DefaultServerUrl = "https://slayqueen-multiplayer-yusuken64.azurewebsites.net";

    public static string ConfigPath
    {
        get
        {
            // Windows/Linux: beside the executable. Editor: project root.
            var directory = Directory.GetParent(Application.dataPath).FullName;
            // macOS dataPath is inside the .app bundle; keep the config beside it.
            if (Application.platform == RuntimePlatform.OSXPlayer)
                directory = Directory.GetParent(directory).FullName;
            return Path.Combine(directory, FileName);
        }
    }

    // NetworkManager reads this once when Common initializes it at startup.
    public static string ResolveServerUrl(string fallbackUrl)
    {
        try
        {
            var path = ConfigPath;
            if (!File.Exists(path)) return fallbackUrl;

            var config = JObject.Parse(File.ReadAllText(path));
            var token = config["serverUrl"];
            if (token == null) return fallbackUrl;
            var value = token.Type == JTokenType.String ? ((string)token).Trim() : null;
            if (!TryNormalizeServerUrl(value, out var serverUrl))
            {
                Debug.LogWarning($"Invalid serverUrl in {path}. Use an HTTP(S) base URL. Using the default server.");
                return fallbackUrl;
            }

            return serverUrl;
        }
        catch (Exception exception) when (exception is IOException ||
            exception is UnauthorizedAccessException || exception is JsonException)
        {
            Debug.LogWarning($"Could not read {FileName}: {exception.Message}. Using the default server.");
            return fallbackUrl;
        }
    }

    internal static bool TryNormalizeServerUrl(string value, out string serverUrl)
    {
        serverUrl = null;
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            string.IsNullOrEmpty(uri.Host) || !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.UserInfo))
            return false;

        serverUrl = uri.AbsoluteUri.TrimEnd('/');
        return true;
    }
}
